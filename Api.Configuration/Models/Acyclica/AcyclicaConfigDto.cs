// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Api.Configuration.Models.Acyclica;

/// <summary>
/// AcyclicaConfigDto
/// </summary>
public class AcyclicaConfigDto
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Url
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// ApiKey
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// PollInterval
    /// </summary>
    public int PollInterval { get; set; }
}
