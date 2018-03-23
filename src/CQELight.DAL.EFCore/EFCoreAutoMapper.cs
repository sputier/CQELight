using CQELight.DAL.Attributes;
using CQELight.DAL.Common;
using CQELight.Tools.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CM = System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Linq;

namespace CQELight.DAL.EFCore
{
    /// <summary>
    /// Class to help transforming attributes into a comprehensive EF Core mapping.
    /// </summary>
    internal static class EFCoreAutoMapper
    {

        #region Members

        private static List<Type> _alreadyTreatedTypes = new List<Type>();
        private static ILogger _logger;
        private static object s_Lock = new object();

        #endregion

        #region Public static methods

        /// <summary>
        /// Basic method that transform all attributes and 
        /// </summary>
        /// <param name="modelBuilder">Builder of EF Core model.</param>
        /// <param name="typeToMap">Type to treat.</param>
        /// <param name="useSchemas">Flag that indicates if current provider can handle schemas.</param>
        /// <param name="loggerFactory">Logger factory</param>
        public static void AutoMap(ModelBuilder modelBuilder, Type typeToMap, bool useSchemas = true, ILoggerFactory loggerFactory = null)
        {
            _logger = _logger ?? loggerFactory?.CreateLogger("EFCoreAutoMapper");

            if (_alreadyTreatedTypes.Contains(typeToMap))
            {
                return;
            }
            lock (s_Lock)
            {
                _alreadyTreatedTypes.Add(typeToMap);

                Log($"Beginning treating mapping for type {typeToMap}", true);

                var entityBuilder = modelBuilder.Entity(typeToMap);
                ApplyBaseRulesOnBuilder(entityBuilder, typeToMap);
                AutoMapInternal(entityBuilder, typeToMap, useSchemas);

                Log($"Ending treating mapping for type {typeToMap}", true);
            }
        }

        #endregion

        #region Internal methods

        internal static void CleanAlreadyTreatedTypes() => _alreadyTreatedTypes.Clear();

        #endregion

        #region Private static methods

        private static void Log(string message, bool debug = false)
        {
            if (_logger == null)
            {
                return;
            }
            if (debug)
            {
                _logger.LogDebug(message);
            }
            else
            {
                _logger.LogInformation(message);
            }
        }

        private static void ApplyBaseRulesOnBuilder(EntityTypeBuilder entityBuilder, Type entityType)
        {
            if (entityType.IsSubclassOf(typeof(BaseDbEntity)))
            {
                SetBasePropertiesConstraints(entityBuilder);
            }
            foreach (var prop in entityType.GetAllProperties().Where(p => p.IsDefined(typeof(IgnoreAttribute))))
            {
                Log($"Property '{prop.Name}' of type '{entityType.Name}' ignored.");
                entityBuilder.Ignore(prop.Name);
            }

        }


        private static void AutoMapInternal(EntityTypeBuilder builder, Type entityType, bool useSQLServer = true)
        {
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            if (tableAttr == null)
            {
                throw new InvalidOperationException($"EFCoreAutoMapper : Cannot treat mapping for type '{entityType.Name}' because TableAttribute was not defined.");
            }
            Log($"Beginning of treatment for type '{tableAttr.TableName}'");

            if (useSQLServer)
            {
                builder.ToTable(tableAttr.TableName, tableAttr.SchemaName);
            }
            else
            {
                builder.ToTable(tableAttr.TableName);
            }

            CreateColumns(builder, entityType);
            CreatePrimaryKey(builder, entityType);
            CreateComplexIndexes(builder, entityType);
            CreateRelations(builder, entityType);
        }

        private static void SetBasePropertiesConstraints(EntityTypeBuilder entityBuilder)
        {
            entityBuilder.Property(BaseDbEntity.CONST_EDIT_DATE_COLUMN).IsRequired(true);
            entityBuilder.Property(BaseDbEntity.CONST_DELETED_COLUMN).IsRequired(false).HasDefaultValue(false);
            entityBuilder.Property(BaseDbEntity.CONST_DELETE_DATE_COLUMN).IsRequired(false);
        }

