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

            // Parse schema if schema files are specified
            GraphQLSchema? schema = null;
            if (options.UseSchemaForTypeInference && options.SchemaFilePaths.Any())
            {
                schema = ParseSchemaFiles(context, options.SchemaFilePaths);
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

            // Enhance fragments with schema information if available
            if (schema != null)
            {
                EnhanceFragmentsWithSchema(allFragments, schema);
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

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenUseSchemaForTypeInference", out var useSchemaForTypeInference))
            {
                options.UseSchemaForTypeInference = bool.TryParse(useSchemaForTypeInference, out var value) && value;
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenValidateNonNullableFields", out var validateNonNullableFields))
            {
                options.ValidateNonNullableFields = bool.TryParse(validateNonNullableFields, out var value) && value;
            }

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenIncludeFieldDescriptions", out var includeFieldDescriptions))
            {
                options.IncludeFieldDescriptions = bool.TryParse(includeFieldDescriptions, out var value) && value;
            }

            // Read schema file paths
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenSchemaFiles", out var schemaFiles))
            {
                if (!string.IsNullOrWhiteSpace(schemaFiles))
                {
                    options.SchemaFilePaths = schemaFiles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }

            // Read custom scalar mappings
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenCustomScalarMappings", out var customScalarMappings))
            {
                if (!string.IsNullOrWhiteSpace(customScalarMappings))
                {
                    var mappings = customScalarMappings.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var mapping in mappings)
                    {
                        var parts = mapping.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            options.CustomScalarMappings[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }
            }

            return options;
        }

        /// <summary>
        /// Parse schema files and combine them into a single schema
        /// </summary>
        private GraphQLSchema ParseSchemaFiles(GeneratorExecutionContext context, List<string> schemaFilePaths)
        {
            var schema = new GraphQLSchema();

            foreach (var schemaFilePath in schemaFilePaths)
            {
                try
                {
                    // Find the schema file in the additional files
                    var schemaFile = context.AdditionalFiles
                        .FirstOrDefault(file => file.Path.EndsWith(schemaFilePath, StringComparison.OrdinalIgnoreCase));

                    if (schemaFile != null)
                    {
                        var schemaContent = schemaFile.GetText()?.ToString() ?? string.Empty;
                        var parsedSchema = Parsing.GraphQLSchemaParser.ParseSchema(schemaContent);

                        // Merge the parsed schema into the combined schema
                        MergeSchemas(schema, parsedSchema);
                    }
                    else
                    {
                        // Report diagnostic for schema file not found
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.SchemaFileNotFound,
                            Location.None,
                            schemaFilePath);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
                catch (Exception ex)
                {
                    // Report diagnostic for schema parsing error
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.InvalidSchemaDefinition,
                        Location.None,
                        ex.Message);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            return schema;
        }

        /// <summary>
        /// Merge two schemas together
        /// </summary>
        private void MergeSchemas(GraphQLSchema target, GraphQLSchema source)
        {
            // Merge types
            foreach (var type in source.Types)
            {
                target.Types[type.Key] = type.Value;
            }

            // Merge interfaces
            foreach (var iface in source.Interfaces)
            {
                target.Interfaces[iface.Key] = iface.Value;
            }

            // Merge unions
            foreach (var union in source.Unions)
            {
                target.Unions[union.Key] = union.Value;
            }

            // Merge enums
            foreach (var enumDef in source.Enums)
            {
                target.Enums[enumDef.Key] = enumDef.Value;
            }

            // Merge input types
            foreach (var input in source.InputTypes)
            {
                target.InputTypes[input.Key] = input.Value;
            }

            // Merge scalar types
            foreach (var scalar in source.ScalarTypes)
            {
                target.ScalarTypes[scalar.Key] = scalar.Value;
            }

            // Set operation type names if not already set
            if (target.QueryTypeName == null)
            {
                target.QueryTypeName = source.QueryTypeName;
            }

            if (target.MutationTypeName == null)
            {
                target.MutationTypeName = source.MutationTypeName;
            }

            if (target.SubscriptionTypeName == null)
            {
                target.SubscriptionTypeName = source.SubscriptionTypeName;
            }
        }

        /// <summary>
        /// Enhance fragments with schema information
        /// </summary>
        private void EnhanceFragmentsWithSchema(List<GraphQLFragment> fragments, GraphQLSchema schema)
        {
            foreach (var fragment in fragments)
            {
                // Skip if the fragment's type doesn't exist in the schema
                if (!schema.Types.ContainsKey(fragment.OnType) &&
                    !schema.Interfaces.ContainsKey(fragment.OnType) &&
                    !schema.Unions.ContainsKey(fragment.OnType))
                {
                    continue;
                }

                // Enhance fields with schema information
                EnhanceFieldsWithSchema(fragment.Fields, fragment.OnType, schema);
            }
        }

        /// <summary>
        /// Enhance fields with schema information
        /// </summary>
        private void EnhanceFieldsWithSchema(List<GraphQLField> fields, string parentTypeName, GraphQLSchema schema)
        {
            foreach (var field in fields)
            {
                // Handle fields with fragment spreads
                if (field.FragmentSpreads.Any())
                {
                    // We still need to set the type for fields with fragment spreads
                    // This is important for proper type inference in complex nested structures
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
                    }
                }
            }
        }

        /// <summary>
        /// Get the base type name from a GraphQL type
        /// </summary>
        private string GetTypeName(GraphQLType type)
        {
            if (type.IsList && type.OfType != null)
            {
                return GetTypeName(type.OfType);
            }
            return type.Name;
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
                    var baseType = MapToCSharpType(field.Type ?? new GraphQLType { Name = "String", IsNullable = true }, options);
                    
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

        /// <summary>
        /// Map a GraphQL type to a C# type, using custom scalar mappings if available
        /// </summary>
        private string MapToCSharpType(GraphQLType type, GraphQLSourceGenOptions options)
        {
            if (type.IsList)
            {
                string elementType = MapToCSharpType(type.OfType!, options);
                return $"List<{elementType}>{(type.IsNullable ? "?" : "")}";
            }

            string csharpType;
            
            // Check for custom scalar mapping first
            if (options.CustomScalarMappings.TryGetValue(type.Name, out var customMapping))
            {
                csharpType = customMapping;
            }
            // Then check built-in mappings
            else if (GraphQLParser.ScalarMappings.TryGetValue(type.Name, out var mappedType))
            {
                csharpType = mappedType;
            }
            else
            {
                // For non-scalar types, assume it's a custom type
                csharpType = type.Name;
            }

            return $"{csharpType}{(type.IsNullable ? "?" : "")}";
        }
    }
}