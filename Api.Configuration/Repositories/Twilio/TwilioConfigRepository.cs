// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Extensions.Twilio;
using Api.Configuration.Models.Twilio;
using Econolite.Ode.Persistence.Mongo.Context;
using MongoDB.Driver;

namespace Api.Configuration.Repositories.Twilio;

/// <summary>
/// TwilioConfigRepository
/// </summary>
public class TwilioConfigRepository : ITwilioConfigRepository
{
    private readonly ILogger<TwilioConfigRepository> _logger;
    private readonly IMongoCollection<TwilioConfigDoc> _twilioConfigCollection;

    /// <summary>
    /// TwilioConfigRepository
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="mongoContext"></param>
    public TwilioConfigRepository(IConfiguration configuration, ILogger<TwilioConfigRepository> logger, IMongoContext mongoContext)
    {
        _logger = logger;
        _twilioConfigCollection = mongoContext.GetCollection<TwilioConfigDoc>(configuration["Collections:TwilioConfig"] ?? "TwilioConfig");
    }

    /// <summary>
    /// FindOneAsync
    /// </summary>
    /// <returns></returns>
    public async Task<TwilioConfigDto?> FindOneAsync()
    {
        try
        {
            var filter = Builders<TwilioConfigDoc>.Filter.Empty;
            var cursor = await  _twilioConfigCollection.FindAsync(filter);
            return (await cursor.ToListAsync()).Select(x => x.ToDto()).SingleOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the Twilio Configuration");
            throw;
        }
    }

    /// <summary>
    /// InsertOneAsync
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    public async Task InsertOneAsync(TwilioConfigDto twilioConfigDto)
    {
        try
        {
            var twilioConfigDoc = twilioConfigDto.ToDoc();
            await _twilioConfigCollection.InsertOneAsync(twilioConfigDoc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create the Twilio Configuration");
            throw;
        }
    }

    /// <summary>
    /// FindOneAndReplaceAsync
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    public async Task FindOneAndReplaceAsync(TwilioConfigDto twilioConfigDto)
    {
        try
        {
            var twilioConfigDoc = twilioConfigDto.ToDoc();
            await _twilioConfigCollection.FindOneAndReplaceAsync(x => x.Id == twilioConfigDoc.Id, twilioConfigDoc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update the Twilio Configuration");
            throw;
        }
    }

    /// <summary>
    /// FindOneAndDeleteAsync
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    public async Task FindOneAndDeleteAsync(TwilioConfigDto twilioConfigDto)
    {
        try
        {
            var twilioConfigDoc = twilioConfigDto.ToDoc();
            await _twilioConfigCollection.FindOneAndDeleteAsync(x => x.Id == twilioConfigDoc.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the Twilio Configuration");
            throw;
        }
    }
}
