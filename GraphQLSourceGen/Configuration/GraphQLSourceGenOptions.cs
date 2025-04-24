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
    }
}