// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Repositories.Acyclica;

namespace Api.Configuration.Extensions.Acyclica;

/// <summary>
/// AcyclicaConfigRepositoryExtensions
/// </summary>
public static class AcyclicaConfigRepositoryExtensions
{
    /// <summary>
    /// Add the AcyclicaConfigRepository service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAcyclicaConfigRepository(this IServiceCollection services)
    {
        services.AddScoped<IAcyclicaConfigRepository, AcyclicaConfigRepository>();
        return services;
    }
}
