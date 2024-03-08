// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Extensions.Acyclica;
using Api.Configuration.Models.Acyclica;
using Econolite.Ode.Persistence.Mongo.Context;
using MongoDB.Driver;

namespace Api.Configuration.Repositories.Acyclica;

/// <summary>
/// AcyclicaConfigRepository
/// </summary>
public class AcyclicaConfigRepository : IAcyclicaConfigRepository
{
    private readonly ILogger<AcyclicaConfigRepository> _logger;
    private readonly IMongoCollection<AcyclicaConfigDoc> _acyclicaConfigCollection;

    /// <summary>
    /// AcyclicaConfigRepository
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="mongoContext"></param>
    public AcyclicaConfigRepository(IConfiguration configuration, ILogger<AcyclicaConfigRepository> logger, IMongoContext mongoContext)
    {
        _logger = logger;
        _acyclicaConfigCollection = mongoContext.GetCollection<AcyclicaConfigDoc>(configuration["Collections:AcyclicaConfig"] ?? "AcyclicaConfig");
    }

    /// <summary>
    /// FindOneAsync
    /// </summary>
    /// <returns></returns>
    public async Task<AcyclicaConfigDto?> FindOneAsync()
    {
        try
        {
            var filter = Builders<AcyclicaConfigDoc>.Filter.Empty;
            var cursor = await  _acyclicaConfigCollection.FindAsync(filter);
            return (await cursor.ToListAsync()).Select(x => x.ToDto()).SingleOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get the Acyclica Configuration");
            throw;
        }
    }

    /// <summary>
    /// InsertOneAsync
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    public async Task InsertOneAsync(AcyclicaConfigDto acyclicaConfigDto)
    {
        try
        {
            var acyclicaConfigDoc = acyclicaConfigDto.ToDoc();
            await _acyclicaConfigCollection.InsertOneAsync(acyclicaConfigDoc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create the Acyclica Configuration");
            throw;
        }
    }

    /// <summary>
    /// FindOneAndReplaceAsync
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    public async Task FindOneAndReplaceAsync(AcyclicaConfigDto acyclicaConfigDto)
    {
        try
        {
            var acyclicaConfigDoc = acyclicaConfigDto.ToDoc();
            await _acyclicaConfigCollection.FindOneAndReplaceAsync(x => x.Id == acyclicaConfigDoc.Id, acyclicaConfigDoc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update the Acyclica Configuration");
            throw;
        }
    }

    /// <summary>
    /// FindOneAndDeleteAsync
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    public async Task FindOneAndDeleteAsync(AcyclicaConfigDto acyclicaConfigDto)
    {
        try
        {
            var acyclicaConfigDoc = acyclicaConfigDto.ToDoc();
            await _acyclicaConfigCollection.FindOneAndDeleteAsync(x => x.Id == acyclicaConfigDoc.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete the Acyclica Configuration");
            throw;
        }
    }
}
