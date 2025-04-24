using GraphQLSourceGen.Models;
using System.Text;

namespace GraphQLSourceGen.Parsing
{
    /// <summary>
    /// GraphQL fragments parser
    /// </summary>
    public class GraphQLParser
    {
        static readonly Dictionary<string, string> ScalarMappings = new Dictionary<string, string>
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
            try
            {
                var fragments = new List<GraphQLFragment>();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return fragments;
                }

                // Tokenize the content
                var tokens = Tokenize(content);
                int position = 0;

                // Parse fragments
                while (position < tokens.Count)
                {
                    if (position + 3 < tokens.Count &&
                        tokens[position].Value == "fragment" &&
                        tokens[position + 2].Value == "on")
                    {
                        try
                        {
                            var fragment = ParseFragment(tokens, ref position);
                            if (fragment != null)
                            {
                                fragments.Add(fragment);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing fragment: {ex.Message}");
                            // Skip to next fragment
                            while (position < tokens.Count && tokens[position].Value != "fragment")
                            {
                                position++;
                            }
                        }
                    }
                    else
                    {
                        position++;
                    }
                }

                return fragments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing GraphQL content: {ex.Message}");
                return new List<GraphQLFragment>();
            }
        }

        /// <summary>
        /// Tokenize GraphQL content
        /// </summary>
        private static List<Token> Tokenize(string content)
        {
            var tokens = new List<Token>();
            int position = 0;

            while (position < content.Length)
            {
                char c = content[position];

                // Skip whitespace
                if (char.IsWhiteSpace(c))
                {
                    position++;
                    continue;
                }

                // Skip comments
                if (c == '#')
                {
                    while (position < content.Length && content[position] != '\n')
                    {
                        position++;
                    }
                    continue;
                }

                // Handle punctuation
                if (c == '{' || c == '}' || c == ':' || c == '!' || c == '@' || c == '[' || c == ']')
                {
                    tokens.Add(new Token { Type = TokenType.Punctuation, Value = c.ToString() });
                    position++;
                    continue;
                }

                // Handle fragment spread
                if (c == '.' && position + 2 < content.Length && content[position + 1] == '.' && content[position + 2] == '.')
                {
                    tokens.Add(new Token { Type = TokenType.Spread, Value = "..." });
                    position += 3;
                    continue;
                }

                // Handle strings
                if (c == '"')
                {
                    int start = position;
                    position++; // Skip opening quote
                    while (position < content.Length && content[position] != '"')
                    {
                        // Handle escaped quotes
                        if (content[position] == '\\' && position + 1 < content.Length && content[position + 1] == '"')
                        {
                            position += 2;
                        }
                        else
                        {
                            position++;
                        }
                    }
                    position++; // Skip closing quote
                    tokens.Add(new Token { Type = TokenType.String, Value = content.Substring(start, position - start) });
                    continue;
                }

                // Handle identifiers and keywords
                if (char.IsLetter(c) || c == '_')
                {
                    int start = position;
                    while (position < content.Length && (char.IsLetterOrDigit(content[position]) || content[position] == '_'))
                    {
                        position++;
                    }
                    string value = content.Substring(start, position - start);
                    tokens.Add(new Token { Type = TokenType.Identifier, Value = value });
                    continue;
                }

                // Skip any other character
                position++;
            }

            return tokens;
        }

        /// <summary>
        /// Parse a GraphQL fragment
        /// </summary>
        private static GraphQLFragment ParseFragment(List<Token> tokens, ref int position)
        {
            // fragment Name on Type { ... }
            if (tokens[position].Value != "fragment")
            {
                throw new Exception("Expected 'fragment' keyword");
            }
            position++; // Skip 'fragment'

            // Get fragment name
            if (position >= tokens.Count || tokens[position].Type != TokenType.Identifier)
            {
                throw new Exception("Expected fragment name");
            }
            string fragmentName = tokens[position].Value;
            position++; // Skip fragment name

            // Expect 'on' keyword
            if (position >= tokens.Count || tokens[position].Value != "on")
            {
                throw new Exception("Expected 'on' keyword");
            }
            position++; // Skip 'on'

            // Get type name
            if (position >= tokens.Count || tokens[position].Type != TokenType.Identifier)
            {
                throw new Exception("Expected type name");
            }
            string typeName = tokens[position].Value;
            position++; // Skip type name

            // Expect opening brace
            if (position >= tokens.Count || tokens[position].Value != "{")
            {
                throw new Exception("Expected '{'");
            }
            position++; // Skip '{'

            // Parse fields
            var fields = ParseSelectionSet(tokens, ref position);

            // Create and return the fragment
            return new GraphQLFragment
            {
                Name = fragmentName,
                OnType = typeName,
                Fields = fields
            };
        }

