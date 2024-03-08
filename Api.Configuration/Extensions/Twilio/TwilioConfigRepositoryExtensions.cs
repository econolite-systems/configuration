// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Repositories.Twilio;

namespace Api.Configuration.Extensions.Twilio;

/// <summary>
/// TwilioConfigRepositoryExtensions
/// </summary>
public static class TwilioConfigRepositoryExtensions
{
    /// <summary>
    /// Add the TwilioConfigRepository service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTwilioConfigRepository(this IServiceCollection services)
    {
        services.AddScoped<ITwilioConfigRepository, TwilioConfigRepository>();
        return services;
    }
}
