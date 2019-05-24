using CQELight.Tools.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.DAL.MongoDb.Serializers
{
    internal class SerializedObject
    {
        public string Data { get; set; }
        public string Type { get; set; }
    }

    internal class ObjectSerializer : SerializerBase<object>
    {
        #region Overriden methods

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
            => context.Writer.WriteString(new SerializedObject { Data = value.ToJson(), Type = value.GetType().AssemblyQualifiedName }.ToJson());

        public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var objAsJson = context.Reader.ReadString();
            if (!string.IsNullOrWhiteSpace(objAsJson))
            {
                var serialized = objAsJson.FromJson<SerializedObject>();
                if (serialized != null)
                {
                    return serialized.Data.FromJson(Type.GetType(serialized.Type));
                }
            }
            return null;
        }

        #endregion

    }

}
