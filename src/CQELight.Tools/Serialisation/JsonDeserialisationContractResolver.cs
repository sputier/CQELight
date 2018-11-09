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
        static readonly IEnumerable<Type> s_AllContracts;

        #endregion

        #region Static accessors

        static JsonDeserialisationContractResolver()
        {
            s_AllContracts = ReflectionTools.GetAllTypes()
                .Where(m => m.GetInterfaces().Contains(typeof(IJsonContractDefinition)));
            DefaultDeserializeSettings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new JsonDeserialisationContractResolver()
            };
        }

        #endregion

        #region Static methods


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
        public static JsonSerializerSettings DefaultDeserializeSettings;

        #endregion

        #region Members

        private IEnumerable<IJsonContractDefinition> _contracts;

        #endregion

        #region Ctor

        public JsonDeserialisationContractResolver(params IJsonContractDefinition[] contracts)
        {
            _contracts = contracts;
        }

        public JsonDeserialisationContractResolver(bool autoLoadContracts = false)
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
                            .Select(p => (p.SetMethod != null, CreateProperty(p, memberSerialization)))
                        .Concat(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => !f.Name.Contains("k__BackingField"))
                        .Select(f => (true, CreateProperty(f, memberSerialization))))
                        .ToList();
            props.ForEach(p => { p.Item2.Writable = p.Item1; p.Item2.Readable = true; });
            return props.Select(p => p.Item2).ToList();
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (_contracts?.Any() == true)
            {
                if ((member is PropertyInfo || member is FieldInfo) && !member.DeclaringType.IsInterface)
                {
                    foreach (var contract in _contracts.ToList())
                    {
                        contract.SetDeserialisationPropertyContractDefinition(property, member);
                    }
                    if (property.ShouldDeserialize == null)
                    {
                        property.ShouldDeserialize = i => (member is PropertyInfo p) ? p.SetMethod != null : true;
                    }
                }
                else
                {
                    property.ShouldDeserialize = i => false;
                }
            }
            return property;
        }

        #endregion
    }
}
