using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CQELight.Tools.Serialisation
{
    /// <summary>
    /// Contrat interface for specific json serialisation needs
    /// </summary>
    public interface IJsonContractDefinition
    {
        /// <summary>
        /// Set value for serialisation scenarios on a specific member/property.
        /// </summary>
        /// <param name="property">JsonProperty object.</param>
        /// <param name="memberInfo">Reflection infos object.</param>
        void SetSerialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo);
        /// <summary>
        /// Set value for deserialisation scenarios on a specific member/property.
        /// </summary>
        /// <param name="property">JsonProperty object.</param>
        /// <param name="memberInfo">Reflection infos object.</param>
        void SetDeserialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo);
    }
}
