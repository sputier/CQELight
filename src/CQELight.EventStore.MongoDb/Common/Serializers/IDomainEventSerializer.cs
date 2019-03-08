using CQELight.Abstractions.Events.Interfaces;
using CQELight.Tools.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CQELight.EventStore.MongoDb.Common.Serializers
{
    internal class IDomainEventSerializer : SerializerBase<IDomainEvent>
    {

        #region Overriden methods

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IDomainEvent value)
        {
            context.Writer.WriteStartDocument();

            context.Writer.WriteName("_id");
            context.Writer.WriteObjectId(ObjectId.GenerateNewId());

            var eventType = value.GetType();
            context.Writer.WriteName("_t");
            context.Writer.WriteString(eventType.AssemblyQualifiedName);

            var provider = new PrimitiveSerializationProvider();

            foreach (var prop in Tools.Extensions.TypeExtensions.GetAllProperties(eventType))
            {
                context.Writer.WriteName(prop.Name);
                var propValue = prop.GetValue(value);
                if (propValue != null)
                {
                    if (propValue is Guid propValueGuid)
                    {
                        new GuidSerializer().Serialize(context, propValueGuid);
                    }
                    else if (propValue is Type propValueType)
                    {
                        new TypeSerializer().Serialize(context, propValueType);
                    }
                    else
                    {
                        var defaultSerializer = provider.GetSerializer(prop.PropertyType);
                        if (defaultSerializer != null)
                        {
                            defaultSerializer.Serialize(context, propValue);
                        }
                        else
                        {
                            new ObjectSerializer().Serialize(context, propValue);
                        }
                    }
                }
                else
                {
                    context.Writer.WriteNull();
                }
            }
            context.Writer.WriteEndDocument();
            //var o = new 
            //var jsonDocument = value.ToJson();
            //var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);
            //bsonDocument["_t"] = value.GetType().AssemblyQualifiedName;

            //var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
            //serializer.Serialize(context, bsonDocument.AsBsonValue);
        }

        public override IDomainEvent Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            context.Reader.ReadStartDocument();

            string name = string.Empty;
            IDomainEvent result = null;
            IEnumerable<PropertyInfo> properties = Enumerable.Empty<PropertyInfo>();
            var provider = new PrimitiveSerializationProvider();
            do
            {
                try
                {
                    name = context.Reader.ReadName();
                    if (name == "_t")
                    {
                        var eventTypeAsString = context.Reader.ReadString();
                        var type = Type.GetType(eventTypeAsString);
                        if (type != null)
                        {
                            result = type.CreateInstanceForce() as IDomainEvent;
                            properties = type.GetAllProperties();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (name == "_id")
                    {
                        context.Reader.ReadObjectId();
                    }
                    else
                    {
                        var prop = properties.FirstOrDefault(p => p.Name == name);
                        object data = null;
                        if (prop != null)
                        {
                            if (prop.PropertyType == typeof(Guid))
                            {
                                data = new GuidSerializer().Deserialize(context, args);
                            }
                            else if (prop.PropertyType == typeof(Type))
                            {
                                data = new TypeSerializer().Deserialize(context, args);
                            }
                            else
                            {
                                var defaultSerializer = provider.GetSerializer(prop.PropertyType);
                                if (defaultSerializer != null)
                                {
                                    data = defaultSerializer.Deserialize(context, args);
                                }
                                else
                                {
                                    data = new ObjectSerializer().Deserialize(context, args);
                                }
                            }
                            prop.SetValue(result, data);
                        }
                        else
                        {
                            context.Reader.ReadUndefined();
                        }
                    }
                }
                catch
                {
                    break;
                }
            }
            while (!string.IsNullOrWhiteSpace(name));
            context.Reader.ReadEndDocument();
            return result;
            //var serializer = BsonSerializer.LookupSerializer<BsonDocument>();
            //var document = serializer.Deserialize(context, args);

            //var type = Type.GetType(document["_t"].AsString);
            //if (type != null)
            //{
            //    document.Remove("_t");
            //    document.Remove("_id");
            //    var value = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            //    return Newtonsoft.Json.JsonConvert.DeserializeObject(value) as IDomainEvent;
            //}
            //return null;
        }

        #endregion

    }
}
