// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Text;
using System.Text.Json;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;

namespace Econolite.Ode.Models.Configuration;

public class ConfigurationUpdateConsumeResultFactory : IConsumeResultFactory<Guid, ConfigurationUpdateMessage>
{
    public ConsumeResult<Guid, ConfigurationUpdateMessage> BuildConsumeResult(Confluent.Kafka.ConsumeResult<byte[], byte[]> consumeResult)
    {
        var tenantId = Guid.Empty;
        if (consumeResult.Message.Headers.TryGetLastBytes(Consts.TENANT_ID_HEADER, out var buffer))
            _ = Guid.TryParse(Encoding.UTF8.GetString(buffer), out tenantId);

        var type = Consts.TYPE_UNSPECIFIED;
        if (consumeResult.Message.Headers.TryGetLastBytes(Consts.TYPE_HEADER, out buffer))
            type = Encoding.UTF8.GetString(buffer);

        Guid? deviceId = default;
        if (consumeResult.Message.Headers.TryGetLastBytes(Consts.DEVICE_ID_HEADER, out buffer))
            if (Guid.TryParse(Encoding.UTF8.GetString(buffer), out var deviceGuid))
                deviceId = deviceGuid;
        return new ConsumeResult<Guid, ConfigurationUpdateMessage>(tenantId, deviceId, type, consumeResult, _ => Guid.Parse(Encoding.UTF8.GetString(_)), new ConfigurationUpdateMessageParser(type));
    }
}

public class ConfigurationUpdateMessageParser : IPayloadSpecialist<ConfigurationUpdateMessage>
{
    private readonly string _type;
    public ConfigurationUpdateMessageParser(string type)
    {
        _type = type;
    }
    
    public ConfigurationUpdateMessage To(Confluent.Kafka.ConsumeResult<byte[], byte[]> consumeResult)
    {
        var data = Encoding.UTF8.GetString(consumeResult.Message.Value);
        try
        {
            return _type switch
            {
                nameof(ConfigurationCreated) => JsonSerializer.Deserialize<ConfigurationCreated>(data)!,
                nameof(ConfigurationChanged) => JsonSerializer.Deserialize<ConfigurationChanged>(data)!,
                nameof(ConfigurationDeleted) => JsonSerializer.Deserialize<ConfigurationDeleted>(data)!,
                nameof(ConfigurationInvalidated) => JsonSerializer.Deserialize<ConfigurationInvalidated>(data)!,

                _ => new UnknownConfigurationUpdateMessage(_type, data)
            };
        }
        catch (Exception ex)
        {
            return new NonParseableConfigurationUpdateMessage(_type, data, ex);
        }
    }

    public TDerived To<TDerived>(Confluent.Kafka.ConsumeResult<byte[], byte[]> consumeResult)
    {
        throw new NotImplementedException();
    }
}
