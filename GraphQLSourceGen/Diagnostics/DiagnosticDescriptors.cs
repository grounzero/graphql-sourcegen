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
    }
}