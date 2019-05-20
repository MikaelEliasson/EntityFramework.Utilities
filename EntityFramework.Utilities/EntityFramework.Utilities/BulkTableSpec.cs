using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EntityFramework.Utilities
{
    public class BulkTableSpec
    {
        public List<ColumnMapping> Properties { get; set; }
        public TableMapping TableMapping { get; set; }
        public TypeMapping TypeMapping { get; set; }

        public static BulkTableSpec Get<TEntity, T>(DbContext context) where TEntity : class, T
        {
            var typeMapping = EfMappingFactory.GetMappingForType<T>(context);
            var tableMapping = typeMapping.TableMapping;

            var properties = GetProperties(typeof(TEntity), tableMapping);

            if (tableMapping.TPHConfiguration != null)
            {
                properties.Add(new ColumnMapping
                {
                    NameInDatabase = tableMapping.TPHConfiguration.ColumnName,
                    StaticValue = tableMapping.TPHConfiguration.Mappings[typeof(TEntity)]
                });
            }

            return new BulkTableSpec
            {
                Properties = properties,
                TableMapping = tableMapping,
                TypeMapping = typeMapping,
            };
        }

        public BulkTableSpec Copy()
        {
            return new BulkTableSpec
            {
                Properties = Properties.Select(p => p.Copy()).ToList(),
                TableMapping = TableMapping.Copy(),
                TypeMapping = TypeMapping.Copy()
            };
        }

        private static List<ColumnMapping> GetProperties(Type currentType, TableMapping tableMapping)
        {
            var properties = tableMapping.PropertyMappings
                .Where(p => currentType.IsSubclassOf(p.ForEntityType) || p.ForEntityType == currentType)
                .Select(p => new ColumnMapping
                {
                    NameInDatabase = p.ColumnName,
                    NameOnObject = p.PropertyName,
                    DataType = p.DataTypeFull,
                    IsPrimaryKey = p.IsPrimaryKey,
                    IsStoreGenerated = p.IsStoreGenerated,
                }).ToList();
            return properties;
        }
    }



}
