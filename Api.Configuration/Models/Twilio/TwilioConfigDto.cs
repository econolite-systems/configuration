// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Api.Configuration.Models.Twilio;

/// <summary>
/// TwilioConfigDto
/// </summary>
public class TwilioConfigDto
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// AccountSid
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// AuthToken
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// SenderPhone
    /// </summary>
    public string SenderPhone { get; set; } = string.Empty;
}
