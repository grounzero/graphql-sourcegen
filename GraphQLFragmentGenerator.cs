using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using GraphQLSourceGen.Models;
using GraphQLSourceGen.Parsing;

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
                var fileContent = file.GetText()?.ToString() ?? string.Empty;
                var fragments = GraphQLParser.ParseFile(fileContent);
                allFragments.AddRange(fragments);
            }

            // Generate code for each fragment
            foreach (var fragment in allFragments)
            {
                string generatedCode = GenerateFragmentCode(fragment, allFragments);
                context.AddSource($"{fragment.Name}Fragment.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
            }
        }

        private string GenerateFragmentCode(GraphQLFragment fragment, List<GraphQLFragment> allFragments)
        {
            var sb = new StringBuilder();

            // Add using statements
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            

            // Add namespace
            sb.AppendLine("namespace GraphQL.Generated");
            sb.AppendLine("{");

            // Generate the record
            GenerateClass(sb, fragment, allFragments, "    ");

            // Close namespace
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void GenerateClass(StringBuilder sb, GraphQLFragment fragment, List<GraphQLFragment> allFragments, string indent)
        {
            // Class declaration
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Generated from GraphQL fragment '{fragment.Name}' on type '{fragment.OnType}'");
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}public class {fragment.Name}Fragment");
            sb.AppendLine($"{indent}{{");

            // Generate properties for each field
            foreach (var field in fragment.Fields)
            {
                GenerateProperty(sb, field, allFragments, indent + "    ");
            }

            // Generate nested records for complex fields
            foreach (var field in fragment.Fields.Where(f => f.SelectionSet.Any()))
            {
                sb.AppendLine();
                var nestedFragment = new GraphQLFragment
                {
                    Name = $"{fragment.Name}_{char.ToUpper(field.Name[0]) + field.Name.Substring(1)}",
                    OnType = field.Type.Name,
                    Fields = field.SelectionSet
                };

                GenerateClass(sb, nestedFragment, allFragments, indent + "    ");
            }

            // Close record
            sb.AppendLine($"{indent}}}");
        }

        private void GenerateProperty(StringBuilder sb, GraphQLField field, List<GraphQLFragment> allFragments, string indent)
        {
            // Add XML documentation
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// {field.Name}");
            sb.AppendLine($"{indent}/// </summary>");

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
            if (field.SelectionSet.Any())
            {
                // For fields with selection sets, use a nested record type
                string typeName = $"{char.ToUpper(field.Name[0]) + field.Name.Substring(1)}";
                bool isList = field.Type.IsList;
                bool isNullable = field.Type.IsNullable;

                if (isList)
                {
                    propertyType = $"List<{typeName}>{(isNullable ? "?" : "")}";
                }
                else
                {
                    propertyType = $"{typeName}{(isNullable ? "?" : "")}";
                }
            }
            else if (field.FragmentSpreads.Any())
            {
                // For fragment spreads, use the fragment type
                string spreadName = field.FragmentSpreads.First();
                propertyType = $"{spreadName}Fragment";
                if (field.Type.IsNullable)
                {
                    propertyType += "?";
                }
            }
            else
            {
                // For scalar fields, map to C# types
                propertyType = GraphQLParser.MapToCSharpType(field.Type);
            }

            // Generate the property
            string propertyName = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
            sb.AppendLine($"{indent}public {propertyType} {propertyName} {{ get; set; }}");
        }
    }
}