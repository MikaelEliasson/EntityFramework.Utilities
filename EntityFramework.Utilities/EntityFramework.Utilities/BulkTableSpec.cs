using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Utilities
{
    public class BulkTableSpec
    {
        public List<ColumnMapping> Properties { get; set; }
        public TableMapping TableMapping { get; set; }
        public TypeMapping TypeMapping { get; set; }

        public static BulkTableSpec Get<TEntity, T>(DbContext context) where TEntity : class, T
        {
            var mapping = EfMappingFactory.GetMappingsForContext(context);
            var typeMapping = mapping.TypeMappings[typeof(T)];
            var tableMapping = typeMapping.TableMappings.First();

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