        private static void CreateColumns(EntityTypeBuilder builder, Type entityType)
        {
            var properties = entityType.GetAllProperties();
            foreach (var column in properties
                .Where(p => p.IsDefined(typeof(ColumnAttribute))))
            {
                var columnAttr = column.GetCustomAttribute<ColumnAttribute>();

                var propBuilder = builder.Property(column.PropertyType, column.Name);
                propBuilder.HasColumnName(columnAttr.ColumnName);

                Log($"Creation of column '{columnAttr.ColumnName}' for property '{column.Name}'.");
                ApplyColumnConstrait(builder, column, columnAttr, propBuilder);
            }


            foreach (var fkColumn in properties
                .Where(p => p.IsDefined(typeof(KeyStorageOfAttribute))))
            {
                var kosData = fkColumn.GetCustomAttribute<KeyStorageOfAttribute>();
                var entityProperty = properties.FirstOrDefault(p => kosData.PropertyName == p.Name);

                var distantProperties = entityProperty.PropertyType.GetAllProperties();
                var distantTableAttr = entityProperty.PropertyType.GetCustomAttribute<TableAttribute>();


                var propBuilder = builder.Property(fkColumn.PropertyType, fkColumn.Name);
                string columnName = string.Empty;
                var columnAttr = fkColumn.GetCustomAttribute<ColumnAttribute>();
                if (columnAttr != null)
                {
                    columnName = columnAttr.ColumnName;
                }
                else
                {
                    var distantKeyProp = distantProperties.FirstOrDefault(p => p.IsDefined(typeof(PrimaryKeyAttribute)));
                    if (distantKeyProp != null)
                    {
                        var distantKeyAttr = distantKeyProp.GetCustomAttribute<PrimaryKeyAttribute>();
                        columnName = string.IsNullOrWhiteSpace(distantKeyAttr.KeyName)
                            ? distantTableAttr.TableName.Substring(0, 3) + "_ID"
                            : distantKeyAttr.KeyName;
                    }
                    else
                    {
                        columnName = fkColumn.Name;
                    }
                }

                propBuilder
                    .HasColumnName(columnName)
                    .IsRequired(fkColumn.IsDefined(typeof(CM.RequiredAttribute)) || (fkColumn.PropertyType.IsValueType && Nullable.GetUnderlyingType(fkColumn.PropertyType) == null));
                if (fkColumn.PropertyType == typeof(string) && fkColumn.IsDefined(typeof(CM.MaxLengthAttribute)))
                {
                    var lgt = fkColumn.GetCustomAttribute<CM.MaxLengthAttribute>().Length;
                    propBuilder.HasMaxLength(lgt);
                    Log($"Setting maxLength of FK '{columnName}' to {lgt}", true);
                }
            }
        }

        private static void ApplyColumnConstrait(EntityTypeBuilder builder, PropertyInfo column, ColumnAttribute columnAttr, PropertyBuilder propBuilder)
        {
            if (column.IsDefined(typeof(CM.RequiredAttribute)) || (column.PropertyType.IsValueType && Nullable.GetUnderlyingType(column.PropertyType) == null))
            {
                propBuilder.IsRequired();
                Log($"Setting '{columnAttr.ColumnName}' as not nullable.", true);
            }
            if (column.PropertyType == typeof(string) && column.IsDefined(typeof(CM.MaxLengthAttribute)))
            {
                var lgt = column.GetCustomAttribute<CM.MaxLengthAttribute>().Length;
                propBuilder.HasMaxLength(lgt);
                Log($"Setting maxLength of '{columnAttr.ColumnName}' to {lgt}.", true);
            }
            if (column.IsDefined(typeof(DefaultValueAttribute)))
            {
                var def = column.GetCustomAttribute<DefaultValueAttribute>().Value;
                propBuilder.HasDefaultValue(def);
                Log($"Setting defaultValue of '{columnAttr.ColumnName}' to {def}.", true);
            }
            if (column.IsDefined(typeof(IndexAttribute)))
            {
                var idxInfos = column.GetCustomAttribute<IndexAttribute>();

                Log($"Creation of an index on column '{columnAttr.ColumnName}'", true);
                var idx = builder.HasIndex(column.Name);
                if (idxInfos.IsUnique)
                {
                    Log(" UNIQUE", true);
                    idx.IsUnique(true);
                }
                if (!string.IsNullOrWhiteSpace(idxInfos.IndexName))
                {
                    Log($" named {idxInfos.IndexName}", true);
                    idx.HasName(idxInfos.IndexName);
                }
            }
        }

