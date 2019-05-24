using CQELight.DAL.Attributes;
using CQELight.Tools.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CQELight.DAL.MongoDb.Mapping
{
    internal class MappingInfo
    {
        #region Members

        private ILogger _logger;

        #endregion

        #region Properties

        public Type EntityType { get; private set; }
        public string CollectionName { get; private set; }
        public string DatabaseName { get; private set; }
        public string IdProperty { get; private set; }
        public IEnumerable<string> IdProperties { get; internal set; }

        #endregion

        #region Ctor

        public MappingInfo(Type type)
            : this(type, null)
        {
        }

        public MappingInfo(Type type, ILoggerFactory loggerFactory)
        {
            _logger = (loggerFactory ?? new LoggerFactory().AddDebug()).CreateLogger("CQELight.DAL.MongoDb");
            EntityType = type;
            ExtractInformationsFromType();
        }

        #endregion

        #region Private methods

        private void Log(string message)
            => _logger.LogInformation(message);

        private void ExtractInformationsFromType()
        {
            ExtractTableAndCollectionInformations();

            var composedKeyAttribute = EntityType.GetCustomAttribute<ComposedKeyAttribute>();
            if (composedKeyAttribute == null)
            {
                var allProperties = EntityType.GetAllProperties();
                var idProperty = allProperties.FirstOrDefault(p =>
                    p.IsDefined(typeof(PrimaryKeyAttribute)) ||p.Name == "Id");
                if (idProperty != null)
                {
                    IdProperty = idProperty.Name;
                }
            }
            else
            {
                IdProperties = composedKeyAttribute.PropertyNames;
            }
        }

        private void ExtractTableAndCollectionInformations()
        {
            Log($"Beginning of treatment for type '{CollectionName}'");

            var tableAttr = EntityType.GetCustomAttribute<TableAttribute>();
            CollectionName =
                !string.IsNullOrWhiteSpace(tableAttr?.TableName)
                ? tableAttr.TableName
                : EntityType.Name;
            if (!string.IsNullOrWhiteSpace(tableAttr?.SchemaName))
            {
                DatabaseName = tableAttr.SchemaName;
            }
            else
            {
                DatabaseName = "DefaultDatabase";
            }
            Log($"Type '{CollectionName}' is defined to go in database '{DatabaseName}'");
        }

        #endregion

    }
}
