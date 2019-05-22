using CQELight.DAL.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
