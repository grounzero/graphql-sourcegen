using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GraphQLSourceGen.Models;

namespace GraphQLSourceGen.Parsing
{
    /// <summary>
    /// A simple parser for GraphQL fragments
    /// </summary>
    public class GraphQLParser
    {
        private static readonly Regex FragmentRegex = new Regex(
            @"fragment\s+(?<name>\w+)\s+on\s+(?<type>\w+)\s*{(?<body>[^}]*)}",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex FieldRegex = new Regex(
            @"(?<name>\w+)(?:\s*\(.*?\))?\s*(?::(?<type>[^\s{,]*))?\s*(?<selection>{[^}]*})?(?<deprecated>@deprecated(?:\(reason:\s*""(?<reason>[^""]*)""\))?)?",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex FragmentSpreadRegex = new Regex(
            @"\.\.\.\s*(?<name>\w+)",
            RegexOptions.Compiled);

        private static readonly Dictionary<string, string> ScalarMappings = new Dictionary<string, string>
        {
            { "String", "string" },
            { "Int", "int" },
            { "Float", "double" },
            { "Boolean", "bool" },
            { "ID", "string" }, // Default to string, but can be configured
            { "DateTime", "DateTime" },
            { "Date", "DateOnly" },
            { "Time", "TimeOnly" }
        };

        /// <summary>
        /// Parse all GraphQL fragments from a file content
        /// </summary>
        /// <param name="fileContent">The content of the GraphQL file</param>
        /// <returns>List of parsed fragments</returns>
        public static List<GraphQLFragment> ParseFile(string fileContent)
        {
            return ParseContent(fileContent);
        }

        /// <summary>
        /// Parse all GraphQL fragments from a string
        /// </summary>
        public static List<GraphQLFragment> ParseContent(string content)
        {
            var fragments = new List<GraphQLFragment>();
            var matches = FragmentRegex.Matches(content);

            foreach (Match match in matches)
            {
                var fragment = new GraphQLFragment
                {
                    Name = match.Groups["name"].Value,
                    OnType = match.Groups["type"].Value,
                    Fields = ParseFields(match.Groups["body"].Value)
                };

                fragments.Add(fragment);
            }

            return fragments;
        }

        /// <summary>
        /// Parse fields from a GraphQL selection set
        /// </summary>
        private static List<GraphQLField> ParseFields(string selectionSet)
        {
            var fields = new List<GraphQLField>();
            var matches = FieldRegex.Matches(selectionSet);

            foreach (Match match in matches)
            {
                string name = match.Groups["name"].Value.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var field = new GraphQLField
                {
                    Name = name,
                    Type = ParseType(match.Groups["type"].Value),
                    IsDeprecated = match.Groups["deprecated"].Success,
                    DeprecationReason = match.Groups["reason"].Success ? match.Groups["reason"].Value : null
                };

                // Check for fragment spreads
                var spreadMatches = FragmentSpreadRegex.Matches(selectionSet);
                foreach (Match spreadMatch in spreadMatches)
                {
                    field.FragmentSpreads.Add(spreadMatch.Groups["name"].Value);
                }

                // Parse nested selection set if present
                if (match.Groups["selection"].Success)
                {
                    string nestedSelection = match.Groups["selection"].Value.Trim();
                    if (nestedSelection.StartsWith("{") && nestedSelection.EndsWith("}"))
                    {
                        nestedSelection = nestedSelection.Substring(1, nestedSelection.Length - 2);
                        field.SelectionSet = ParseFields(nestedSelection);
                    }
                }

                fields.Add(field);
            }

            return fields;
        }

        /// <summary>
        /// Parse a GraphQL type
        /// </summary>
        private static GraphQLType ParseType(string typeStr)
        {
            if (string.IsNullOrWhiteSpace(typeStr))
            {
                // Default to String if type is not specified
                return new GraphQLType { Name = "String", IsNullable = true };
            }

            bool isNullable = !typeStr.EndsWith("!");
            if (!isNullable)
            {
                typeStr = typeStr.Substring(0, typeStr.Length - 1);
            }

            bool isList = typeStr.StartsWith("[") && typeStr.EndsWith("]");
            if (isList)
            {
                typeStr = typeStr.Substring(1, typeStr.Length - 2);
                return new GraphQLType
                {
                    IsList = true,
                    IsNullable = isNullable,
                    OfType = ParseType(typeStr)
                };
            }

            return new GraphQLType
            {
                Name = typeStr,
                IsNullable = isNullable
            };
        }

        /// <summary>
        /// Map a GraphQL type to a C# type
        /// </summary>
        public static string MapToCSharpType(GraphQLType type)
        {
            if (type.IsList)
            {
                string elementType = MapToCSharpType(type.OfType!);
                return $"List<{elementType}>{(type.IsNullable ? "?" : "")}";
            }

            string csharpType;
            if (ScalarMappings.TryGetValue(type.Name, out var mappedType))
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