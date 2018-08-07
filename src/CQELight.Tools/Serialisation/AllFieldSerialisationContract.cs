using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace CQELight.Tools.Serialisation
{
    /// <summary>
    /// Contract definition that implies of (de)serializing all members,
    /// including private/protected/internal ones.
    /// </summary>
    public class AllFieldSerialisationContract : IJsonContractDefinition
    {

        #region IJsonContractDefinition

        public void SetDeserialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo)
        {
            property.ShouldDeserialize = _ => true;
        }

        public void SetSerialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo)
        {
            property.ShouldSerialize = _ => true;
        }

        #endregion
    }
}
