// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Configuration.Models.Twilio;

/// <summary>
/// TwilioConfigDoc
/// </summary>
public class TwilioConfigDoc
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// AccountSid
    /// </summary>
    [BsonElement("AccountSid")]
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// AuthToken
    /// </summary>
    [BsonElement("AuthToken")]
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// SenderPhone
    /// </summary>
    [BsonElement("SenderPhone")]
    public string SenderPhone { get; set; } = string.Empty;
}
