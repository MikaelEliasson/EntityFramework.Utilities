/// <summary>
/// Adapted from http://romiller.com/2013/09/24/ef-code-first-mapping-between-types-tables/ 
/// This whole file contains a hack needed because the mapping API is internal pre 6.1 atleast
/// </summary>
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EntityFramework.Utilities
{

    /// <summary>
    /// Represents the mapping of an entitiy type to one or mode tables in the database
    ///
    /// A single entity can be mapped to more than one table when 'Entity Splitting' is used
    /// Entity Splitting involves mapping different properties from the same type to different tables
    /// See http://msdn.com/data/jj591617#2.7 for more details
    /// </summary>
    public class TypeMapping
    {
        /// <summary>
        /// The type of the entity from the model
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// The table(s) that the entity is mapped to
        /// </summary>
        public List<TableMapping> TableMappings { get; set; }
    }

    /// <summary>
    /// Represents the mapping of an entity to a table in the database
    /// </summary>
    public class TableMapping
    {
        /// <summary>
        /// The name of the table the entity is mapped to
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// The schema of the table the entity is mapped to
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Details of the property-to-column mapping
        /// </summary>
        public List<PropertyMapping> PropertyMappings { get; set; }

    }

    /// <summary>
    /// Represents the mapping of a property to a column in the database
    /// </summary>
    public class PropertyMapping
    {
        /// <summary>
        /// The property chain leading to this property. For scalar properties this is a single value but for Complex properties this is a dot (.) separated list
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The column that property is mapped to
        /// </summary>
        public string ColumnName { get; set; }
    }

    /// <summary>
    /// Represents that mapping between entity types and tables in an EF model
    /// </summary>
    public class EfMapping
    {
        /// <summary>
        /// Mapping information for each entity type in the model
        /// </summary>
        public Dictionary<Type, TypeMapping> TypeMappings { get; set; }

        /// <summary>
        /// Initializes an instance of the EfMapping class
        /// </summary>
        /// <param name="db">The context to get the mapping from</param>
        public EfMapping(DbContext db)
        {
            this.TypeMappings = new Dictionary<Type, TypeMapping>();

            var metadata = ((IObjectContextAdapter)db).ObjectContext.MetadataWorkspace;

            // Conceptual part of the model has info about the shape of our entity classes
            var conceptualContainer = metadata.GetItems<EntityContainer>(DataSpace.CSpace).Single();

            // Storage part of the model has info about the shape of our tables
            var storeContainer = metadata.GetItems<EntityContainer>(DataSpace.SSpace).Single();

            // Object part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Mapping part of model is not public, so we need to write to xml and use 'LINQ to XML'
            var edmx = GetEdmx(db);

            // Loop thru each entity type in the model
            foreach (var set in conceptualContainer.BaseEntitySets.OfType<EntitySet>())
            {
                var typeMapping = new TypeMapping
                {
                    TableMappings = new List<TableMapping>(),
                    EntityType = GetClrType(metadata, objectItemCollection, set)
                };

                this.TypeMappings.Add(typeMapping.EntityType, typeMapping);

                // Get the mapping fragments for this type
                // (types may have mutliple fragments if 'Entity Splitting' is used)
                var mappingFragments = edmx
                    .Descendants()
                    .Single(e =>
                        e.Name.LocalName == "EntityTypeMapping"
                        && (e.Attribute("TypeName").Value == set.ElementType.FullName || e.Attribute("TypeName").Value == string.Format("IsTypeOf({0})", set.ElementType.FullName)))
                    .Descendants()
                    .Where(e => e.Name.LocalName == "MappingFragment");

                foreach (var mapping in mappingFragments)
                {
                    var tableMapping = new TableMapping
                    {
                        PropertyMappings = new List<PropertyMapping>()
                    };
                    typeMapping.TableMappings.Add(tableMapping);

                    // Find the table that this fragment maps to
                    var storeset = mapping.Attribute("StoreEntitySet").Value;
                    var store = storeContainer
                        .BaseEntitySets.OfType<EntitySet>()
                        .Single(s => s.Name == storeset);

                    tableMapping.TableName = (string)store.MetadataProperties["Table"].Value;
                    tableMapping.Schema = (string)store.MetadataProperties["Schema"].Value;

                    // Find the property-to-column mappings
                    var propertyMappings = mapping
                        .Descendants()
                        .Where(e => e.Name.LocalName == "ScalarProperty");

                    foreach (var propertyMapping in propertyMappings)
                    {
                        var propertyName = GetFullName(propertyMapping);
                        var columnName = propertyMapping.Attribute("ColumnName").Value;

                        tableMapping.PropertyMappings.Add(new PropertyMapping
                        {
                            PropertyName = propertyName,
                            ColumnName = columnName
                        });
                    }
                }
            }
        }

        private string GetFullName(XElement propertyMapping)
        {
            if (propertyMapping.Parent.Name.LocalName == "ComplexProperty")
            {
                return GetFullName(propertyMapping.Parent) + "." + propertyMapping.Attribute("Name").Value;
            }
            return propertyMapping.Attribute("Name").Value;
        }

        private static Type GetClrType(MetadataWorkspace metadata, ObjectItemCollection objectItemCollection, EntitySet set)
        {
            return metadata
                   .GetItems<EntityType>(DataSpace.OSpace)
                   .Select(objectItemCollection.GetClrType)
                   .Single(e => e.Name == set.ElementType.Name);
        }

        private static XDocument GetEdmx(DbContext db)
        {
            XDocument doc;
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(
                    memoryStream, new XmlWriterSettings
                    {
                        Indent = true
                    }))
                {
                    EdmxWriter.WriteEdmx(db, xmlWriter);
                }

                memoryStream.Position = 0;

                doc = XDocument.Load(memoryStream);
            }
            return doc;
        }
    }

    public static class EfMappingFactory
    {
        private static Dictionary<Type, EfMapping> cache = new Dictionary<Type, EfMapping>();

        public static EfMapping GetMappingsForContext(DbContext context)
        {
            var type = context.GetType();
            EfMapping mapping;
            if (!cache.TryGetValue(type, out mapping))
            {
                mapping = new EfMapping(context);
                cache.Add(type, mapping);
            }
            return mapping;
        }

    }
}
