// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Configuration.Snmp.Rsu.Extensions;
using Econolite.Dto.Repository.LogicStatement.Extensions;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization.Extensions;
using Econolite.Ode.Configuration.Rsu.Extensions;
using Econolite.Ode.Domain.DeviceManager;
using Econolite.Ode.Domain.Entities.Extensions;
using Econolite.Ode.Domain.SystemModeller;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.ConnectedVehicle;
using Econolite.Ode.Repository.DeviceManager;
using Econolite.Ode.Repository.Entities;
using Econolite.Ode.Repository.PavementCondition;
using Econolite.Ode.Repository.Rsu.Extensions;
using Econolite.Ode.Repository.WrongWayDriver;
using Econolite.Ode.Services.ConnectedVehicle;
using Econolite.Ode.Services.PavementCondition;
using Econolite.Ode.Services.WeatherResponsive;
using Econolite.Ode.Services.WrongWayDriver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using NetTopologySuite;
using NetTopologySuite.IO.Converters;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Econolite.Ode.Monitoring.HealthChecks.Kafka.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Api.Configuration.Extensions.Acyclica;
using Api.Configuration.Extensions.Twilio;
using Econolite.Ode.Config.Messaging.Extensions;
using Econolite.Ode.Domain.SystemModeller.Extensions;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Service.LogicStatement.Extensions;
using Monitoring.AspNet.Metrics;

var builder = WebApplication.CreateBuilder(args);
var RestrictedOrigins = "_restrictedOrigins";
var origins = builder.Configuration["CORSOrigins"];

Audit.Core.Configuration.Setup()
    .UseCustomProvider(new AuditMongoDataProvider(config => config
        .ConnectionString(builder.Configuration.GetConnectionString("Mongo"))
        .Database(builder.Configuration["Mongo:DbName"])
        .Collection(builder.Configuration["Collections:Audit"])
        // This is important!
        .SerializeAsBson(true)
    ));

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // this constructor is overloaded.  see other overloads for options.
        var geoJsonConverterFactory = new GeoJsonConverterFactory();
        options.JsonSerializerOptions.Converters.Add(geoJsonConverterFactory);
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddLogging();
builder.Services.AddAudit();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
#if DEBUG
    var basePath = AppDomain.CurrentDomain.BaseDirectory;
    var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
    options.IncludeXmlComments(Path.Combine(basePath, fileName));
#endif
});
builder.Services.AddMongo();
builder.Services
    .AddMetrics(builder.Configuration, "Configuration")
    .ConfigureRequestMetrics(c =>
    {
        c.RequestCounter = "Requests";
        c.ResponseCounter = "Responses";
    })
    .AddUserEventSupport(builder.Configuration, _ =>
    {
        _.DefaultSource = "Configuration";
        _.DefaultLogName = Econolite.Ode.Monitoring.Events.LogName.SystemEvent;
        _.DefaultCategory = Econolite.Ode.Monitoring.Events.Category.Server;
        _.DefaultTenantId = Guid.Empty;
    });
// TODO: Add middleware to track total API calls rather than adding counters to every single controller?

builder.Services.AddMessaging();
builder.Services.AddRsuRepo();
builder.Services.AddRsuService();
builder.Services.AddActionSetRepo();
builder.Services.AddActionSetService();
builder.Services.AddEntityRepo();
builder.Services.AddEntityService();
builder.Services.AddDmConfig((options =>
{
    options.DefaultChannel = builder.Configuration["Topics:ConfigResponseTopic"]!;
}, options =>
{
   options.DefaultChannel = builder.Configuration["Topics:ConfigRequestTopic"]!; 
}));
builder.Services.AddRemoteConfig(options =>
{
    options.DefaultChannel = builder.Configuration["Topics:ConfigResponseTopic"]!;
});
builder.Services.AddDmRepo();
builder.Services.AddDmService();
builder.Services.AddSystemModellerService();
builder.Services.AddSingleton(NtsGeometryServices.Instance);
builder.Services.AddPavementConditionConfigRepository();
builder.Services.AddPavementConditionConfigService();
builder.Services.AddPavementConditionStatusRepository();
builder.Services.AddPavementConditionStatusService();
builder.Services.AddConnectedVehicleConfigRepository();
builder.Services.AddConnectedVehicleConfigService();
builder.Services.AddWrongWayDriverConfigRepository();
builder.Services.AddWrongWayDriverConfigService();
builder.Services.AddWeatherResponsiveConfigurationSupport();
builder.Services.AddConfigRequestConsumers(builder.Configuration);
builder.Services.AddConfigProducer(options => options.ConfigResponseTopic = builder.Configuration["Topics:ConfigResponse"]!);
builder.Services.AddIntersectionConfigUpdate(options => options.DefaultChannel = builder.Configuration["Topics:ConfigIntersectionResponse"]!);
builder.Services.AddIntersectionConfigResponseWorker(options => 
    options.DefaultChannel = builder.Configuration["Topics:ConfigIntersectionResponse"] ?? 
                             throw new NullReferenceException("Topic:ConfigIntersectionResponse missing in config"),
    options => options.DefaultChannel = builder.Configuration["Topics:ConfigIntersectionRequest"] ??
                                        throw new NullReferenceException("Topic:ConfigIntersectionRequest missing in config"));
builder.Services.AddRsuConfigHandler();
builder.Services.AddAcyclicaConfigRepository();
builder.Services.AddAcyclicaConfigService();
builder.Services.AddTwilioConfigRepository();
builder.Services.AddTwilioConfigService();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: RestrictedOrigins,
        policy =>
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins(origins?.Split(';') ?? new[] { "*" })
                .AllowCredentials();
        });
});

builder.Services.AddMvc(config =>
{
    config.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddAuthenticationJwtBearer(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                },
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header,
            },
            new List<string>()
        },
    });
});

builder.Services
    .AddHealthChecks()
    .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 1024, name: "Process Allocated Memory", tags: new[] { "memory" })
    .AddMongoDbHealthCheck()
    .AddKafkaHealthCheck();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(RestrictedOrigins);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.AddRequestMetrics();
app.UseHealthChecksPrometheusExporter("/metrics");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapControllers();
});

app.Run();
