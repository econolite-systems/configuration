// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Collections.Immutable;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.Configuration;
using Econolite.Ode.Persistence.Mongo.Client;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Econolite.Ode.Worker.ConfigurationChangeMonitor;

public class ConfigurationChangeWorker : BackgroundService
{
    private const string ResumeTokenKey = "ConfigurationChangeResumeToken";

    private readonly ILogger<ConfigurationChangeWorker> _logger;
    private readonly IClientProvider _clientProvider;
    private readonly IReadOnlyDictionary<string, ConfigurationCategory> _configurationCategories;
    private readonly IDatabase _redisDb;
    private readonly string _updateTopic;
    private readonly IProducer<Guid, ConfigurationUpdateMessage> _updateProducer;
    private readonly IMessageFactory<Guid, ConfigurationUpdateMessage> _updateMessageBuilder;
    private readonly IImmutableList<string> _watchCollections;
    
    public ConfigurationChangeWorker(
        IClientProvider clientProvider,
        IProducer<Guid, ConfigurationUpdateMessage> updateProducer,
        IMessageFactory<Guid, ConfigurationUpdateMessage> updateMessageBuilder,
        IConfiguration config,
        ILogger<ConfigurationChangeWorker> logger
    )
    {
        _logger = logger;
        _clientProvider = clientProvider;

        _configurationCategories = _makeConfigurationCategoryDict(config);

        var redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"));
        _redisDb = redis.GetDatabase();
        _updateTopic = config["Topics:ConfigurationUpdate"] ?? throw new NullReferenceException("Topics:ConfigurationUpdate missing in config");
        _updateProducer = updateProducer;
        _updateMessageBuilder = updateMessageBuilder;

        _watchCollections = config.GetSection("WatchCollections").Get<List<string>>()?.Select(c => config[$"Collections:{c}"]).ToImmutableArray() ?? throw new NullReferenceException("WatchCollections null in config");

        // Ensure all the watched collections have a configuration category mapping
        var topicCategories = _configurationCategories.Keys.ToImmutableHashSet();
        if (!topicCategories.SetEquals(_watchCollections))
        {
            var missingCollections = _watchCollections.ToImmutableHashSet().Except(topicCategories);
            throw new Exception($"Missing configuration category for: {string.Join(',', missingCollections)}");
        }
    }

    private static IReadOnlyDictionary<string, ConfigurationCategory> _makeConfigurationCategoryDict(
        IConfiguration config)
    {
        return new Dictionary<string, ConfigurationCategory>
        {
            [config["Collections:EnvironmentalSensor"] ?? throw new NullReferenceException("Collections:EnvironmentalSensor missing in config")] = ConfigurationCategory.EnvironmentalSensor,
            [config["Collections:LogicStatement"] ?? throw new NullReferenceException("Collections:LogicStatement missing in config")] = ConfigurationCategory.LogicStatement,
        };
    }

    private ConfigurationCategory _getCategory(string collection)
    {
        return _configurationCategories[collection];
    }

    private async Task _publishCollectionInvalidation()
    {
        await _updateProducer.ProduceAsync(
            _updateTopic,
            _updateMessageBuilder.Build(Guid.NewGuid(), new ConfigurationInvalidated()));
    }

    private async Task _broadcastInsert(string collection, Guid id)
    {
        await _updateProducer.ProduceAsync(
            _updateTopic,
            _updateMessageBuilder.Build(id, new ConfigurationCreated(_getCategory(collection), id)));
    }

    private async Task _broadcastUpdate(string collection, Guid id)
    {
        await _updateProducer.ProduceAsync(
            _updateTopic,
            _updateMessageBuilder.Build(id, new ConfigurationChanged(_getCategory(collection), id)));
    }

    private async Task _broadcastDelete(string collection, Guid id)
    {
        await _updateProducer.ProduceAsync(
            _updateTopic,
            _updateMessageBuilder.Build(id, new ConfigurationDeleted(_getCategory(collection), id)));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        BsonDocument? resumeToken = default;

        _logger.LogDebug("Fetching resume token");
        var oldResumeToken = await _redisDb.StringGetAsync(ResumeTokenKey);
        if (oldResumeToken.IsNullOrEmpty)
        {
            _logger.LogInformation($"No resume token found");
        }
        else
        {
            _logger.LogInformation("Resume token found");
            try
            {
                resumeToken = oldResumeToken.HasValue
                    ? BsonSerializer.Deserialize<BsonDocument>((byte[])oldResumeToken)
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    $"Exception while deserializing cached resume token, configuration caches will be invalidated");
            }
        }

        try
        {
            var changeStreamOptions = new ChangeStreamOptions
            {
                StartAfter = resumeToken,
            };

            var changeStreamPipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
                .Match(new BsonDocument("$expr",
                    new BsonDocument("$in",
                        new BsonArray(new BsonValue[] { "$ns.coll", new BsonArray(_watchCollections) }))));

            using var cursor =
                await _clientProvider.Database!.WatchAsync(changeStreamPipeline, changeStreamOptions, stoppingToken);

            if (resumeToken is null)
            {
                _logger.LogWarning("Broadcasting configuration invalidation to all services");
                await _publishCollectionInvalidation();
            }

            await cursor.ForEachAsync(async change =>
                {
                    var collection = change.CollectionNamespace.CollectionName;

                    if (!change.DocumentKey.TryGetValue("_id", out var idValue))
                    {
                        _logger.LogError("Document key was missing the '_id' field: {}", change.ToJson());
                        return;
                    }

                    if (idValue is not BsonBinaryData)
                    {
                        _logger.LogError("Document key was not BSON binary data: {} {} for change {}",
                            idValue.GetType().Name,
                            idValue, change.ToJson());
                        return;
                    }

                    var id = Guid.Empty;
                    try
                    {
                        id = new Guid(idValue.AsByteArray);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "Unable to create GUID from ID bytes: {}", idValue);
                        return;
                    }

                    try
                    {
                        switch (change.OperationType)
                        {
                            case ChangeStreamOperationType.Delete:
                            {
                                await _broadcastDelete(collection, id);

                                break;
                            }

                            case ChangeStreamOperationType.Insert:
                            {
                                await _broadcastInsert(collection, id);

                                break;
                            }

                            case ChangeStreamOperationType.Replace:
                            case ChangeStreamOperationType.Update:
                            {
                                await _broadcastUpdate(collection, id);

                                break;
                            }

                            case ChangeStreamOperationType.Invalidate:
                            case ChangeStreamOperationType.Rename:
                            case ChangeStreamOperationType.Drop:
                            default:
                            {
                                _logger.LogDebug("Unhandled change operation for collection '{}': {} - {}",
                                    change.CollectionNamespace.FullName, change.OperationType, change.ToJson());
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to broadcast configuration change: {}", change.ToJson());
                        return;
                    }

                    try
                    {
                        resumeToken = change.ResumeToken;
                        if (resumeToken is not null)
                        {
                            await _redisDb.StringSetAsync(ResumeTokenKey, resumeToken.ToBson());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save resume token");
                    }
                },
                stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker.ConfigurationChangeMonitor stopping");
        }
    }
}
