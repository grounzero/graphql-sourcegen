using Microsoft.CodeAnalysis;

namespace GraphQLSourceGen.Diagnostics
{
    /// <summary>
    /// Diagnostic descriptors for GraphQL Source Generator
    /// </summary>
    internal static class DiagnosticDescriptors
    {
        private const string AnalyzerReleaseTrackingId = "GQLSG";
        /// <summary>
        /// Invalid GraphQL syntax diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidGraphQLSyntax = new(
            id: "GQLSG001",
            title: "Invalid GraphQL syntax",
            messageFormat: "The GraphQL file contains invalid syntax: {0}",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The GraphQL file contains syntax that could not be parsed. Fix the syntax error to generate code.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// No GraphQL fragments found diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor NoFragmentsFound = new(
            id: "GQLSG002",
            title: "No GraphQL fragments found",
            messageFormat: "No GraphQL fragments were found in the file {0}",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The GraphQL file does not contain any fragment definitions. Add fragment definitions to generate code.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// Invalid fragment name diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFragmentName = new(
            id: "GQLSG003",
            title: "Invalid fragment name",
            messageFormat: "The fragment name '{0}' is not a valid C# identifier",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The fragment name must be a valid C# identifier. Rename the fragment to a valid C# identifier.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// Fragment spread not found diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor FragmentSpreadNotFound = new(
            id: "GQLSG004",
            title: "Fragment spread not found",
            messageFormat: "The fragment spread '{0}' was not found in any of the GraphQL files",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The fragment spread references a fragment that was not found in any of the GraphQL files. Make sure the fragment is defined in one of the GraphQL files.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// Schema file not found diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor SchemaFileNotFound = new(
            id: "GQLSG005",
            title: "Schema file not found",
            messageFormat: "The GraphQL schema file '{0}' was not found",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The specified GraphQL schema file was not found. Make sure the file exists and is included in the project.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// Invalid schema definition diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidSchemaDefinition = new(
            id: "GQLSG006",
            title: "Invalid schema definition",
            messageFormat: "The GraphQL schema contains invalid syntax: {0}",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The GraphQL schema contains syntax that could not be parsed. Fix the syntax error to generate code.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// Type not found in schema diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor TypeNotFoundInSchema = new(
            id: "GQLSG007",
            title: "Type not found in schema",
            messageFormat: "The type '{0}' referenced in fragment '{1}' was not found in the schema",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The fragment references a type that was not found in the schema. Make sure the type is defined in the schema.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// Field not found in type diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor FieldNotFoundInType = new(
            id: "GQLSG008",
            title: "Field not found in type",
            messageFormat: "The field '{0}' referenced in fragment '{1}' was not found in type '{2}'",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The fragment references a field that was not found in the type. Make sure the field is defined in the type.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);

        /// <summary>
        /// Invalid fragment on interface or union diagnostic
        /// </summary>
        public static readonly DiagnosticDescriptor InvalidFragmentOnInterfaceOrUnion = new(
            id: "GQLSG009",
            title: "Invalid fragment on interface or union",
            messageFormat: "The fragment '{0}' is defined on interface or union type '{1}' but does not include __typename field",
            category: "GraphQLSourceGen",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Fragments on interface or union types should include the __typename field to enable proper type resolution.",
            helpLinkUri: $"https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md",
            customTags: [WellKnownDiagnosticTags.AnalyzerException]);
    }
}