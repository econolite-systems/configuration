// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Models.Configuration;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Worker.ConfigurationChangeMonitor;
using MongoDB.Bson.Serialization;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builderContext, services) =>
    {
        services.AddMessaging();
        services.AddMongo();
        
        services.AddTransient<IProducer<Guid, ConfigurationUpdateMessage>, Producer<Guid, ConfigurationUpdateMessage>>();
        services.AddHostedService<ConfigurationChangeWorker>();
    })
    .Build();

await host.RunAsync();
