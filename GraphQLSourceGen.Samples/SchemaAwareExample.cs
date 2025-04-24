using GraphQL.Generated;
using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;
using System;
using System.Collections.Generic;
using System.IO;

namespace GraphQLSourceGen.Samples
{
    /// <summary>
    /// Example demonstrating the schema-aware fragment generation
    /// </summary>
    public class SchemaAwareExample
    {
        public static void Run()
        {
            Console.WriteLine("\nSchema-Aware Fragment Generation Example");
            Console.WriteLine("=========================================");

            // Step 1: Load the GraphQL schema
            string schemaContent = File.ReadAllText("schema-definition.graphql");
            var schema = GraphQLSchemaParser.ParseSchema(schemaContent);
            
            Console.WriteLine($"Loaded schema with:");
            Console.WriteLine($"- {schema.Types.Count} types");
            Console.WriteLine($"- {schema.Interfaces.Count} interfaces");
            Console.WriteLine($"- {schema.Unions.Count} unions");
            Console.WriteLine($"- {schema.Enums.Count} enums");
            Console.WriteLine($"- {schema.ScalarTypes.Count} scalar types");

            // Step 2: Define a GraphQL fragment
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
                    isPublished
                    tags
                    categories
                  }
                }
            ";

            // Step 3: Parse the fragment
            var fragments = GraphQLParser.ParseContent(fragmentContent);
            Console.WriteLine($"\nParsed fragment: {fragments[0].Name} on {fragments[0].OnType}");
            
            // Step 4: Enhance the fragment with schema information
            EnhanceFragmentsWithSchema(fragments, schema);
            
            // Step 5: Display the enhanced fragment with type information
            var fragment = fragments[0];
            Console.WriteLine("\nEnhanced fragment with type information:");
            
            foreach (var field in fragment.Fields)
            {
                DisplayField(field, 1);
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
                // Skip fields with fragment spreads
                if (field.FragmentSpreads.Any())
                {
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
                        EnhanceFieldsWithSchema(field.SelectionSet, nestedTypeName, schema);
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Field {field.Name} not found in type {parentTypeName}");
                }
            }
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