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
        static readonly IEnumerable<Type> s_AllContracts;

        #endregion

        #region Static accessors

        static JsonSerialisationContractResolver()
        {
            s_AllContracts = ReflectionTools.GetAllTypes()
                .Where(m => m.GetInterfaces().Contains(typeof(IJsonContractDefinition))).ToList();
            DefaultSerializeSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ContractResolver = new JsonSerialisationContractResolver(true)
            };
        }

        #endregion

        #region Static methods

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
        public static JsonSerializerSettings DefaultSerializeSettings;


        #endregion

        #region Members

        private IEnumerable<IJsonContractDefinition> _contracts;

        #endregion

        #region Ctor

        public JsonSerialisationContractResolver(params IJsonContractDefinition[] contracts)
        {
            _contracts = contracts;
        }

        public JsonSerialisationContractResolver(bool autoLoadContracts = false)
        {
            if (autoLoadContracts)
            {
                _contracts = s_AllContracts.Select(GetOrCreateInstance);
            }
        }

        #endregion

        #region Overriden methods

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .Select(p => base.CreateProperty(p, memberSerialization))
                        .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                   .Select(f => base.CreateProperty(f, memberSerialization)))
                        .ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (member is PropertyInfo || member is FieldInfo)
            {
                foreach (var contract in _contracts.ToList())
                {
                    contract.SetSerialisationPropertyContractDefinition(property, member);
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
