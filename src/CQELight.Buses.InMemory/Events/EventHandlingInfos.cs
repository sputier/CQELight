using CQELight.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CQELight.Buses.InMemory.Events
{
    sealed class EventHandlingInfos
    {

        #region Properties


        public MethodInfo HandlerMethod { get; }
        public object HandlerInstance { get; }

        #endregion

        #region Ctor

        public EventHandlingInfos(MethodInfo handlerMethod, object handlerInstance)
        {
            HandlerMethod = handlerMethod ?? throw new ArgumentNullException(nameof(handlerMethod));
            HandlerInstance = handlerInstance ?? throw new ArgumentNullException(nameof(handlerInstance));
        }

        #endregion

        #region Overriden methods

        public override bool Equals(object obj)
        {
            if (obj is EventHandlingInfos infos)
            {
                return infos.HandlerMethod.ToString() == HandlerMethod.ToString()
                    && new TypeEqualityComparer().Equals(infos.HandlerMethod.ReflectedType, HandlerMethod.ReflectedType);
            }
            return false;
        }

        public override int GetHashCode()
            => HandlerMethod.ToString().GetHashCode();

        #endregion

    }
}
