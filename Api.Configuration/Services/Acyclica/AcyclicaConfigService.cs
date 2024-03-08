// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Acyclica;
using Api.Configuration.Repositories.Acyclica;

namespace Api.Configuration.Services.Acyclica;

/// <summary>
/// AcyclicaConfigService
/// </summary>
public class AcyclicaConfigService : IAcyclicaConfigService
{
    private readonly IAcyclicaConfigRepository _acyclicaConfigRepository;

    /// <summary>
    /// AcyclicaConfigService
    /// </summary>
    /// <param name="acyclicaConfigRepository"></param>
    public AcyclicaConfigService(IAcyclicaConfigRepository acyclicaConfigRepository)
    {
        _acyclicaConfigRepository = acyclicaConfigRepository;
    }

    /// <summary>
    /// Get the Acyclica Configuration
    /// </summary>
    /// <returns></returns>
    public async Task<AcyclicaConfigDto> GetAsync()
    {
        var acyclicaConfig = await _acyclicaConfigRepository.FindOneAsync();
        if (acyclicaConfig == null)
        {
            acyclicaConfig = new AcyclicaConfigDto
            {
                Id = Guid.NewGuid(),
                Url = string.Empty,
                ApiKey = string.Empty,
                PollInterval = 60,
            };
        }

        return acyclicaConfig;
    }

    /// <summary>
    /// Create the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    public async Task CreateAsync(AcyclicaConfigDto acyclicaConfigDto)
    {
        var acyclicaConfig = await _acyclicaConfigRepository.FindOneAsync();
        if (acyclicaConfig == null)
        {
            await _acyclicaConfigRepository.InsertOneAsync(acyclicaConfigDto);
        }
        else
        {
            await _acyclicaConfigRepository.FindOneAndReplaceAsync(acyclicaConfigDto);
        }
    }

    /// <summary>
    /// Update the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    public async Task UpdateAsync(AcyclicaConfigDto acyclicaConfigDto)
    {
        var acyclicaConfig = await _acyclicaConfigRepository.FindOneAsync();
        if (acyclicaConfig == null)
        {
            await _acyclicaConfigRepository.InsertOneAsync(acyclicaConfigDto);
        }
        else
        {
            await _acyclicaConfigRepository.FindOneAndReplaceAsync(acyclicaConfigDto);
        }
    }

    /// <summary>
    /// Delete the Acyclica Configuration
    /// </summary>
    /// <returns></returns>
    public async Task DeleteAsync(AcyclicaConfigDto acyclicaConfigDto)
    {
        await _acyclicaConfigRepository.FindOneAndDeleteAsync(acyclicaConfigDto);
    }
}
