// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Api.Configuration.Services.Twilio;

namespace Api.Configuration.Extensions.Twilio;

/// <summary>
/// TwilioConfigServiceExtensions
/// </summary>
public static class TwilioConfigServiceExtensions
{
    /// <summary>
    /// Add the TwilioConfigService service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTwilioConfigService(this IServiceCollection services)
    {
        services.AddScoped<ITwilioConfigService, TwilioConfigService>();
        return services;
    }
}