        private static void CreateComplexIndexes(EntityTypeBuilder builder, Type entityType)
        {
            var complexIndexAttrs = entityType.GetCustomAttributes<ComplexIndexAttribute>();
            if (!complexIndexAttrs.Any())
            {
                return;
            }
            var properties = entityType.GetAllProperties();
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            foreach (var idx in complexIndexAttrs)
            {
                Log($"Creation of a complex index on table '{tableAttr.TableName}' on properties '{string.Join(",", idx.PropertyNames)}'");
                var idxColumns = new List<string>();

                foreach (var item in idx.PropertyNames)
                {
                    var prop = properties.FirstOrDefault(p => p.Name == item);
                    if (prop == null)
                    {
                        throw new InvalidOperationException($"Cannot get property '{item}' which is part of complex index '{entityType.Name}'");
                    }
                    if (!IsForeignEntity(prop))
                    {
                        idxColumns.Add(item);
                    }
                    else
                    {
                        var keyStorageOfProp = properties.FirstOrDefault(p => p.IsDefined(typeof(KeyStorageOfAttribute))
                            && p.GetCustomAttribute<KeyStorageOfAttribute>().PropertyName == item);
                        if (keyStorageOfProp == null)
                        {
                            throw new InvalidOperationException($"Cannot use property '{item}' as part of a complex index " +
                                $"because it's ForeignKey and KeyStorageOfAttribute is not defined.");
                        }
                        idxColumns.Add(keyStorageOfProp.Name);
                    }
                }

                var dbIdx = builder.HasIndex(idxColumns.ToArray());
                if (idx.IsUnique)
                {
                    dbIdx.IsUnique();
                }
                if (!string.IsNullOrWhiteSpace(idx.IndexName))
                {
                    string name = idx.IndexName;
                    if (name.Length > 128)
                    {
                        name = idx.IndexName.Substring(0, 127);
                    }
                    dbIdx.HasName(name);
                }
            }
        }

        private static void CreatePrimaryKey(EntityTypeBuilder builder, Type entityType)
        {
            var properties = entityType.GetAllProperties();
            if (!entityType.IsSubclassOf(typeof(ComposedKeyDbEntity)))
            {
                var keyProp = properties.FirstOrDefault(p => p.IsDefined(typeof(PrimaryKeyAttribute)));
                if (keyProp == null)
                {
                    throw new InvalidOperationException($"Cannot retrieve primary key informations for '{entityType.Name}'");
                }
                var keyAttr = keyProp.GetCustomAttribute<PrimaryKeyAttribute>();
                var keyName =
                    string.IsNullOrWhiteSpace(keyAttr.KeyName)
                    ? entityType.GetCustomAttribute<TableAttribute>().TableName.Substring(0, 3) + "_ID"
                    : keyAttr.KeyName;
                Log($"Creation of primary key '{keyName}' for type '{entityType.Name}'", true);
                var keyPropBuilder = builder.Property(keyProp.Name)
                       .HasColumnName(keyName);
                if (keyProp.PropertyType == typeof(string) && keyProp.IsDefined(typeof(CM.MaxLengthAttribute)))
                {
                    var lgt = keyProp.GetCustomAttribute<CM.MaxLengthAttribute>().Length;
                    keyPropBuilder.HasMaxLength(lgt);
                    Log($"Setting maxLength of primary key '{keyName}' to '{lgt}'", true);
                }
                builder.HasKey(keyProp.Name);
            }
            else
            {
                var composedKeyAttr = entityType.GetCustomAttribute<ComposedKeyAttribute>();
                if (composedKeyAttr == null)
                {
                    throw new InvalidOperationException($"Cannot retrieve composed primary key informations for '{entityType.Name}'.");
                }
                var keyProperties = new List<string>();
                foreach (var item in composedKeyAttr.PropertyNames)
                {
                    var prop = properties.FirstOrDefault(p => p.Name == item);
                    if (prop == null)
                    {
                        throw new InvalidOperationException($"Cannot retrieve property '{item}' which is part of composed primary key for entity type '{entityType.Name}'");
                    }
                    if (!IsForeignEntity(prop)) // Type simple faisant partie de la clé
                    {
                        keyProperties.Add(item);
                    }
                    else
                    {
                        var keyStorageOfProp = properties.FirstOrDefault(p => p.IsDefined(typeof(KeyStorageOfAttribute))
                            && p.GetCustomAttribute<KeyStorageOfAttribute>().PropertyName == item);
                        if (keyStorageOfProp == null)
                        {
                            throw new InvalidOperationException($"Cannot retrieve property '{item}' which is part of composed primary key because" +
                                $" it's a foreign key and KeyStorageOfAttribute is not defined.");
                        }
                        keyProperties.Add(keyStorageOfProp.Name);
                    }
                }
                builder.HasKey(keyProperties.ToArray());
            }
        }

