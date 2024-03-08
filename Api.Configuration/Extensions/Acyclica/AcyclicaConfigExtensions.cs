// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Acyclica;

namespace Api.Configuration.Extensions.Acyclica;

/// <summary>
/// AcyclicaConfigExtensions
/// </summary>
public static class AcyclicaConfigExtensions
{
    /// <summary>
    /// ToDto
    /// </summary>
    /// <param name="acyclicaConfigDoc"></param>
    /// <returns></returns>
    public static AcyclicaConfigDto ToDto(this AcyclicaConfigDoc acyclicaConfigDoc)
    {
        return new AcyclicaConfigDto
        {
            Id = acyclicaConfigDoc.Id,
            Url = acyclicaConfigDoc.Url,
            ApiKey = acyclicaConfigDoc.ApiKey,
            PollInterval = acyclicaConfigDoc.PollInterval,
        };
    }

    /// <summary>
    /// ToDoc
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    public static AcyclicaConfigDoc ToDoc(this AcyclicaConfigDto acyclicaConfigDto)
    {
        return new AcyclicaConfigDoc
        {
            Id = acyclicaConfigDto.Id,
            Url = acyclicaConfigDto.Url,
            ApiKey = acyclicaConfigDto.ApiKey,
            PollInterval = acyclicaConfigDto.PollInterval,
        };
    }
}
