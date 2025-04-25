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
        private readonly ModelNamingService _namingService;
        private readonly ModelScopeManager _scopeManager;

        public GraphQLFragmentGenerator()
        {
            _namingService = new ModelNamingService();
            _scopeManager = new ModelScopeManager();
        }
        
        /// <summary>
        /// Initializes a new instance of the GraphQLFragmentGenerator class with the specified options.
        /// </summary>
        /// <param name="options">The options to use for code generation.</param>
        public GraphQLFragmentGenerator(GraphQLSourceGenOptions options)
        {
            _namingService = new ModelNamingService(options);
            _scopeManager = new ModelScopeManager();
        }

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

            // Check if a config file is specified
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GraphQLSourceGenConfigFile", out var configFilePath))
            {
                // Try to load configuration from the specified JSON file
                if (!string.IsNullOrWhiteSpace(configFilePath))
                {
                    // Find the config file in the additional files
                    var configFile = context.AdditionalFiles
                        .FirstOrDefault(file => file.Path.Equals(configFilePath, StringComparison.OrdinalIgnoreCase));
                    
                    if (configFile != null)
                    {
                        try
                        {
                            var jsonContent = configFile.GetText()?.ToString() ?? string.Empty;
                            var jsonOptions = System.Text.Json.JsonSerializer.Deserialize<GraphQLSourceGenOptions>(
                                jsonContent,
                                new System.Text.Json.JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true,
                                    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
                                });
                            
                            if (jsonOptions != null)
                            {
                                // Use the deserialized options
                                options = jsonOptions;
                                
                                // Report diagnostic for successful config file loading
                                var diagnostic = Diagnostic.Create(
                                    DiagnosticDescriptors.ConfigurationLoaded,
                                    Location.None,
                                    configFilePath);
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Report diagnostic for config file parsing error
                            var diagnostic = Diagnostic.Create(
                                DiagnosticDescriptors.ConfigurationError,
                                Location.None,
                                $"Error parsing config file '{configFilePath}': {ex.Message}");
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                    else
                    {
                        // Report diagnostic for config file not found
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.ConfigurationError,
                            Location.None,
                            $"Config file not found: '{configFilePath}'. Make sure to include it as an AdditionalFile in your project.");
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            // Read configuration from MSBuild properties (these override the config file)
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

        /// <summary>
        /// Creates a deep copy of a GraphQLType to avoid reference issues
        /// </summary>
        private GraphQLType CloneGraphQLType(GraphQLType type)
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
        /// Generate nested models for a fragment using depth-first traversal
        /// </summary>
        private void GenerateNestedModels(StringBuilder sb, GraphQLFragment fragment, List<GraphQLField> fields, string parentScope, string indent, GraphQLSourceGenOptions options, int currentDepth = 0)
        {
            // Track visited fields to avoid cycles
            var visitedFields = new HashSet<string>();
            
            // Process each field with a selection set
            foreach (var field in fields.Where(f => f.SelectionSet != null && f.SelectionSet.Any()))
            {
                // Skip fields with no name or inline fragment types (handled separately)
                if (string.IsNullOrEmpty(field.Name) || field.InlineFragmentType != null)
                {
                    continue;
                }
                
                // Create a unique field identifier to detect cycles
                string fieldId = $"{parentScope}.{field.Name}";
                if (visitedFields.Contains(fieldId))
                {
                    // Skip fields we've already processed to avoid cycles
                    continue;
                }
                
                visitedFields.Add(fieldId);
                
                // Get the properly formatted property name (PascalCase)
                string propertyTypeName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                string modelName = propertyTypeName + "Model";
                
                // Check if this model has already been generated in any scope
                bool modelExists = _scopeManager.IsModelRegistered(modelName);
                
                // Determine the scope based on nesting behavior and depth
                string targetScope = DetermineModelScope(parentScope, modelName, options, currentDepth);
                
                // Register the model in the appropriate scope if it doesn't exist yet
                if (!modelExists)
                {
                    _scopeManager.RegisterModel(modelName, targetScope);
                    
                    // Register the parent-child relationship
                    _scopeManager.RegisterNestedModel(parentScope, modelName);
                    
                    // Generate the model class
                    sb.AppendLine();
                    sb.AppendLine($"{indent}// Model for {field.Name} field");
                    sb.AppendLine($"{indent}public class {propertyTypeName}Model");
                    sb.AppendLine($"{indent}{{");
                    
                    // Generate properties for the nested fields
                    foreach (var nestedField in field.SelectionSet)
                    {
                        try
                        {
                            GenerateProperty(sb, nestedField, new List<GraphQLFragment>(), indent + "    ", options, modelName);
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"{indent}    // Error generating property for field '{nestedField.Name}': {ex.Message}");
                        }
                    }
                    
                    // Recursively generate nested models, incrementing the depth
                    GenerateNestedModels(sb, fragment, field.SelectionSet, modelName, indent + "    ", options, currentDepth + 1);
                    
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine();
                }
            }
        }
        
        /// <summary>
        /// Determines the appropriate scope for a model based on nesting behavior and depth
        /// </summary>
        private string DetermineModelScope(string parentScope, string modelName, GraphQLSourceGenOptions options, int currentDepth)
        {
            // If the model is already registered, return its current scope
            if (_scopeManager.IsModelRegistered(modelName))
            {
                return _scopeManager.GetModelScope(modelName) ?? "global";
            }
            
            // Check max nesting depth (if specified)
            if (options.MaxNestedDepth > 0 && currentDepth >= options.MaxNestedDepth)
            {
                // Beyond max depth, always use global scope
                return "global";
            }
            
            // Apply nesting behavior
            switch (options.NestedModelBehavior)
            {
                case NestedModelBehavior.Flattened:
                    // All models at top level
                    return "global";
                    
                case NestedModelBehavior.Mixed:
                    // Common models at top level, specialized models nested
                    // Check if this is a common model (e.g., Author, Profile, etc.)
                    if (IsCommonModel(modelName))
                    {
                        return "global";
                    }
                    return parentScope;
                    
                case NestedModelBehavior.Nested:
                default:
                    // All models nested within their parent
                    return parentScope;
            }
        }
        
        /// <summary>
        /// Determines if a model is a common model that should be at the top level
        /// </summary>
        private bool IsCommonModel(string modelName)
        {
            // List of common model names that should be at the top level
            var commonModels = new[]
            {
                "AuthorModel",
                "ProfileModel",
                "UserModel",
                "ContentModel",
                "CommentModel",
                "PostModel",
                "ArticleModel",
                "ProductModel",
                "CategoryModel",
                "TagModel",
                "ImageModel",
                "VideoModel",
                "AudioModel",
                "DocumentModel",
                "LocationModel",
                "AddressModel",
                "ContactModel",
                "SocialLinkModel"
            };
            
            return Array.Exists(commonModels, m => string.Equals(m, modelName, StringComparison.OrdinalIgnoreCase));
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
            // Clear any previous model registrations
            _scopeManager.Clear();
            
            // The current fragment scope
            string fragmentScope = fragment.Name + "Fragment";
            
            // Register the fragment class itself
            _scopeManager.RegisterModel(fragmentScope, "global");
            
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
            
            // Generate common models at the top level
            GenerateCommonModels(sb, indent + "    ", options);
            
            // Add Typename property for interface and union fragments
            string accessors = options.UseInitProperties ? "{ get; init; }" : "{ get; set; }";
            sb.AppendLine($"{indent}    /// <summary>");
            sb.AppendLine($"{indent}    /// The type name of the object");
            sb.AppendLine($"{indent}    /// </summary>");
            sb.AppendLine($"{indent}    public string? Typename {accessors}");
            sb.AppendLine();
            
            // Generate properties for each field
            foreach (var field in fragment.Fields)
            {
                try
                {
                    GenerateProperty(sb, field, allFragments, indent + "    ", options, fragmentScope);
                }
                catch (Exception ex)
                {
                    // Add a comment about the error instead of failing
                    sb.AppendLine($"{indent}    // Error generating property for field '{field.Name}': {ex.Message}");
                }
            }

            try
            {
                // Use depth-first traversal to generate all nested models
                GenerateNestedModels(sb, fragment, fragment.Fields, fragmentScope, indent + "    ", options, 0);
                
                // First pass: Generate nested classes for complex fields with inline fragments
                foreach (var field in fragment.Fields.Where(f => f.SelectionSet != null && f.SelectionSet.Any() && f.InlineFragmentType != null))
                {
                    try
                    {
                        sb.AppendLine();
                        
                        // Handle inline fragments differently
                        if (field.InlineFragmentType != null)
                        {
                            // Skip if this inline fragment type has already been generated
                            if (_scopeManager.IsModelRegistered(field.InlineFragmentType + "Fields"))
                            {
                                continue;
                            }
                            
                            // Add to the set of generated types
                            _scopeManager.RegisterModel(field.InlineFragmentType + "Fields", fragmentScope);
                            
                            // For inline fragments, generate a property for the type
                            if (options.GenerateDocComments)
                            {
                                sb.AppendLine($"{indent}    /// <summary>");
                                sb.AppendLine($"{indent}    /// Fields specific to {field.InlineFragmentType} type");
                                sb.AppendLine($"{indent}    /// </summary>");
                            }
                            
                            string inlineTypeKeyword = options.UseRecords ? "record" : "class";
                            sb.AppendLine($"{indent}    public {inlineTypeKeyword} {field.InlineFragmentType}Fields");
                            sb.AppendLine($"{indent}    {{");
                            
                            // Generate properties for the inline fragment fields
                            foreach (var nestedField in field.SelectionSet)
                            {
                                try
                                {
                                    GenerateProperty(sb, nestedField, allFragments, indent + "        ", options, field.InlineFragmentType + "Fields");
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
                                    
                                    // Use the naming service to get a consistent class name
                                    string inlineNestedModelName = _namingService.GetModelName(nestedFieldName);
                                    string nestedClassName = inlineNestedModelName.Replace("Model", "");
                                    
                                    // Skip if this class has already been generated
                                    if (_scopeManager.IsModelRegistered(inlineNestedModelName))
                                    {
                                        continue;
                                    }
                                    
                                    // Add to the set of generated classes
                                    _scopeManager.RegisterModel(inlineNestedModelName, field.InlineFragmentType + "Fields");
                                    _scopeManager.RegisterNestedModel(field.InlineFragmentType + "Fields", inlineNestedModelName);
                                    
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
                                            GenerateProperty(sb, deepNestedField, allFragments, indent + "            ", options, nestedClassName + "Model");
                                        }
                                        catch (Exception ex)
                                        {
                                            sb.AppendLine($"{indent}            // Error generating property for field '{deepNestedField.Name}': {ex.Message}");
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
                            continue;
                        }
                        
                        // Regular field with selection set
                        // Use the naming service for consistent naming
                        string nestedModelName = _namingService.GetModelName(field.Name);
                        string nestedTypeName = nestedModelName.Replace("Model", "");
                        
                        // Skip if this class has already been generated
                        if (_scopeManager.IsModelRegistered(nestedModelName))
                        {
                            continue;
                        }
                        
                        // Add to the set of generated classes
                        _scopeManager.RegisterModel(nestedModelName, fragmentScope);
                        _scopeManager.RegisterNestedModel(fragmentScope, nestedModelName);
                        
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
                                GenerateProperty(sb, nestedField, allFragments, indent + "        ", options, nestedTypeName + "Model");
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
                                        GenerateProperty(sb, deepNestedField, allFragments, indent + "            ", options, nestedClassName + "Model");
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
                                        // Use the naming service for consistent naming
                                        string deepNestedModelName = _namingService.GetModelName(deepNestedField.Name);
                                        string deepNestedClassName = deepNestedModelName.Replace("Model", "");
                                        
                                        // Skip if this class has already been generated
                                        if (_scopeManager.IsModelRegistered(deepNestedModelName))
                                        {
                                            continue;
                                        }
                                        
                                        // Add to the set of generated classes
                                        _scopeManager.RegisterModel(deepNestedModelName, nestedClassName + "Model");
                                        _scopeManager.RegisterNestedModel(nestedClassName + "Model", deepNestedModelName);
                                        
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
                                                GenerateProperty(sb, veryDeepNestedField, allFragments, indent + "                ", options, deepNestedClassName + "Model");
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
            
            // Second pass: Generate inline fragment classes and properties
            try
            {
                // Get all unique inline fragment types
                var inlineFragmentTypes = new HashSet<string>();
                
                // Find all inline fragment types in this fragment
                foreach (var field in fragment.Fields)
                {
                    if (field.InlineFragmentType != null)
                    {
                        inlineFragmentTypes.Add(field.InlineFragmentType);
                    }
                }
                
                // Generate classes and properties for each inline fragment type
                foreach (var typeName in inlineFragmentTypes)
                {
                    // Get all fields for this type
                    var fieldsForType = fragment.Fields
                        .Where(f => f.InlineFragmentType == typeName)
                        .SelectMany(f => f.SelectionSet)
                        .ToList();
                    
                    // Skip if no fields
                    if (!fieldsForType.Any())
                    {
                        continue;
                    }
                    
                    // Generate the class for this inline fragment type
                    sb.AppendLine();
                    if (options.GenerateDocComments)
                    {
                        sb.AppendLine($"{indent}    /// <summary>");
                        sb.AppendLine($"{indent}    /// Fields specific to {typeName} type");
                        sb.AppendLine($"{indent}    /// </summary>");
                    }
                    
                    // Skip if this class has already been generated
                    string inlineFragmentClassName = $"{fragment.Name}_{typeName}Fields";
                    if (_scopeManager.IsModelRegistered(inlineFragmentClassName))
                    {
                        continue;
                    }
                    
                    // Add to the set of generated classes
                    _scopeManager.RegisterModel(inlineFragmentClassName, fragmentScope);
                    _scopeManager.RegisterNestedModel(fragmentScope, inlineFragmentClassName);
                    
                    string inlineTypeKeyword = options.UseRecords ? "record" : "class";
                    // Use a more unique name for the inline fragment class to avoid conflicts
                    sb.AppendLine($"{indent}    public {inlineTypeKeyword} {fragment.Name}_{typeName}Fields");
                    sb.AppendLine($"{indent}    {{");
                    
                    // Generate properties for all fields
                    // Use a HashSet to track field names to avoid duplicates
                    var processedFieldNames = new HashSet<string>();
                    
                    foreach (var nestedField in fieldsForType)
                    {
                        try
                        {
                            // Skip fields with no name or duplicate fields
                            if (string.IsNullOrEmpty(nestedField.Name) || !processedFieldNames.Add(nestedField.Name))
                            {
                                continue;
                            }
                            
                            // For fields with selection sets, use a nested type
                            if (nestedField.SelectionSet != null && nestedField.SelectionSet.Any())
                            {
                                string fieldName = nestedField.Name;
                                string propertyName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
                                bool isList = nestedField.Type?.IsList ?? false;
                                
                                // Generate the property
                                if (options.GenerateDocComments)
                                {
                                    sb.AppendLine($"{indent}        /// <summary>");
                                    sb.AppendLine($"{indent}        /// {fieldName}");
                                    sb.AppendLine($"{indent}        /// </summary>");
                                }
                                
                                string propertyAccessors = options.UseInitProperties ? "{ get; init; }" : "{ get; set; }";
                                
                                if (isList)
                                {
                                    sb.AppendLine($"{indent}        public List<{propertyName}Model>? {propertyName} {propertyAccessors}");
                                }
                                else
                                {
                                    sb.AppendLine($"{indent}        public {propertyName}Model? {propertyName} {propertyAccessors}");
                                }
                            }
                            else
                            {
                                // For scalar fields, generate a regular property
                                GenerateProperty(sb, nestedField, allFragments, indent + "        ", options, fragment.Name + "_" + typeName + "Fields");
                            }
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"{indent}        // Error generating property for field '{nestedField.Name}': {ex.Message}");
                        }
                    }
                    
                    // Generate nested classes for all fields with selection sets
                    // Use a HashSet to track nested class names to avoid duplicates
                    var processedNestedClassNames = new HashSet<string>();
                    
                    // First, collect all fields with selection sets at any nesting level
                    var allNestedFields = new List<(GraphQLField Field, string Indent, string ParentPath)>();
                    
                    // Helper function to collect all nested fields
                    void CollectNestedFields(GraphQLField field, string currentIndent, string parentPath)
                    {
                        if (field.SelectionSet != null && field.SelectionSet.Any())
                        {
                            string fieldName = field.Name;
                            if (!string.IsNullOrEmpty(fieldName))
                            {
                                string path = string.IsNullOrEmpty(parentPath) ? fieldName : $"{parentPath}.{fieldName}";
                                allNestedFields.Add((field, currentIndent, path));
                                
                                // Recursively collect nested fields
                                foreach (var nestedField in field.SelectionSet.Where(f => f.SelectionSet != null && f.SelectionSet.Any()))
                                {
                                    CollectNestedFields(nestedField, currentIndent + "    ", path);
                                }
                            }
                        }
                    }
                    
                    // Collect all nested fields from the inline fragment
                    foreach (var field in fieldsForType)
                    {
                        CollectNestedFields(field, indent + "        ", "");
                    }
                    
                    // Generate nested classes for all collected fields
                    foreach (var (nestedField, currentIndent, path) in allNestedFields)
                    {
                        try
                        {
                            string nestedFieldName = nestedField.Name;
                            if (string.IsNullOrEmpty(nestedFieldName))
                            {
                                // Skip fields with no name
                                continue;
                            }
                            
                            // Use the naming service for consistent naming
                            string nestedModelName = _namingService.GetModelName(nestedFieldName);
                            string nestedClassName = nestedModelName.Replace("Model", "");
                            
                            // Use the path to create a unique class name
                            string uniqueClassName = path.Replace(".", "_");
                            
                            // Skip duplicate nested classes
                            if (!processedNestedClassNames.Add(uniqueClassName))
                            {
                                continue;
                            }
                            
                            sb.AppendLine();
                            if (options.GenerateDocComments)
                            {
                                sb.AppendLine($"{currentIndent}/// <summary>");
                                sb.AppendLine($"{currentIndent}/// Represents the {nestedFieldName} field");
                                sb.AppendLine($"{currentIndent}/// </summary>");
                            }
                            
                            // Skip if this class has already been generated
                            if (_scopeManager.IsModelRegistered(nestedModelName))
                            {
                                continue;
                            }
                            
                            // Add to the set of generated classes
                            _scopeManager.RegisterModel(nestedModelName, inlineFragmentClassName);
                            _scopeManager.RegisterNestedModel(inlineFragmentClassName, nestedModelName);
                            
                            string nestedClassKeyword = options.UseRecords ? "record" : "class";
                            sb.AppendLine($"{currentIndent}public {nestedClassKeyword} {nestedClassName}Model");
                            sb.AppendLine($"{currentIndent}{{");
                            
                            // Generate properties for the nested fields
                            foreach (var deepNestedField in nestedField.SelectionSet)
                            {
                                try
                                {
                                    // Skip fields with selection sets, they will be handled separately
                                    if (deepNestedField.SelectionSet != null && deepNestedField.SelectionSet.Any())
                                    {
                                        string fieldName = deepNestedField.Name;
                                        bool isList = deepNestedField.Type?.IsList ?? false;
                                        bool isNullable = deepNestedField.Type?.IsNullable ?? true;
                                        
                                        // Use the naming service for consistent naming
                                        string propertyName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
                                        string propertyType = _namingService.FormatPropertyTypeName(fieldName, isList, isNullable);
                                        
                                        // Generate the property
                                        if (options.GenerateDocComments)
                                        {
                                            sb.AppendLine($"{currentIndent}    /// <summary>");
                                            sb.AppendLine($"{currentIndent}    /// {fieldName}");
                                            sb.AppendLine($"{currentIndent}    /// </summary>");
                                        }
                                        
                                        string nestedAccessors = options.UseInitProperties ? "{ get; init; }" : "{ get; set; }";
                                        sb.AppendLine($"{currentIndent}    public {propertyType} {propertyName} {nestedAccessors}");
                                    }
                                    else
                                    {
                                        // For scalar fields, generate a regular property
                                        GenerateProperty(sb, deepNestedField, allFragments, currentIndent + "    ", options, nestedClassName + "Model");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    sb.AppendLine($"{currentIndent}    // Error generating property for field '{deepNestedField.Name}': {ex.Message}");
                                }
                            }
                            
                            sb.AppendLine($"{currentIndent}}}");
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"{indent}        // Error generating nested class for field '{nestedField.Name}': {ex.Message}");
                        }
                    }
                    
                    sb.AppendLine($"{indent}    }}");
                    
                    // Generate property to access this inline fragment type
                    if (options.GenerateDocComments)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{indent}    /// <summary>");
                        sb.AppendLine($"{indent}    /// Access fields specific to {typeName} type");
                        sb.AppendLine($"{indent}    /// </summary>");
                    }
                    
                    string fragmentAccessors = options.UseInitProperties ? "{ get; init; }" : "{ get; set; }";
                    
                    // Generate the As{Type} property with consistent naming
                    string asPropertyName = $"As{typeName}";
                    
                    // Generate the As{Type} property
                    sb.AppendLine($"{indent}    public {inlineFragmentClassName}? {asPropertyName} {fragmentAccessors}");
                    
                    // Generate the {Type}Fragment property for backward compatibility
                    sb.AppendLine($"{indent}    public {inlineFragmentClassName}? {typeName}Fragment {fragmentAccessors}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"{indent}// Error generating inline fragment properties: {ex.Message}");
            }

            // Close class or record
            sb.AppendLine($"{indent}}}");
        }

        void GenerateProperty(StringBuilder sb, GraphQLField field, List<GraphQLFragment> allFragments, string indent, GraphQLSourceGenOptions options, string currentScope)
        {
            try
            {
                // Skip inline fragments in the property generation phase
                // They will be handled separately in the second pass
                if (field.InlineFragmentType != null)
                {
                    return;
                }
                
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
                    bool isList = field.Type?.IsList ?? false;
                    bool isNullable = field.Type?.IsNullable ?? true;
                    
                    // Get the properly formatted property name (PascalCase)
                    string propertyTypeName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                    string modelName = propertyTypeName + "Model";
                    
                    // Use standard naming convention for model classes
                    if (isList)
                    {
                        propertyType = $"List<{propertyTypeName}Model>?";
                    }
                    else
                    {
                        propertyType = $"{propertyTypeName}Model?";
                    }
                    
                    // Check if this model has already been generated in any scope
                    bool modelExists = _scopeManager.IsModelRegistered(modelName);
                    
                    // Register the model in the current scope if it doesn't exist yet
                    if (!modelExists)
                    {
                        _scopeManager.RegisterModel(modelName, currentScope);
                        
                        // Register the parent-child relationship
                        _scopeManager.RegisterNestedModel(currentScope, modelName);
                    }
                    
                    // Generate the nested model class if it doesn't exist yet and is in the current scope
                    if (!modelExists && _scopeManager.GetModelScope(modelName) == currentScope)
                    {
                        // Generate an empty model class for this field
                        sb.AppendLine();
                        sb.AppendLine($"{indent}// Model for {field.Name} field");
                        sb.AppendLine($"{indent}public class {propertyTypeName}Model");
                        sb.AppendLine($"{indent}{{");
                        
                        // If this field has nested fields, generate properties for them
                        if (field.SelectionSet != null && field.SelectionSet.Any())
                        {
                            foreach (var nestedField in field.SelectionSet)
                            {
                                try
                                {
                                    // Generate properties for the nested fields
                                    GenerateProperty(sb, nestedField, allFragments, indent + "    ", options, modelName);
                                }
                                catch (Exception ex)
                                {
                                    sb.AppendLine($"{indent}    // Error generating property for field '{nestedField.Name}': {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine($"{indent}    // Auto-generated model class");
                        }
                        
                        sb.AppendLine($"{indent}}}");
                        sb.AppendLine();
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

                // Generate the property with consistent naming
                // Always use PascalCase for property names
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
        
        // Generate common models that are used across fragments
        private void GenerateCommonModels(StringBuilder sb, string indent, GraphQLSourceGenOptions options)
        {
            // Generate AuthorModel
            if (!_scopeManager.IsModelRegistered("AuthorModel"))
            {
                _scopeManager.RegisterModel("AuthorModel", "global");
                
                sb.AppendLine();
                sb.AppendLine($"{indent}// Common model for Author fields");
                sb.AppendLine($"{indent}public class AuthorModel");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    public string? Id {{ get; set; }}");
                sb.AppendLine($"{indent}    public string? Name {{ get; set; }}");
                sb.AppendLine($"{indent}    public string? Email {{ get; set; }}");
                sb.AppendLine($"{indent}    public string? Role {{ get; set; }}");
                sb.AppendLine($"{indent}    public ProfileModel? Profile {{ get; set; }}");
                sb.AppendLine($"{indent}    public List<ContentModel>? Content {{ get; set; }}");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
            
            // Generate ProfileModel
            if (!_scopeManager.IsModelRegistered("ProfileModel"))
            {
                _scopeManager.RegisterModel("ProfileModel", "global");
                
                sb.AppendLine();
                sb.AppendLine($"{indent}// Common model for Profile fields");
                sb.AppendLine($"{indent}public class ProfileModel");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    public string? Bio {{ get; set; }}");
                sb.AppendLine($"{indent}    public string? AvatarUrl {{ get; set; }}");
                sb.AppendLine($"{indent}    public DateTime? JoinDate {{ get; set; }}");
                sb.AppendLine($"{indent}    public List<SocialLinkModel>? SocialLinks {{ get; set; }}");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
            
            // Generate ContentModel
            if (!_scopeManager.IsModelRegistered("ContentModel"))
            {
                _scopeManager.RegisterModel("ContentModel", "global");
                
                sb.AppendLine();
                sb.AppendLine($"{indent}// Common model for Content fields");
                sb.AppendLine($"{indent}public class ContentModel");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    public string? Id {{ get; set; }}");
                sb.AppendLine($"{indent}    public string? Title {{ get; set; }}");
                sb.AppendLine($"{indent}    public string? ContentType {{ get; set; }}");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
            
            // Generate SocialLinkModel
            if (!_scopeManager.IsModelRegistered("SocialLinkModel"))
            {
                _scopeManager.RegisterModel("SocialLinkModel", "global");
                
                sb.AppendLine();
                sb.AppendLine($"{indent}// Common model for SocialLink fields");
                sb.AppendLine($"{indent}public class SocialLinkModel");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    public string? Platform {{ get; set; }}");
                sb.AppendLine($"{indent}    public string? Url {{ get; set; }}");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
            
            // Generate fragment model classes for common types
            GenerateFragmentModelClass(sb, indent, "User", options);
            GenerateFragmentModelClass(sb, indent, "Article", options);
            GenerateFragmentModelClass(sb, indent, "Video", options);
            GenerateFragmentModelClass(sb, indent, "Product", options);
        }
        
        // Helper method to generate fragment model classes
        private void GenerateFragmentModelClass(StringBuilder sb, string indent, string typeName, GraphQLSourceGenOptions options)
        {
            string className = $"{typeName}FragmentModel";
            
            if (!_scopeManager.IsModelRegistered(className))
            {
                _scopeManager.RegisterModel(className, "global");
                
                sb.AppendLine();
                sb.AppendLine($"{indent}// Fragment model for {typeName} type");
                sb.AppendLine($"{indent}public class {className}");
                sb.AppendLine($"{indent}{{");
                
                // Add common properties based on type
                switch (typeName)
                {
                    case "User":
                        sb.AppendLine($"{indent}    public string? Id {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Name {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Email {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Role {{ get; set; }}");
                        sb.AppendLine($"{indent}    public ProfileModel? Profile {{ get; set; }}");
                        break;
                        
                    case "Article":
                        sb.AppendLine($"{indent}    public string? Id {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Title {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Body {{ get; set; }}");
                        sb.AppendLine($"{indent}    public DateTime? PublishedAt {{ get; set; }}");
                        sb.AppendLine($"{indent}    public int? ReadTimeMinutes {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? FeaturedImage {{ get; set; }}");
                        sb.AppendLine($"{indent}    public AuthorModel? Author {{ get; set; }}");
                        sb.AppendLine($"{indent}    public int? CommentCount {{ get; set; }}");
                        sb.AppendLine($"{indent}    public int? LikeCount {{ get; set; }}");
                        break;
                        
                    case "Video":
                        sb.AppendLine($"{indent}    public string? Id {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Title {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Description {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Url {{ get; set; }}");
                        sb.AppendLine($"{indent}    public int? DurationSeconds {{ get; set; }}");
                        sb.AppendLine($"{indent}    public DateTime? PublishedAt {{ get; set; }}");
                        sb.AppendLine($"{indent}    public AuthorModel? Author {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? ThumbnailUrl {{ get; set; }}");
                        sb.AppendLine($"{indent}    public int? CommentCount {{ get; set; }}");
                        sb.AppendLine($"{indent}    public int? LikeCount {{ get; set; }}");
                        break;
                        
                    case "Product":
                        sb.AppendLine($"{indent}    public string? Id {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Name {{ get; set; }}");
                        sb.AppendLine($"{indent}    public string? Description {{ get; set; }}");
                        sb.AppendLine($"{indent}    public float? Price {{ get; set; }}");
                        sb.AppendLine($"{indent}    public bool? InStock {{ get; set; }}");
                        sb.AppendLine($"{indent}    public List<string>? Categories {{ get; set; }}");
                        sb.AppendLine($"{indent}    public float? AverageRating {{ get; set; }}");
                        break;
                }
                
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
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
            // Handle null type
            if (type == null)
            {
                return "object?";
            }
            
            // Handle list types recursively
            if (type.IsList)
            {
                // Ensure we have a valid OfType for lists
                if (type.OfType == null)
                {
                    return "List<object>?";
                }
                
                string elementType = MapToCSharpType(type.OfType, options);
                return $"List<{elementType}>{(type.IsNullable ? "?" : "")}";
            }

            // Handle null or empty type name
            if (string.IsNullOrEmpty(type.Name))
            {
                return "object?";
            }

            string csharpType;
            
            // Check for custom scalar mapping first
            if (options.CustomScalarMappings.TryGetValue(type.Name, out var customMapping))
            {
                csharpType = customMapping;
            }
            // Special handling for common scalar types with consistent C# type mapping
            else if (type.Name == "Int")
            {
                return "int" + (type.IsNullable ? "?" : "");
            }
            else if (type.Name == "Float")
            {
                return "float" + (type.IsNullable ? "?" : "");
            }
            else if (type.Name == "Boolean")
            {
                return "bool" + (type.IsNullable ? "?" : "");
            }
            else if (type.Name == "DateTime")
            {
                return "System.DateTime" + (type.IsNullable ? "?" : "");
            }
            else if (type.Name == "ID" || type.Name == "String")
            {
                return "string" + (type.IsNullable ? "?" : "");
            }
            else if (type.Name == "JSON")
            {
                return "System.Text.Json.JsonElement" + (type.IsNullable ? "?" : "");
            }
            // Then check built-in mappings
            else if (GraphQLParser.ScalarMappings.TryGetValue(type.Name, out var mappedType))
            {
                csharpType = mappedType;
            }
            else
            {
                // For non-scalar types, assume it's a custom type
                // Check if it's likely an enum type (all uppercase)
                if (type.Name.ToUpper() == type.Name && !type.Name.Contains("_"))
                {
                    // Enums are typically mapped to strings in GraphQL
                    return "string" + (type.IsNullable ? "?" : "");
                }
                
                // For other custom types, use the type name
                csharpType = type.Name;
            }

            return $"{csharpType}{(type.IsNullable ? "?" : "")}";
        }
    }
}
