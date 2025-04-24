namespace GraphQLSourceGen.Models
{
    /// <summary>
    /// Represents a GraphQL type
    /// </summary>
    public class GraphQLType
    {
        public string Name { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsList { get; set; }
        public GraphQLType? OfType { get; set; }

        public override string ToString()
        {
            if (IsList)
            {
                return $"[{OfType}]{(IsNullable ? "" : "!")}";
            }
            return $"{Name}{(IsNullable ? "" : "!")}";
        }
    }

    /// <summary>
    /// Represents a field in a GraphQL fragment or type
    /// </summary>
    public class GraphQLField
    {
        public string Name { get; set; } = string.Empty;
        public GraphQLType Type { get; set; } = new GraphQLType();
        public bool IsDeprecated { get; set; }
        public string? DeprecationReason { get; set; }
        public List<GraphQLField> SelectionSet { get; set; } = [];
        public List<string> FragmentSpreads { get; set; } = [];
    }

    /// <summary>
    /// Represents a GraphQL fragment definition
    /// </summary>
    public class GraphQLFragment
    {
        public string Name { get; set; } = string.Empty;
        public string OnType { get; set; } = string.Empty;
        public List<GraphQLField> Fields { get; set; } = [];
    }
}