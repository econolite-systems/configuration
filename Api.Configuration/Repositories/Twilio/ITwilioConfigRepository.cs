// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Twilio;

namespace Api.Configuration.Repositories.Twilio;

/// <summary>
/// ITwilioConfigRepository
/// </summary>
public interface ITwilioConfigRepository
{
    /// <summary>
    /// FindOneAsync
    /// </summary>
    /// <returns></returns>
    Task<TwilioConfigDto?> FindOneAsync();

    /// <summary>
    /// InsertOneAsync
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    Task InsertOneAsync(TwilioConfigDto twilioConfigDto);

    /// <summary>
    /// FindOneAndReplaceAsync
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    Task FindOneAndReplaceAsync(TwilioConfigDto twilioConfigDto);

    /// <summary>
    /// FindOneAndDeleteAsync
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    Task FindOneAndDeleteAsync(TwilioConfigDto twilioConfigDto);
}
