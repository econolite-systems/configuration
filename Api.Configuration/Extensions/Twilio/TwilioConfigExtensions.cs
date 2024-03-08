// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Twilio;

namespace Api.Configuration.Extensions.Twilio;

/// <summary>
/// TwilioConfigExtensions
/// </summary>
public static class TwilioConfigExtensions
{
    /// <summary>
    /// ToDto
    /// </summary>
    /// <param name="twilioConfigDoc"></param>
    /// <returns></returns>
    public static TwilioConfigDto ToDto(this TwilioConfigDoc twilioConfigDoc)
    {
        return new TwilioConfigDto
        {
            Id = twilioConfigDoc.Id,
            AccountSid = twilioConfigDoc.AccountSid,
            AuthToken = twilioConfigDoc.AuthToken,
            SenderPhone = twilioConfigDoc.SenderPhone,
        };
    }

    /// <summary>
    /// ToDoc
    /// </summary>
    /// <param name="twilioConfigDto"></param>
    /// <returns></returns>
    public static TwilioConfigDoc ToDoc(this TwilioConfigDto twilioConfigDto)
    {
        return new TwilioConfigDoc
        {
            Id = twilioConfigDto.Id,
            AccountSid = twilioConfigDto.AccountSid,
            AuthToken = twilioConfigDto.AuthToken,
            SenderPhone = twilioConfigDto.SenderPhone,
        };
    }
}
