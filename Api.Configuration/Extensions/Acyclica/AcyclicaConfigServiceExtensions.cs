// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Services.Acyclica;

namespace Api.Configuration.Extensions.Acyclica;

/// <summary>
/// AcyclicaConfigServiceExtensions
/// </summary>
public static class AcyclicaConfigServiceExtensions
{
    /// <summary>
    /// Add the AcyclicaConfigService service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAcyclicaConfigService(this IServiceCollection services)
    {
        services.AddScoped<IAcyclicaConfigService, AcyclicaConfigService>();
        return services;
    }
}
