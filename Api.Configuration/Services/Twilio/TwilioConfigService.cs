// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Twilio;
using Api.Configuration.Repositories.Twilio;

namespace Api.Configuration.Services.Twilio;

/// <summary>
/// TwilioConfigService
/// </summary>
public class TwilioConfigService : ITwilioConfigService
{
    private readonly ITwilioConfigRepository _twilioConfigRepository;

    /// <summary>
    /// TwilioConfigService
    /// </summary>
    /// <param name="twilioConfigRepository"></param>
    public TwilioConfigService(ITwilioConfigRepository twilioConfigRepository)
    {
        _twilioConfigRepository = twilioConfigRepository;
    }

    /// <summary>
    /// Get the Twilio Configuration
    /// </summary>
    /// <returns></returns>
    public async Task<TwilioConfigDto> GetAsync()
    {
        var twilioConfig = await _twilioConfigRepository.FindOneAsync();
        if (twilioConfig == null)
        {
            twilioConfig = new TwilioConfigDto
            {
                Id = Guid.NewGuid(),
                AccountSid = string.Empty,
                AuthToken = string.Empty,
                SenderPhone = string.Empty,
            };
        }

        return twilioConfig;
    }

    /// <summary>
    /// Create the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    public async Task CreateAsync(TwilioConfigDto twilioConfigDto)
    {
        var twilioConfig = await _twilioConfigRepository.FindOneAsync();
        if (twilioConfig == null)
        {
            await _twilioConfigRepository.InsertOneAsync(twilioConfigDto);
        }
        else
        {
            await _twilioConfigRepository.FindOneAndReplaceAsync(twilioConfigDto);
        }
    }

    /// <summary>
    /// Update the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    public async Task UpdateAsync(TwilioConfigDto twilioConfigDto)
    {
        var twilioConfig = await _twilioConfigRepository.FindOneAsync();
        if (twilioConfig == null)
        {
            await _twilioConfigRepository.InsertOneAsync(twilioConfigDto);
        }
        else
        {
            await _twilioConfigRepository.FindOneAndReplaceAsync(twilioConfigDto);
        }
    }

    /// <summary>
    /// Delete the Twilio Configuration
    /// </summary>
    /// <returns></returns>
    public async Task DeleteAsync(TwilioConfigDto twilioConfigDto)
    {
        await _twilioConfigRepository.FindOneAndDeleteAsync(twilioConfigDto);
    }
}
