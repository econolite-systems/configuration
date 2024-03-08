// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Configuration;

public abstract record ConfigurationUpdateMessage;

public sealed record UnknownConfigurationUpdateMessage(string Type, string Data) : ConfigurationUpdateMessage;

public sealed record NonParseableConfigurationUpdateMessage
    (string Type, string Data, Exception Exception) : ConfigurationUpdateMessage;

public sealed record ConfigurationCreated(ConfigurationCategory Category, Guid Id) : ConfigurationUpdateMessage;

public sealed record ConfigurationChanged(ConfigurationCategory Category, Guid Id) : ConfigurationUpdateMessage;

public sealed record ConfigurationDeleted(ConfigurationCategory Category, Guid Id) : ConfigurationUpdateMessage;

public sealed record ConfigurationInvalidated : ConfigurationUpdateMessage;
