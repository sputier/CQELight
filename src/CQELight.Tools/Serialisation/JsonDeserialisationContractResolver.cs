using CQELight.Tools.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.Tools.Serialisation
{
    /// <summary>
    /// Contrat de déserialisation Json.
    /// </summary>
    public class JsonDeserialisationContractResolver : DefaultContractResolver
    {
        #region Static members

        private static readonly List<IJsonContractDefinition> s_IJsonContractDefinitionCache = new List<IJsonContractDefinition>();
        private static readonly object s_lockObject = new object();

        #endregion

        #region Static methods


        static readonly IEnumerable<Type> s_ContractsType = ReflectionTools.GetAllTypes()
                .Where(m => m.GetInterfaces().Contains(typeof(IJsonContractDefinition)));

        private static IJsonContractDefinition GetOrCreateInstance(Type type)
        {
            lock (s_lockObject)
            {
                IJsonContractDefinition instance = s_IJsonContractDefinitionCache.Find(m => m.GetType() == type);
                if (instance == null)
                {
                    instance = (IJsonContractDefinition)type.CreateInstance();
                    s_IJsonContractDefinitionCache.Add(instance);
                }
                return instance;
            }
        }

        /// <summary>
        /// Default settings.
        /// </summary>
        public static JsonSerializerSettings DefaultDeserializeSettings = new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new JsonDeserialisationContractResolver()
        };

        #endregion

        #region Overriden methods

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if ((member is PropertyInfo || member is FieldInfo) && !member.DeclaringType.IsInterface)
            {
                var contracts = s_ContractsType.ToList();
                foreach (var contractType in contracts)
                {
                    GetOrCreateInstance(contractType).SetDeserialisationPropertyContractDefinition(property, member);
                }
                if (property.ShouldDeserialize == null)
                {
                    property.ShouldDeserialize = i => true;
                }
            }
            else
            {
                property.ShouldDeserialize = i => false;
            }
            return property;
        }

        #endregion
    }
}
