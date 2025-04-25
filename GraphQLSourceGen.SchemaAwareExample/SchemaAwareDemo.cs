using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphQLSourceGen.SchemaAwareExample
{
    /// <summary>
    /// Demonstrates how schema-aware fragment generation works behind the scenes
    /// </summary>
    public class SchemaAwareDemo
    {
        /// <summary>
        /// Run a demonstration of schema parsing and fragment enhancement
        /// </summary>
        public static void RunDemo()
        {
            Console.WriteLine("\nSchema-Aware Fragment Generation Demo");
            Console.WriteLine("======================================");

            // Step 1: Load the GraphQL schema files
            var schemaFiles = new[]
            {
                "GraphQLSourceGen.SchemaAwareExample/schema/schema.graphql",
                "GraphQLSourceGen.SchemaAwareExample/schema/schema-extensions.graphql"
            };
            
            GraphQLSchema schema = LoadSchema(schemaFiles);
            
            if (schema == null)
            {
                Console.WriteLine("Failed to load schema. Demo cannot continue.");
                return;
            }
            
            Console.WriteLine($"Loaded schema with:");
            Console.WriteLine($"- {schema.Types.Count} types");
            Console.WriteLine($"- {schema.Interfaces.Count} interfaces");
            Console.WriteLine($"- {schema.Unions.Count} unions");
            Console.WriteLine($"- {schema.Enums.Count} enums");
            Console.WriteLine($"- {schema.ScalarTypes.Count} scalar types");

            // Step 2: Load and parse a GraphQL fragment
            string fragmentContent = @"
                fragment UserWithPosts on User {
                  id
                  name
                  email
                  posts {
                    id
                    title
                    content
                    publishedAt
                    viewCount
                    rating
                  }
                }
            ";

            var fragments = GraphQLParser.ParseContent(fragmentContent);
            Console.WriteLine($"\nParsed fragment: {fragments[0].Name} on {fragments[0].OnType}");
            
            // Step 3: Enhance the fragment with schema information
            EnhanceFragmentsWithSchema(fragments, schema);
            
            // Step 4: Display the enhanced fragment with type information
            var fragment = fragments[0];
            Console.WriteLine("\nEnhanced fragment with type information:");
            
            foreach (var field in fragment.Fields)
            {
                DisplayField(field, 1);
            }
            
            // Step 5: Explain the benefits of schema-aware generation
            Console.WriteLine("\nBenefits of Schema-Aware Generation:");
            Console.WriteLine("1. Accurate Type Inference: The generator uses the schema to determine the exact type of each field");
            Console.WriteLine("2. Support for Complex Types: Properly handles interfaces, unions, custom scalars, and nested types");
            Console.WriteLine("3. Type Validation: Validates that fields referenced in fragments actually exist in the schema");
            Console.WriteLine("4. Better Documentation: Includes field descriptions from the schema in the generated code");
        }

        /// <summary>
        /// Load and parse GraphQL schema files
        /// </summary>
        private static GraphQLSchema LoadSchema(string[] schemaFilePaths)
        {
            try
            {
                // Create a new schema
                var schema = new GraphQLSchema();
                
                // Load and parse each schema file
                foreach (var path in schemaFilePaths)
                {
                    if (!File.Exists(path))
                    {
                        Console.WriteLine($"Warning: Schema file not found: {path}");
                        continue;
                    }
                    
                    string schemaContent = File.ReadAllText(path);
                    var parsedSchema = GraphQLSchemaParser.ParseSchema(schemaContent);
                    
                    // Manually merge the parsed schema into the main schema
                    MergeSchemas(schema, parsedSchema);
                    
                    Console.WriteLine($"Loaded schema file: {path}");
                }
                
                return schema;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading schema: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Manually merge two schemas
        /// </summary>
        private static void MergeSchemas(GraphQLSchema target, GraphQLSchema source)
        {
            // Merge types
            foreach (var type in source.Types)
            {
                if (!target.Types.ContainsKey(type.Key))
                {
                    target.Types[type.Key] = type.Value;
                }
                else
                {
                    // Merge fields for existing types
                    foreach (var field in type.Value.Fields)
                    {
                        if (!target.Types[type.Key].Fields.ContainsKey(field.Key))
                        {
                            target.Types[type.Key].Fields[field.Key] = field.Value;
                        }
                    }
                }
            }

            // Merge interfaces
            foreach (var iface in source.Interfaces)
            {
                if (!target.Interfaces.ContainsKey(iface.Key))
                {
                    target.Interfaces[iface.Key] = iface.Value;
                }
            }

            // Merge unions
            foreach (var union in source.Unions)
            {
                if (!target.Unions.ContainsKey(union.Key))
                {
                    target.Unions[union.Key] = union.Value;
                }
            }

            // Merge enums
            foreach (var enumType in source.Enums)
            {
                if (!target.Enums.ContainsKey(enumType.Key))
                {
                    target.Enums[enumType.Key] = enumType.Value;
                }
            }

            // Merge scalar types
            foreach (var scalar in source.ScalarTypes)
            {
                if (!target.ScalarTypes.ContainsKey(scalar.Key))
                {
                    target.ScalarTypes[scalar.Key] = scalar.Value;
                }
                else
                {
                    // For existing scalar types, we might want to merge properties
                    var targetScalar = target.ScalarTypes[scalar.Key];
                    var sourceScalar = scalar.Value;
                    
                    // Merge description if needed
                    if (!string.IsNullOrEmpty(sourceScalar.Description) &&
                        string.IsNullOrEmpty(targetScalar.Description))
                    {
                        targetScalar.Description = sourceScalar.Description;
                    }
                }
            }
        }

        /// <summary>
        /// Enhance fragments with schema information (simplified version)
        /// </summary>
        private static void EnhanceFragmentsWithSchema(List<GraphQLFragment> fragments, GraphQLSchema schema)
        {
            foreach (var fragment in fragments)
            {
                // Skip if the fragment's type doesn't exist in the schema
                if (!schema.Types.ContainsKey(fragment.OnType) &&
                    !schema.Interfaces.ContainsKey(fragment.OnType) &&
                    !schema.Unions.ContainsKey(fragment.OnType))
                {
                    Console.WriteLine($"Warning: Type {fragment.OnType} not found in schema");
                    continue;
                }

                // Enhance fields with schema information
                EnhanceFieldsWithSchema(fragment.Fields, fragment.OnType, schema);
            }
        }

        /// <summary>
        /// Enhance fields with schema information (simplified version)
        /// </summary>
        private static void EnhanceFieldsWithSchema(List<GraphQLField> fields, string parentTypeName, GraphQLSchema schema)
        {
            foreach (var field in fields)
            {
                // Handle inline fragments
                if (field.InlineFragmentType != null)
                {
                    // For inline fragments, use the specified type for the nested fields
                    // First, check if the inline fragment type is valid for this parent type
                    bool isValidType = false;
                    
                    // Check if parent is an interface and the inline fragment type implements it
                    if (schema.Interfaces.TryGetValue(parentTypeName, out var interfaceDef))
                    {
                        // Find all types that implement this interface
                        foreach (var type in schema.Types.Values)
                        {
                            if (type.Interfaces.Contains(parentTypeName) && type.Name == field.InlineFragmentType)
                            {
                                isValidType = true;
                                break;
                            }
                        }
                    }
                    // Check if parent is a union and the inline fragment type is a member
                    else if (schema.Unions.TryGetValue(parentTypeName, out var unionDef))
                    {
                        isValidType = unionDef.PossibleTypes.Contains(field.InlineFragmentType);
                    }
                    // If parent is a concrete type, the inline fragment type must match
                    else if (schema.Types.ContainsKey(parentTypeName))
                    {
                        isValidType = parentTypeName == field.InlineFragmentType;
                    }
                    
                    if (isValidType)
                    {
                        // Use the concrete type for the nested fields
                        EnhanceFieldsWithSchema(field.SelectionSet, field.InlineFragmentType, schema);
                    }
                    continue;
                }
                
                // Skip fields with fragment spreads
                if (field.FragmentSpreads.Any())
                {
                    // We still need to set the type for fields with fragment spreads
                    var fragmentFieldDef = schema.GetFieldDefinition(parentTypeName, field.Name);
                    if (fragmentFieldDef != null)
                    {
                        field.Type = fragmentFieldDef.Type;
                    }
                    continue;
                }

                // Get field definition from schema
                var fieldDefinition = schema.GetFieldDefinition(parentTypeName, field.Name);
                if (fieldDefinition != null)
                {
                    // Update field type information
                    field.Type = fieldDefinition.Type;

                    // Update deprecation information if not already set
                    if (!field.IsDeprecated)
                    {
                        field.IsDeprecated = fieldDefinition.IsDeprecated;
                        field.DeprecationReason = fieldDefinition.DeprecationReason;
                    }

                    // Recursively enhance nested fields
                    if (field.SelectionSet.Any() && fieldDefinition.Type != null)
                    {
                        string nestedTypeName = GetTypeName(fieldDefinition.Type);
                        
                        // Handle interface and union types
                        if (schema.Interfaces.ContainsKey(nestedTypeName))
                        {
                            // For interfaces, we need to check the implementing types
                            EnhanceFieldsWithSchema(field.SelectionSet, nestedTypeName, schema);
                        }
                        else if (schema.Unions.ContainsKey(nestedTypeName))
                        {
                            // For unions, we need to check all possible types
                            EnhanceFieldsWithSchema(field.SelectionSet, nestedTypeName, schema);
                        }
                        else
                        {
                            // For regular types
                            EnhanceFieldsWithSchema(field.SelectionSet, nestedTypeName, schema);
                        }
                        
                        // After enhancing nested fields, we need to check each nested field
                        // to ensure its type information is properly set
                        foreach (var nestedField in field.SelectionSet)
                        {
                            // Skip inline fragments, they're handled separately
                            if (nestedField.InlineFragmentType != null)
                            {
                                continue;
                            }
                            
                            // Get the field definition from the schema
                            var nestedFieldDef = schema.GetFieldDefinition(nestedTypeName, nestedField.Name);
                            if (nestedFieldDef != null)
                            {
                                // Create a deep copy of the type to ensure we don't have reference issues
                                nestedField.Type = CloneGraphQLType(nestedFieldDef.Type);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Field {field.Name} not found in type {parentTypeName}");
                }
            }
        }

        /// <summary>
        /// Creates a deep copy of a GraphQLType to avoid reference issues
        /// </summary>
        private static GraphQLType CloneGraphQLType(GraphQLType type)
        {
            if (type == null)
            {
                return null;
            }

            return new GraphQLType
            {
                Name = type.Name,
                IsNullable = type.IsNullable,
                IsList = type.IsList,
                OfType = type.OfType != null ? CloneGraphQLType(type.OfType) : null
            };
        }

        /// <summary>
        /// Get the base type name from a GraphQL type
        /// </summary>
        private static string GetTypeName(GraphQLType type)
        {
            if (type.IsList && type.OfType != null)
            {
                return GetTypeName(type.OfType);
            }
            return type.Name;
        }

        /// <summary>
        /// Display a field with its type information
        /// </summary>
        private static void DisplayField(GraphQLField field, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);
            string typeInfo = FormatTypeInfo(field.Type);
            string deprecationInfo = field.IsDeprecated ? " @deprecated" + (field.DeprecationReason != null ? $"(reason: \"{field.DeprecationReason}\")" : "") : "";
            
            Console.WriteLine($"{indent}{field.Name}: {typeInfo}{deprecationInfo}");
            
            if (field.SelectionSet.Any())
            {
                foreach (var nestedField in field.SelectionSet)
                {
                    DisplayField(nestedField, indentLevel + 1);
                }
            }
        }

        /// <summary>
        /// Format type information for display
        /// </summary>
        private static string FormatTypeInfo(GraphQLType? type)
        {
            if (type == null)
            {
                return "unknown";
            }
            
            if (type.IsList)
            {
                return $"[{FormatTypeInfo(type.OfType)}]{(type.IsNullable ? "" : "!")}";
            }
            
            return $"{type.Name}{(type.IsNullable ? "" : "!")}";
        }
    }
}