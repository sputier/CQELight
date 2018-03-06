using CQELight.DAL.Attributes;
using CQELight.DAL.Exceptions;
using CQELight.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.DAL.Common
{
    /// <summary>
    /// Base class for entity that have a custom primary key.
    /// </summary>
    public abstract class CustomKeyDbEntity : BaseDbEntity
    {

        #region Private members

        private PropertyInfo _primaryKeyProperty;

        #endregion

        #region Private properties

        private PropertyInfo PrimaryKeyProperty
        {
            get
            {
                if (_primaryKeyProperty == null)
                {
                    var entityType = GetType();
                    _primaryKeyProperty = entityType.GetAllProperties().SingleOrDefault(p => p.IsDefined(typeof(PrimaryKeyAttribute), true));
                }
                return _primaryKeyProperty;
            }
        }

        #endregion

        #region Overriden methods

        public override object GetKeyValue()
        {
            if (PrimaryKeyProperty != null)
            {
                return PrimaryKeyProperty.GetValue(this);
            }
            throw new PrimaryKeyPropertyNotFoundException(GetType());
        }

        public override bool IsKeySet()
        {
            if (PrimaryKeyProperty != null)
            {
                object defaultValue = null;
                if (PrimaryKeyProperty.PropertyType.IsValueType)
                {
                    defaultValue = PrimaryKeyProperty.PropertyType.CreateInstance();
                }
                return PrimaryKeyProperty.GetValue(this) != defaultValue;
            }
            throw new PrimaryKeyPropertyNotFoundException(GetType());
        }

        #endregion

    }
}