        /// <summary>
        /// Parse a GraphQL selection set
        /// </summary>
        private static List<GraphQLField> ParseSelectionSet(List<Token> tokens, ref int position)
        {
            var fields = new List<GraphQLField>();

            // Parse fields until closing brace
            while (position < tokens.Count && tokens[position].Value != "}")
            {
                try
                {
                    // Check for fragment spread
                    if (tokens[position].Value == "...")
                    {
                        position++; // Skip '...'
                        if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
                        {
                            var spreadField = new GraphQLField();
                            spreadField.FragmentSpreads.Add(tokens[position].Value);
                            fields.Add(spreadField);
                            position++; // Skip fragment name
                        }
                        else
                        {
                            throw new Exception("Expected fragment name after spread operator");
                        }
                    }
                    // Parse field
                    else if (tokens[position].Type == TokenType.Identifier)
                    {
                        fields.Add(ParseField(tokens, ref position));
                    }
                    else
                    {
                        // Skip unexpected token
                        position++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing field: {ex.Message}");
                    // Skip to next field or closing brace
                    while (position < tokens.Count &&
                           tokens[position].Type != TokenType.Identifier &&
                           tokens[position].Value != "}" &&
                           tokens[position].Value != "...")
                    {
                        position++;
                    }
                }
            }

            // Skip closing brace
            if (position < tokens.Count && tokens[position].Value == "}")
            {
                position++;
            }

            return fields;
        }

        /// <summary>
        /// Parse a GraphQL field
        /// </summary>
        private static GraphQLField ParseField(List<Token> tokens, ref int position)
        {
            // Get field name
            string fieldName = tokens[position].Value;
            position++; // Skip field name

            // Check for arguments (skip them for now)
            if (position < tokens.Count && tokens[position].Value == "(")
            {
                int depth = 1;
                position++; // Skip '('
                while (position < tokens.Count && depth > 0)
                {
                    if (tokens[position].Value == "(")
                    {
                        depth++;
                    }
                    else if (tokens[position].Value == ")")
                    {
                        depth--;
                    }
                    position++;
                }
            }

            // Check for type annotation
            string fieldType = "";
            if (position < tokens.Count && tokens[position].Value == ":")
            {
                position++; // Skip ':'
                fieldType = ParseTypeAnnotation(tokens, ref position);
            }

            // Create the field
            var field = new GraphQLField
            {
                Name = fieldName,
                Type = ParseType(fieldType)
            };

            // Check for nested selection
            if (position < tokens.Count && tokens[position].Value == "{")
            {
                position++; // Skip '{'
                field.SelectionSet = ParseSelectionSet(tokens, ref position);
            }

            // Check for deprecated directive
            if (position < tokens.Count && tokens[position].Value == "@")
            {
                position++; // Skip '@'
                if (position < tokens.Count && tokens[position].Value == "deprecated")
                {
                    field.IsDeprecated = true;
                    position++; // Skip 'deprecated'

                    // Check for reason
                    if (position < tokens.Count && tokens[position].Value == "(")
                    {
                        position++; // Skip '('
                        if (position < tokens.Count && tokens[position].Value == "reason")
                        {
                            position++; // Skip 'reason'
                            if (position < tokens.Count && tokens[position].Value == ":")
                            {
                                position++; // Skip ':'
                                if (position < tokens.Count && tokens[position].Type == TokenType.String)
                                {
                                    // Extract reason from quoted string
                                    string quotedReason = tokens[position].Value;
                                    field.DeprecationReason = quotedReason.Substring(1, quotedReason.Length - 2);
                                    position++; // Skip reason string
                                }
                            }
                        }

                        // Skip to closing parenthesis
                        while (position < tokens.Count && tokens[position].Value != ")")
                        {
                            position++;
                        }
                        if (position < tokens.Count)
                        {
                            position++; // Skip ')'
                        }
                    }
                }
            }

            return field;
        }

        /// <summary>
        /// Parse a GraphQL type annotation
        /// </summary>
        private static string ParseTypeAnnotation(List<Token> tokens, ref int position)
        {
            StringBuilder typeBuilder = new StringBuilder();

            // Handle list type
            if (position < tokens.Count && tokens[position].Value == "[")
            {
                typeBuilder.Append('[');
                position++; // Skip '['

                // Parse inner type
                typeBuilder.Append(ParseTypeAnnotation(tokens, ref position));

                // Expect closing bracket
                if (position < tokens.Count && tokens[position].Value == "]")
                {
                    typeBuilder.Append(']');
                    position++; // Skip ']'
                }
            }
            // Handle named type
            else if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                typeBuilder.Append(tokens[position].Value);
                position++; // Skip type name
            }

            // Handle non-null
            if (position < tokens.Count && tokens[position].Value == "!")
            {
                typeBuilder.Append('!');
                position++; // Skip '!'
            }

            return typeBuilder.ToString();
        }

        /// <summary>
        /// Parse a GraphQL type
        /// </summary>
        static GraphQLType ParseType(string typeStr)
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

    /// <summary>
    /// Token types for the GraphQL lexer
    /// </summary>
    enum TokenType
    {
        Identifier,
        Punctuation,
        String,
        Spread
    }

    /// <summary>
    /// Token for the GraphQL lexer
    /// </summary>
    class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}