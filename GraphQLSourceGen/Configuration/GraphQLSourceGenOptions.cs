using System.Collections.Generic;

namespace GraphQLSourceGen.Configuration
{
    /// <summary>
    /// Configuration options for GraphQL Source Generator
    /// </summary>
    public class GraphQLSourceGenOptions
    {
        /// <summary>
        /// The namespace to use for generated types. If null, the namespace will be "GraphQL.Generated"
        /// </summary>
        public string? Namespace { get; set; }
        
        /// <summary>
        /// Whether to generate records (true) or classes (false)
        /// </summary>
        public bool UseRecords { get; set; } = true;
        
        /// <summary>
        /// Whether to use init-only properties
        /// </summary>
        public bool UseInitProperties { get; set; } = true;
        
        /// <summary>
        /// Whether to include XML documentation comments in generated code
        /// </summary>
        public bool GenerateDocComments { get; set; } = true;

        /// <summary>
        /// Whether to use schema information for type inference
        /// </summary>
        public bool UseSchemaForTypeInference { get; set; } = true;

        /// <summary>
        /// The paths to the GraphQL schema files
        /// </summary>
        public List<string> SchemaFilePaths { get; set; } = new List<string>();

        /// <summary>
        /// Custom scalar type mappings (GraphQL scalar name -> C# type name)
        /// </summary>
        public Dictionary<string, string> CustomScalarMappings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Whether to generate validation for non-nullable fields
        /// </summary>
        public bool ValidateNonNullableFields { get; set; } = true;

        /// <summary>
        /// Whether to include field descriptions from the schema in the generated code
        /// </summary>
        public bool IncludeFieldDescriptions { get; set; } = true;
    }
}