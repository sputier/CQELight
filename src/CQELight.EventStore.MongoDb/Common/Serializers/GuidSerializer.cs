using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQELight.EventStore.MongoDb.Common
{
    internal class GuidSerializer : SerializerBase<Guid>
    {
        #region Overriden methods

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Guid value)
            => context.Writer.WriteString(value.ToString());

        public override Guid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var guidAsString = context.Reader.ReadString();
            if (!string.IsNullOrWhiteSpace(guidAsString))
            {
                return Guid.Parse(guidAsString);
            }
            return Guid.Empty;
        }

        #endregion

    }
}
