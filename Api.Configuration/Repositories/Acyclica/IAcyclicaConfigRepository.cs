// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Models.Acyclica;

namespace Api.Configuration.Repositories.Acyclica;

/// <summary>
/// IAcyclicaConfigRepository
/// </summary>
public interface IAcyclicaConfigRepository
{
    /// <summary>
    /// FindOneAsync
    /// </summary>
    /// <returns></returns>
    Task<AcyclicaConfigDto?> FindOneAsync();

    /// <summary>
    /// InsertOneAsync
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    Task InsertOneAsync(AcyclicaConfigDto acyclicaConfigDto);

    /// <summary>
    /// FindOneAndReplaceAsync
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    Task FindOneAndReplaceAsync(AcyclicaConfigDto acyclicaConfigDto);

    /// <summary>
    /// FindOneAndDeleteAsync
    /// </summary>
    /// <param name="acyclicaConfigDto"></param>
    /// <returns></returns>
    Task FindOneAndDeleteAsync(AcyclicaConfigDto acyclicaConfigDto);
}
