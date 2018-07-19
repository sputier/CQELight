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
    /// Json serialisation contract resolver.
    /// </summary>
    public class JsonSerialisationContractResolver : DefaultContractResolver
    {

        #region Static members

        private static readonly List<IJsonContractDefinition> s_IJsonContractDefinitionCache = new List<IJsonContractDefinition>();
        private static readonly object s_lockObject = new object();

        #endregion

        #region Static accessors

        static JsonSerialisationContractResolver()
        {
            s_ContractsType = ReflectionTools.GetAllTypes()
                .Where(m => m.GetInterfaces().Contains(typeof(IJsonContractDefinition))).ToList();
        }

        #endregion

        #region Static methods

        static readonly IEnumerable<Type> s_ContractsType;

        private static IJsonContractDefinition GetOrCreateInstance(Type type)
        {
            lock (s_lockObject)
            {
                IJsonContractDefinition instance = s_IJsonContractDefinitionCache.FirstOrDefault(m => m.GetType() == type);
                if (instance == null)
                {
                    instance = (IJsonContractDefinition)type.CreateInstance();
                    s_IJsonContractDefinitionCache.Add(instance);
                }
                return instance;
            }
        }

        /// <summary>
        /// Default parameters.
        /// </summary>
        public static JsonSerializerSettings DefaultSerializeSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ContractResolver = new JsonSerialisationContractResolver()
        };


        #endregion

        #region Overriden methods

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (member is PropertyInfo || member is FieldInfo)
            {
                foreach (var contractType in s_ContractsType)
                {
                    GetOrCreateInstance(contractType).SetSerialisationPropertyContractDefinition(property, member);
                }
            }
            else
            {
                property.ShouldSerialize = i => false;
            }
            return property;
        }

        #endregion

    }
}
