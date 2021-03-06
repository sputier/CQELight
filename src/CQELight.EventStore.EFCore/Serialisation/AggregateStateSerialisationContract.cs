﻿using CQELight.DAL.Attributes;
using CQELight.Tools.Serialisation;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CQELight.EventStore.EFCore.Serialisation
{
    class AggregateStateSerialisationContract : IJsonContractDefinition
    {
        #region IJsonContractDefinion

        public void SetDeserialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo)
        {
            if(memberInfo.IsDefined(typeof(IgnoreAttribute)))
            {
                property.ShouldDeserialize = _ => false;
            }
            else
            {
                property.ShouldDeserialize = _ => true;
            }
        }

        public void SetSerialisationPropertyContractDefinition(JsonProperty property, MemberInfo memberInfo)
        {
            if (memberInfo.IsDefined(typeof(IgnoreAttribute)))
            {
                property.ShouldSerialize = _ => false;
            }
            else
            {
                property.ShouldSerialize = _ => true;
            }
        }

        #endregion
    }
}
