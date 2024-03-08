// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Twilio;

namespace Api.Configuration.Services.Twilio;

/// <summary>
/// ITwilioConfigService
/// </summary>
public interface ITwilioConfigService
{
    /// <summary>
    /// Get the Twilio Configuration
    /// </summary>
    /// <returns></returns>
    Task<TwilioConfigDto> GetAsync();

    /// <summary>
    /// Create the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    Task CreateAsync(TwilioConfigDto twilioConfigDto);

    /// <summary>
    /// Update the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    Task UpdateAsync(TwilioConfigDto twilioConfigDto);

    /// <summary>
    /// Delete the Twilio Configuration
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    Task DeleteAsync(TwilioConfigDto twilioConfigDto);
}
