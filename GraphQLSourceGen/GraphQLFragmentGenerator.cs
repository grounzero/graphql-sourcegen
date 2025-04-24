using GraphQLSourceGen.Configuration;
using GraphQLSourceGen.Diagnostics;
using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace GraphQLSourceGen
{
    [Generator]
    public class GraphQLFragmentGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Not required
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Read configuration from MSBuild properties
            var options = ReadConfiguration(context);

            // Find all .graphql files in the project
            var graphqlFiles = context.AdditionalFiles
                .Where(file => file.Path.EndsWith(".graphql", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!graphqlFiles.Any())
            {
                // No .graphql files found
                return;
            }

            // Parse all fragments from all files
            var allFragments = new List<GraphQLFragment>();
            foreach (var file in graphqlFiles)
            {
                try
                {
                    var fileContent = file.GetText()?.ToString() ?? string.Empty;
                    var fragments = GraphQLParser.ParseFile(fileContent);

                    if (!fragments.Any())
                    {
                        // Report diagnostic for no fragments found
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.NoFragmentsFound,
                            Location.None,
                            Path.GetFileName(file.Path));
                        context.ReportDiagnostic(diagnostic);
                        continue;
                    }

                    allFragments.AddRange(fragments);
                }
                catch (Exception ex)
                {
                    // Report diagnostic for parsing error
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.InvalidGraphQLSyntax,
                        Location.None,
                        ex.Message);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Validate fragment names and references
            ValidateFragments(context, allFragments);

            // Generate code for each fragment
            foreach (var fragment in allFragments)
            {
                try
                {
                    string generatedCode = GenerateFragmentCode(fragment, allFragments, options);
                    context.AddSource($"{fragment.Name}Fragment.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    // Report diagnostic for code generation error
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.InvalidGraphQLSyntax,
                        Location.None,
                        $"Error generating code for fragment '{fragment.Name}': {ex.Message}");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void ValidateFragments(GeneratorExecutionContext context, List<GraphQLFragment> fragments)
        {
            // Check for invalid fragment names
            foreach (var fragment in fragments)
            {
                if (!IsValidCSharpIdentifier(fragment.Name))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.InvalidFragmentName,
                        Location.None,
                        fragment.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Check for fragment spreads that don't exist
            HashSet<string> fragmentNames = [.. fragments.Select(f => f.Name)];
            foreach (var fragment in fragments)
            {
                foreach (var field in fragment.Fields)
                {
                    foreach (var spread in field.FragmentSpreads)
                    {
                        if (!fragmentNames.Contains(spread))
                        {
                            var diagnostic = Diagnostic.Create(
                                DiagnosticDescriptors.FragmentSpreadNotFound,
                                Location.None,
                                spread);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        private bool IsValidCSharpIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            if (!char.IsLetter(name[0]) && name[0] != '_')
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                    return false;
            }

            return true;
        }

        private GraphQLSourceGenOptions ReadConfiguration(GeneratorExecutionContext context)
        {
            var options = new GraphQLSourceGenOptions();

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenNamespace", out var ns))
            {
                options.Namespace = ns;
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenUseRecords", out var useRecords))
            {
                options.UseRecords = bool.TryParse(useRecords, out var value) && value;
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenUseInitProperties", out var useInitProperties))
            {
                options.UseInitProperties = bool.TryParse(useInitProperties, out var value) && value;
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenGenerateDocComments", out var generateDocComments))
            {
                options.GenerateDocComments = bool.TryParse(generateDocComments, out var value) && value;
            }

            return options;
        }

        string GenerateFragmentCode(GraphQLFragment fragment, List<GraphQLFragment> allFragments, GraphQLSourceGenOptions options)
        {
            var sb = new StringBuilder();

            // Add using statements and nullable directive
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            // Add namespace
            string ns = options.Namespace ?? "GraphQL.Generated";
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");

            // Generate the class or record
            GenerateClass(sb, fragment, allFragments, "    ", options);

            // Close namespace
            sb.AppendLine("}");

            return sb.ToString();
        }

        void GenerateClass(StringBuilder sb, GraphQLFragment fragment, List<GraphQLFragment> allFragments, string indent, GraphQLSourceGenOptions options)
        {
            // Class or record declaration
            if (options.GenerateDocComments)
            {
                sb.AppendLine($"{indent}/// <summary>");
                sb.AppendLine($"{indent}/// Generated from GraphQL fragment '{fragment.Name}' on type '{fragment.OnType}'");
                sb.AppendLine($"{indent}/// </summary>");
            }

            string typeKeyword = options.UseRecords ? "record" : "class";
            sb.AppendLine($"{indent}public {typeKeyword} {fragment.Name}Fragment");
            sb.AppendLine($"{indent}{{");

            // Generate properties for each field
            foreach (var field in fragment.Fields)
            {
                try
                {
                    GenerateProperty(sb, field, allFragments, indent + "    ", options);
                }
                catch (Exception ex)
                {
                    // Add a comment about the error instead of failing
                    sb.AppendLine($"{indent}    // Error generating property for field '{field.Name}': {ex.Message}");
                }
            }

            try
            {
                // Generate nested classes for complex fields
                foreach (var field in fragment.Fields.Where(f => f.SelectionSet != null && f.SelectionSet.Any()))
                {
                    try
                    {
                        sb.AppendLine();
                        string nestedTypeName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                        
                        if (options.GenerateDocComments)
                        {
                            sb.AppendLine($"{indent}    /// <summary>");
                            sb.AppendLine($"{indent}    /// Represents the {field.Name} field of {fragment.Name}");
                            sb.AppendLine($"{indent}    /// </summary>");
                        }
                        
                        string nestedTypeKeyword = options.UseRecords ? "record" : "class";
                        sb.AppendLine($"{indent}    public {nestedTypeKeyword} {nestedTypeName}Model");
                        sb.AppendLine($"{indent}    {{");
                        
                        // Generate properties for nested fields
                        foreach (var nestedField in field.SelectionSet)
                        {
                            try
                            {
                                GenerateProperty(sb, nestedField, allFragments, indent + "        ", options);
                            }
                            catch (Exception ex)
                            {
                                sb.AppendLine($"{indent}        // Error generating property for field '{nestedField.Name}': {ex.Message}");
                            }
                        }
                        
                        // Generate nested classes for nested fields with selection sets
                        foreach (var nestedField in field.SelectionSet.Where(f => f.SelectionSet != null && f.SelectionSet.Any()))
                        {
                            try
                            {
                                sb.AppendLine();
                                string nestedFieldName = nestedField.Name;
                                if (string.IsNullOrEmpty(nestedFieldName))
                                {
                                    // Skip fields with no name
                                    continue;
                                }
                                
                                // Use the field name directly for the nested class name
                                string nestedClassName = char.ToUpper(nestedFieldName[0]) + nestedFieldName.Substring(1);
                                
                                if (options.GenerateDocComments)
                                {
                                    sb.AppendLine($"{indent}        /// <summary>");
                                    sb.AppendLine($"{indent}        /// Represents the {nestedFieldName} field");
                                    sb.AppendLine($"{indent}        /// </summary>");
                                }
                                
                                string nestedClassKeyword = options.UseRecords ? "record" : "class";
                                sb.AppendLine($"{indent}        public {nestedClassKeyword} {nestedClassName}Model");
                                sb.AppendLine($"{indent}        {{");
                                
                                // Generate properties for the nested fields
                                foreach (var deepNestedField in nestedField.SelectionSet)
                                {
                                    try
                                    {
                                        GenerateProperty(sb, deepNestedField, allFragments, indent + "            ", options);
                                    }
                                    catch (Exception ex)
                                    {
                                        sb.AppendLine($"{indent}            // Error generating property for field '{deepNestedField.Name}': {ex.Message}");
                                    }
                                }
                                
                                // Generate nested classes for deeply nested fields
                                foreach (var deepNestedField in nestedField.SelectionSet.Where(f => f.SelectionSet != null && f.SelectionSet.Any()))
                                {
                                    try
                                    {
                                        sb.AppendLine();
                                        string deepNestedClassName = char.ToUpper(deepNestedField.Name[0]) + deepNestedField.Name.Substring(1);
                                        
                                        if (options.GenerateDocComments)
                                        {
                                            sb.AppendLine($"{indent}            /// <summary>");
                                            sb.AppendLine($"{indent}            /// Represents the {deepNestedField.Name} field");
                                            sb.AppendLine($"{indent}            /// </summary>");
                                        }
                                        
                                        string deepNestedClassKeyword = options.UseRecords ? "record" : "class";
                                        sb.AppendLine($"{indent}            public {deepNestedClassKeyword} {deepNestedClassName}Model");
                                        sb.AppendLine($"{indent}            {{");
                                        
                                        // Generate properties for the deeply nested fields
                                        foreach (var veryDeepNestedField in deepNestedField.SelectionSet)
                                        {
                                            try
                                            {
                                                GenerateProperty(sb, veryDeepNestedField, allFragments, indent + "                ", options);
                                            }
                                            catch (Exception ex)
                                            {
                                                sb.AppendLine($"{indent}                // Error generating property for field '{veryDeepNestedField.Name}': {ex.Message}");
                                            }
                                        }
                                        
                                        sb.AppendLine($"{indent}            }}");
                                    }
                                    catch (Exception ex)
                                    {
                                        sb.AppendLine($"{indent}            // Error generating nested class for field '{deepNestedField.Name}': {ex.Message}");
                                    }
                                }
                                
                                sb.AppendLine($"{indent}        }}");
                            }
                            catch (Exception ex)
                            {
                                sb.AppendLine($"{indent}        // Error generating nested class for field '{nestedField.Name}': {ex.Message}");
                            }
                        }
                        
                        sb.AppendLine($"{indent}    }}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"{indent}    // Error generating nested class for field '{field.Name}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"{indent}// Error generating nested classes: {ex.Message}");
            }

            // Close class or record
            sb.AppendLine($"{indent}}}");
        }

        void GenerateProperty(StringBuilder sb, GraphQLField field, List<GraphQLFragment> allFragments, string indent, GraphQLSourceGenOptions options)
        {
            try
            {
                // Skip fields with empty names
                if (string.IsNullOrWhiteSpace(field.Name))
                {
                    return;
                }

                // Add XML documentation
                if (options.GenerateDocComments)
                {
                    sb.AppendLine($"{indent}/// <summary>");
                    sb.AppendLine($"{indent}/// {field.Name}");
                    sb.AppendLine($"{indent}/// </summary>");
                }

                // Add [Obsolete] attribute if the field is deprecated
                if (field.IsDeprecated)
                {
                    string reason = field.DeprecationReason != null
                        ? $", \"{field.DeprecationReason}\""
                        : string.Empty;
                    sb.AppendLine($"{indent}[Obsolete(\"This field is deprecated{reason}\")]");
                }

                // Determine property type
                string propertyType;
                if (field.SelectionSet != null && field.SelectionSet.Any())
                {
                    // For fields with selection sets, use a nested type
                    string typeName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                    bool isList = field.Type?.IsList ?? false;
                    
                    // Always make properties nullable to avoid warnings
                    if (isList)
                    {
                        propertyType = $"List<{typeName}Model>?";
                    }
                    else
                    {
                        propertyType = $"{typeName}Model?";
                    }
                }
                else if (field.FragmentSpreads != null && field.FragmentSpreads.Any())
                {
                    // For fragment spreads, use the fragment type
                    string spreadName = field.FragmentSpreads.First();
                    propertyType = $"{spreadName}Fragment?";
                }
                else
                {
                    // For scalar fields, map to C# types and make them nullable
                    var baseType = GraphQLParser.MapToCSharpType(field.Type ?? new GraphQLType { Name = "String", IsNullable = true });
                    
                    // If it's not already nullable and not a value type, make it nullable
                    if (!baseType.EndsWith("?") && !IsValueType(baseType))
                    {
                        propertyType = baseType + "?";
                    }
                    else
                    {
                        propertyType = baseType;
                    }
                }

                // Generate the property
                string propertyName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                string accessors = options.UseInitProperties ? "{ get; init; }" : "{ get; set; }";
                
                sb.AppendLine($"{indent}public {propertyType} {propertyName} {accessors}");
            }
            catch (Exception ex)
            {
                // Add a comment about the error instead of failing
                sb.AppendLine($"{indent}// Error generating property: {ex.Message}");
            }
        }
        
        // Helper method to determine if a type is a value type
        private bool IsValueType(string typeName)
        {
            return typeName == "int" ||
                   typeName == "long" ||
                   typeName == "float" ||
                   typeName == "double" ||
                   typeName == "decimal" ||
                   typeName == "bool" ||
                   typeName == "DateTime" ||
                   typeName == "Guid" ||
                   typeName == "TimeSpan" ||
                   typeName == "DateTimeOffset" ||
                   typeName == "byte" ||
                   typeName == "sbyte" ||
                   typeName == "short" ||
                   typeName == "ushort" ||
                   typeName == "uint" ||
                   typeName == "ulong" ||
                   typeName == "char";
        }
    }
}