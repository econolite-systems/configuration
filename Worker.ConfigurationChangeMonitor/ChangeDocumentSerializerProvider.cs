// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Econolite.Ode.Worker.ConfigurationChangeMonitor;

public class ChangeDocumentSerializerProvider : IBsonSerializationProvider
{
    private static readonly ChangeStreamDocumentSerializer<BsonDocument> Serializer =
        new(BsonDocumentSerializer.Instance);

    public IBsonSerializer? GetSerializer(Type type)
    {
        return type == typeof(ChangeStreamDocument<BsonDocument>) ? Serializer : null;
    }
}