        private static void CreateRelations(EntityTypeBuilder builder, Type entityType)
        {
            var properties = entityType.GetAllProperties();
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            foreach (var simpleEntityLink in properties.Where(IsForeignEntity))
            {
                var foreignKeyProps = new List<string>();

                var distantProperties = simpleEntityLink.PropertyType.GetAllProperties();
                var distantTableAttr = simpleEntityLink.PropertyType.GetCustomAttribute<TableAttribute>();
                var currentFkAttr = simpleEntityLink.GetCustomAttribute<ForeignKeyAttribute>();

                bool required = simpleEntityLink.IsDefined(typeof(CM.RequiredAttribute));

                if (simpleEntityLink.PropertyType.IsSubclassOf(typeof(ComposedKeyDbEntity)))
                {
                    var composedKeyAttr = simpleEntityLink.PropertyType.GetCustomAttribute<ComposedKeyAttribute>();
                    if (composedKeyAttr == null)
                    {
                        throw new InvalidOperationException($"Entity of type '{simpleEntityLink.PropertyType.Name}' cannot have a relation with '{entityType.Name}' " +
                            "because composed primary key was'nt defined.");
                    }
                    var keyStoragesOf = properties.Where(p => p.IsDefined(typeof(KeyStorageOfAttribute))
                        && p.GetCustomAttributes<KeyStorageOfAttribute>().Any(a => a.PropertyName == simpleEntityLink.Name));
                    if (!keyStoragesOf.Any() || keyStoragesOf.Count() != composedKeyAttr.PropertyNames.Length)
                    {
                        throw new InvalidOperationException($"Some attribute or columns are missing for entity type '{entityType.Name}' " +
                            $"to allow a relation with '{simpleEntityLink.PropertyType.Name}'");
                    }
                    foreignKeyProps = keyStoragesOf.Select(p => p.Name).ToList();
                }
                else
                {
                    var keyStorageOf = properties.FirstOrDefault(p => p.IsDefined(typeof(KeyStorageOfAttribute))
                        && p.GetCustomAttributes<KeyStorageOfAttribute>().Any(a => a.PropertyName == simpleEntityLink.Name));
                    if (keyStorageOf == null)
                    {
                        throw new InvalidOperationException($"Cannot find KeyStorageOf property for '{simpleEntityLink.PropertyType.Name}'" +
                            $" in entity type '{entityType.Name}'");
                    }
                    foreignKeyProps.Add(keyStorageOf.Name);
                }

                var distantCollectionProperty = distantProperties.FirstOrDefault(
                   p =>
                       p.PropertyType.IsGenericType &&
                       p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                       p.PropertyType.GenericTypeArguments[0] == entityType &&
                       (currentFkAttr == null || string.IsNullOrWhiteSpace(currentFkAttr.InversePropertyName) || currentFkAttr.InversePropertyName == p.Name));

                var distantEntity = distantProperties.FirstOrDefault(
                    p => p.PropertyType == entityType);

                if (distantCollectionProperty != null)
                {
                    var link = builder
                        .HasOne(simpleEntityLink.PropertyType, simpleEntityLink.Name)
                        .WithMany(distantCollectionProperty.Name)
                        .IsRequired(required)
                        .OnDelete(required ? (currentFkAttr.DeleteCascade ? DeleteBehavior.Cascade : DeleteBehavior.Restrict) : DeleteBehavior.SetNull);
                    if (foreignKeyProps != null && foreignKeyProps.Any())
                    {
                        link
                            .HasForeignKey(foreignKeyProps.ToArray());
                    }
                    Log($"Creating relationship from 1 {tableAttr.TableName} to many {distantTableAttr.TableName}");
                }
                else if (distantEntity != null)
                {

                    var link = builder
                        .HasOne(simpleEntityLink.PropertyType, simpleEntityLink.Name)
                        .WithOne(distantEntity.Name)
                        .IsRequired(required)
                        .OnDelete(required ? (currentFkAttr.DeleteCascade ? DeleteBehavior.Cascade : DeleteBehavior.Restrict) : DeleteBehavior.SetNull);
                    if (foreignKeyProps != null && foreignKeyProps.Any())
                    {
                        link
                            .HasForeignKey(entityType, foreignKeyProps.ToArray());
                    }
                    Log($"Creating relationship from 1 {tableAttr.TableName} to 1 {distantTableAttr.TableName}");
                }
                else
                {
                    var link = builder
                           .HasOne(simpleEntityLink.PropertyType, simpleEntityLink.Name)
                           .WithMany()
                           .IsRequired(required)
                           .OnDelete(required ? (currentFkAttr.DeleteCascade ? DeleteBehavior.Cascade : DeleteBehavior.Restrict) : DeleteBehavior.SetNull);
                    if (foreignKeyProps != null && foreignKeyProps.Any())
                    {
                        link
                            .HasForeignKey(foreignKeyProps.ToArray());
                    }
                    Log($"Creating relationship from 1 {tableAttr.TableName} to many (guessed) {distantTableAttr.TableName}");
                }
            }

            foreach (var item in properties.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                var collectionEntityType = item.PropertyType.GetGenericArguments()[0];

                var distantProperties = collectionEntityType.GetAllProperties();
                var distantTableAttr = collectionEntityType.GetCustomAttribute<TableAttribute>();
                if (distantTableAttr == null)
                {
                    throw new InvalidOperationException($"EFCoreAutoMapper.CreateRelations() : Entity of type '{collectionEntityType.Name}' has no TableAttribute, it's impossible to use it.");
                }

                var distantCollectionProperty = distantProperties.FirstOrDefault(
                   p =>
                       p.PropertyType.IsGenericType &&
                       p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                       p.PropertyType.GenericTypeArguments[0] == collectionEntityType);
                if (distantCollectionProperty != null)
                {
                    throw new NotSupportedException("Many to many direct relationship are currently impossible in EFCore (version 2.0). " +
                        "You should create an object in between to define it.");
                }

                bool CheckDistantPropIsValid(PropertyInfo p)
                {
                    return !p.IsDefined(typeof(ForeignKeyAttribute))
                        || (p.IsDefined(typeof(ForeignKeyAttribute)) &&
                                (p.GetCustomAttribute<ForeignKeyAttribute>().InversePropertyName == item.Name
                                || string.IsNullOrWhiteSpace(p.GetCustomAttribute<ForeignKeyAttribute>().InversePropertyName)));
                };

                var distantEntity = distantProperties.FirstOrDefault(
                    p => p.PropertyType == entityType
                    && CheckDistantPropIsValid(p));

                if (distantEntity != null)
                {

                    var link = builder
                        .HasMany(collectionEntityType, item.Name)
                        .WithOne(distantEntity.Name);


                    Log($"Creating relationship from many {tableAttr.TableName} to 1 {distantTableAttr.TableName}");
                }
                else
                {
                    var link = builder
                            .HasMany(collectionEntityType, item.Name)
                            .WithOne();
                    Log($"Creating relationship from many {tableAttr.TableName} to 1 (guessed) {distantTableAttr.TableName}");
                }

            }
        }

        private static bool IsForeignEntity(PropertyInfo p)
            => p.PropertyType.IsSubclassOf(typeof(DbEntity)) 
            || p.PropertyType.IsSubclassOf(typeof(ComposedKeyDbEntity)) 
            || p.PropertyType.IsSubclassOf(typeof(CustomKeyDbEntity));

        #endregion

    }
}
