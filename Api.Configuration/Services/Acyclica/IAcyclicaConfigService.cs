// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Acyclica;

namespace Api.Configuration.Services.Acyclica;

/// <summary>
/// IAcyclicaConfigService
/// </summary>
public interface IAcyclicaConfigService
{
    /// <summary>
    /// Get the Acyclica Configuration
    /// </summary>
    /// <returns></returns>
    Task<AcyclicaConfigDto> GetAsync();

    /// <summary>
    /// Create the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    Task CreateAsync(AcyclicaConfigDto acyclicaConfigDto);

    /// <summary>
    /// Update the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    Task UpdateAsync(AcyclicaConfigDto acyclicaConfigDto);

    /// <summary>
    /// Delete the Acyclica Configuration
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    Task DeleteAsync(AcyclicaConfigDto acyclicaConfigDto);
}
