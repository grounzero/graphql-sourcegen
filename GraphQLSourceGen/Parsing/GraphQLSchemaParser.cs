using GraphQLSourceGen.Models;
using System.Text;

namespace GraphQLSourceGen.Parsing
{
    /// <summary>
    /// Parser for GraphQL schema definitions
    /// </summary>
    public class GraphQLSchemaParser
    {
        /// <summary>
        /// Parse a GraphQL schema from a string
        /// </summary>
        /// <param name="schemaContent">The content of the GraphQL schema</param>
        /// <returns>The parsed GraphQL schema</returns>
        public static GraphQLSchema ParseSchema(string schemaContent)
        {
            var schema = new GraphQLSchema();
            if (string.IsNullOrWhiteSpace(schemaContent))
            {
                return schema;
            }

            // Tokenize the content
            var tokens = GraphQLParser.Tokenize(schemaContent);
            int position = 0;

            // Parse schema definitions
            while (position < tokens.Count)
            {
                try
                {
                    // Skip comments and whitespace
                    if (position < tokens.Count && tokens[position].Type == TokenType.Comment)
                    {
                        position++;
                        continue;
                    }

                    // Parse schema definition
                    if (position < tokens.Count && tokens[position].Value == "schema")
                    {
                        ParseSchemaDefinition(tokens, ref position, schema);
                    }
                    // Parse type definition
                    else if (position < tokens.Count && tokens[position].Value == "type")
                    {
                        var typeDefinition = ParseTypeDefinition(tokens, ref position);
                        schema.Types[typeDefinition.Name] = typeDefinition;
                    }
                    // Parse interface definition
                    else if (position < tokens.Count && tokens[position].Value == "interface")
                    {
                        try
                        {
                            var interfaceDefinition = ParseInterfaceDefinition(tokens, ref position);
                            schema.Interfaces[interfaceDefinition.Name] = interfaceDefinition;
                        }
                        catch (Exception ex)
                        {
                            // Get the current token for context
                            string currentToken = position < tokens.Count ? tokens[position].Value : "end of file";
                            int lineNumber = GetLineNumber(tokens, position);
                            
                            // Create a more specific error message with context
                            string errorMessage = $"Error parsing interface at line {lineNumber}, near '{currentToken}': {ex.Message}";
                            Console.WriteLine(errorMessage);
                            
                            // Skip to next definition for better recovery
                            SkipToNextDefinition(tokens, ref position);
                        }
                    }
                    // Parse union definition
                    else if (position < tokens.Count && tokens[position].Value == "union")
                    {
                        try
                        {
                            var unionDefinition = ParseUnionDefinition(tokens, ref position);
                            schema.Unions[unionDefinition.Name] = unionDefinition;
                        }
                        catch (Exception ex)
                        {
                            // Get the current token for context
                            string currentToken = position < tokens.Count ? tokens[position].Value : "end of file";
                            int lineNumber = GetLineNumber(tokens, position);
                            
                            // Create a more specific error message with context
                            string errorMessage = $"Error parsing union at line {lineNumber}, near '{currentToken}': {ex.Message}";
                            Console.WriteLine(errorMessage);
                            
                            // Skip to next definition for better recovery
                            SkipToNextDefinition(tokens, ref position);
                        }
                    }
                    // Parse enum definition
                    else if (position < tokens.Count && tokens[position].Value == "enum")
                    {
                        var enumDefinition = ParseEnumDefinition(tokens, ref position);
                        schema.Enums[enumDefinition.Name] = enumDefinition;
                    }
                    // Parse input definition
                    else if (position < tokens.Count && tokens[position].Value == "input")
                    {
                        var inputDefinition = ParseInputDefinition(tokens, ref position);
                        schema.InputTypes[inputDefinition.Name] = inputDefinition;
                    }
                    // Parse scalar definition
                    else if (position < tokens.Count && tokens[position].Value == "scalar")
                    {
                        var scalarDefinition = ParseScalarDefinition(tokens, ref position);
                        schema.ScalarTypes[scalarDefinition.Name] = scalarDefinition;
                    }
                    // Skip unknown tokens
                    else
                    {
                        position++;
                    }
                }
                catch (Exception ex)
                {
                    // Get the current token for context
                    string currentToken = position < tokens.Count ? tokens[position].Value : "end of file";
                    int lineNumber = GetLineNumber(tokens, position);
                    
                    // Create a more specific error message with context
                    string errorMessage = $"Error parsing schema at line {lineNumber}, near '{currentToken}': {ex.Message}";
                    Console.WriteLine(errorMessage);
                    
                    // Skip to next definition for better recovery
                    SkipToNextDefinition(tokens, ref position);
                }
            }

            return schema;
        }

        /// <summary>
        /// Parse a schema definition
        /// </summary>
        private static void ParseSchemaDefinition(List<Token> tokens, ref int position, GraphQLSchema schema)
        {
            // Skip 'schema' keyword
            position++;

            // Expect opening brace
            if (position < tokens.Count && tokens[position].Value == "{")
            {
                position++;
            }
            else
            {
                throw new Exception("Expected '{' after 'schema'");
            }

            // Parse schema fields
            while (position < tokens.Count && tokens[position].Value != "}")
            {
                if (position + 2 < tokens.Count && tokens[position + 1].Value == ":")
                {
                    string operationType = tokens[position].Value;
                    position += 2; // Skip operation type and colon

                    if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
                    {
                        string typeName = tokens[position].Value;
                        position++; // Skip type name

                        // Set the appropriate type name
                        if (operationType == "query")
                        {
                            schema.QueryTypeName = typeName;
                        }
                        else if (operationType == "mutation")
                        {
                            schema.MutationTypeName = typeName;
                        }
                        else if (operationType == "subscription")
                        {
                            schema.SubscriptionTypeName = typeName;
                        }
                    }
                }
                else
                {
                    // Skip unexpected token
                    position++;
                }
            }

            // Skip closing brace
            if (position < tokens.Count && tokens[position].Value == "}")
            {
                position++;
            }
        }

        /// <summary>
        /// Parse a type definition
        /// </summary>
        private static GraphQLTypeDefinition ParseTypeDefinition(List<Token> tokens, ref int position)
        {
            // Skip 'type' keyword
            position++;

            // Get type name
            if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                string typeName = tokens[position].Value;
                position++; // Skip type name

                var typeDefinition = new GraphQLTypeDefinition
                {
                    Name = typeName
                };

                // Check for implements
                if (position < tokens.Count && tokens[position].Value == "implements")
                {
                    position++; // Skip 'implements'

                    // Parse implemented interfaces
                    while (position < tokens.Count && 
                          tokens[position].Type == TokenType.Identifier && 
                          tokens[position].Value != "{")
                    {
                        typeDefinition.Interfaces.Add(tokens[position].Value);
                        position++; // Skip interface name

                        // Skip '&' if present
                        if (position < tokens.Count && tokens[position].Value == "&")
                        {
                            position++;
                        }
                    }
                }

                // Expect opening brace
                if (position < tokens.Count && tokens[position].Value == "{")
                {
                    position++; // Skip '{'
                    
                    // Parse fields
                    typeDefinition.Fields = ParseFieldDefinitions(tokens, ref position);
                }

                return typeDefinition;
            }
            else
            {
                throw new Exception("Expected type name after 'type'");
            }
        }

        /// <summary>
        /// Parse an interface definition
        /// </summary>
        private static GraphQLInterfaceDefinition ParseInterfaceDefinition(List<Token> tokens, ref int position)
        {
            // Skip 'interface' keyword
            position++;

            // Get interface name
            if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                string interfaceName = tokens[position].Value;
                position++; // Skip interface name

                var interfaceDefinition = new GraphQLInterfaceDefinition
                {
                    Name = interfaceName
                };

                // Expect opening brace
                if (position < tokens.Count && tokens[position].Value == "{")
                {
                    position++; // Skip '{'
                    
                    // Parse fields
                    interfaceDefinition.Fields = ParseFieldDefinitions(tokens, ref position);
                }

                return interfaceDefinition;
            }
            else
            {
                throw new Exception("Expected interface name after 'interface'");
            }
        }

        /// <summary>
        /// Parse a union definition
        /// </summary>
        private static GraphQLUnionDefinition ParseUnionDefinition(List<Token> tokens, ref int position)
        {
            // Skip 'union' keyword
            position++;

            // Get union name
            if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                string unionName = tokens[position].Value;
                position++; // Skip union name

                var unionDefinition = new GraphQLUnionDefinition
                {
                    Name = unionName
                };

                // Expect equals sign
                if (position < tokens.Count && tokens[position].Value == "=")
                {
                    position++; // Skip '='

                    // Parse possible types
                    while (position < tokens.Count && 
                          tokens[position].Type == TokenType.Identifier)
                    {
                        unionDefinition.PossibleTypes.Add(tokens[position].Value);
                        position++; // Skip type name

                        // Skip '|' if present
                        if (position < tokens.Count && tokens[position].Value == "|")
                        {
                            position++;
                        }
                    }
                }

                return unionDefinition;
            }
            else
            {
                throw new Exception("Expected union name after 'union'");
            }
        }

        /// <summary>
        /// Parse an enum definition
        /// </summary>
        private static GraphQLEnumDefinition ParseEnumDefinition(List<Token> tokens, ref int position)
        {
            // Skip 'enum' keyword
            position++;

            // Get enum name
            if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                string enumName = tokens[position].Value;
                position++; // Skip enum name

                var enumDefinition = new GraphQLEnumDefinition
                {
                    Name = enumName
                };

                // Expect opening brace
                if (position < tokens.Count && tokens[position].Value == "{")
                {
                    position++; // Skip '{'

                    // Parse enum values
                    while (position < tokens.Count && tokens[position].Value != "}")
                    {
                        if (tokens[position].Type == TokenType.Identifier)
                        {
                            var enumValue = new GraphQLEnumValueDefinition
                            {
                                Name = tokens[position].Value
                            };
                            position++; // Skip enum value name

                            // Parse directives
                            while (position < tokens.Count && tokens[position].Value == "@")
                            {
                                if (position + 1 < tokens.Count && tokens[position + 1].Value == "deprecated")
                                {
                                    ParseDeprecatedDirective(tokens, ref position, enumValue);
                                }
                                else
                                {
                                    // Skip other directives
                                    position++; // Skip '@'
                                    if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
                                    {
                                        position++; // Skip directive name
                                        
                                        // Skip arguments if present
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
                                    }
                                }
                            }

                            enumDefinition.Values.Add(enumValue);
                        }
                        else
                        {
                            // Skip unexpected token
                            position++;
                        }
                    }

                    // Skip closing brace
                    if (position < tokens.Count && tokens[position].Value == "}")
                    {
                        position++;
                    }
                }

                return enumDefinition;
            }
            else
            {
                throw new Exception("Expected enum name after 'enum'");
            }
        }

        /// <summary>
        /// Parse an input definition
        /// </summary>
        private static GraphQLInputDefinition ParseInputDefinition(List<Token> tokens, ref int position)
        {
            // Skip 'input' keyword
            position++;

            // Get input name
            if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                string inputName = tokens[position].Value;
                position++; // Skip input name

                var inputDefinition = new GraphQLInputDefinition
                {
                    Name = inputName
                };

                // Expect opening brace
                if (position < tokens.Count && tokens[position].Value == "{")
                {
                    position++; // Skip '{'

                    // Parse input fields
                    while (position < tokens.Count && tokens[position].Value != "}")
                    {
                        if (tokens[position].Type == TokenType.Identifier)
                        {
                            var inputField = ParseInputValueDefinition(tokens, ref position);
                            inputDefinition.InputFields[inputField.Name] = inputField;
                        }
                        else
                        {
                            // Skip unexpected token
                            position++;
                        }
                    }

                    // Skip closing brace
                    if (position < tokens.Count && tokens[position].Value == "}")
                    {
                        position++;
                    }
                }

                return inputDefinition;
            }
            else
            {
                throw new Exception("Expected input name after 'input'");
            }
        }

        /// <summary>
        /// Parse a scalar definition
        /// </summary>
        private static GraphQLScalarDefinition ParseScalarDefinition(List<Token> tokens, ref int position)
        {
            // Skip 'scalar' keyword
            position++;

            // Get scalar name
            if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                string scalarName = tokens[position].Value;
                position++; // Skip scalar name

                return new GraphQLScalarDefinition
                {
                    Name = scalarName
                };
            }
            else
            {
                throw new Exception("Expected scalar name after 'scalar'");
            }
        }

        /// <summary>
        /// Parse field definitions
        /// </summary>
        private static Dictionary<string, GraphQLFieldDefinition> ParseFieldDefinitions(List<Token> tokens, ref int position)
        {
            var fields = new Dictionary<string, GraphQLFieldDefinition>();

            // Parse fields until closing brace
            while (position < tokens.Count && tokens[position].Value != "}")
            {
                if (tokens[position].Type == TokenType.Identifier)
                {
                    var field = ParseFieldDefinition(tokens, ref position);
                    fields[field.Name] = field;
                }
                else
                {
                    // Skip unexpected token
                    position++;
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
        /// Parse a field definition
        /// </summary>
        private static GraphQLFieldDefinition ParseFieldDefinition(List<Token> tokens, ref int position)
        {
            // Get field name
            string fieldName = tokens[position].Value;
            position++; // Skip field name

            var field = new GraphQLFieldDefinition
            {
                Name = fieldName
            };

            // Check for arguments
            if (position < tokens.Count && tokens[position].Value == "(")
            {
                position++; // Skip '('

                // Parse arguments
                while (position < tokens.Count && tokens[position].Value != ")")
                {
                    if (tokens[position].Type == TokenType.Identifier)
                    {
                        var argument = ParseInputValueDefinition(tokens, ref position);
                        field.Arguments[argument.Name] = argument;
                    }
                    else
                    {
                        // Skip unexpected token
                        position++;
                    }
                }

                // Skip closing parenthesis
                if (position < tokens.Count && tokens[position].Value == ")")
                {
                    position++;
                }
            }

            // Expect colon
            if (position < tokens.Count && tokens[position].Value == ":")
            {
                position++; // Skip ':'

                // Parse type
                field.Type = ParseType(tokens, ref position);
            }

            // Parse directives
            while (position < tokens.Count && tokens[position].Value == "@")
            {
                if (position + 1 < tokens.Count && tokens[position + 1].Value == "deprecated")
                {
                    ParseDeprecatedDirective(tokens, ref position, field);
                }
                else
                {
                    // Skip other directives
                    position++; // Skip '@'
                    if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
                    {
                        position++; // Skip directive name
                        
                        // Skip arguments if present
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
                    }
                }
            }

            return field;
        }

        /// <summary>
        /// Parse an input value definition
        /// </summary>
        private static GraphQLInputValueDefinition ParseInputValueDefinition(List<Token> tokens, ref int position)
        {
            // Get input value name
            string inputValueName = tokens[position].Value;
            position++; // Skip input value name

            var inputValue = new GraphQLInputValueDefinition
            {
                Name = inputValueName
            };

            // Expect colon
            if (position < tokens.Count && tokens[position].Value == ":")
            {
                position++; // Skip ':'

                // Parse type
                inputValue.Type = ParseType(tokens, ref position);
            }

            // Check for default value
            if (position < tokens.Count && tokens[position].Value == "=")
            {
                position++; // Skip '='

                // Parse default value
                if (position < tokens.Count)
                {
                    inputValue.DefaultValue = tokens[position].Value;
                    position++; // Skip default value
                }
            }

            return inputValue;
        }

        /// <summary>
        /// Parse a type
        /// </summary>
        private static GraphQLType ParseType(List<Token> tokens, ref int position)
        {
            // Handle list type
            if (position < tokens.Count && tokens[position].Value == "[")
            {
                position++; // Skip '['

                var type = new GraphQLType
                {
                    IsList = true,
                    IsNullable = true // Default to nullable
                };

                // Parse inner type
                type.OfType = ParseType(tokens, ref position);

                // Expect closing bracket
                if (position < tokens.Count && tokens[position].Value == "]")
                {
                    position++; // Skip ']'
                }

                // Check for non-null
                if (position < tokens.Count && tokens[position].Value == "!")
                {
                    type.IsNullable = false;
                    position++; // Skip '!'
                }

                return type;
            }
            // Handle named type
            else if (position < tokens.Count && tokens[position].Type == TokenType.Identifier)
            {
                var type = new GraphQLType
                {
                    Name = tokens[position].Value,
                    IsNullable = true // Default to nullable
                };
                position++; // Skip type name

                // Check for non-null
                if (position < tokens.Count && tokens[position].Value == "!")
                {
                    type.IsNullable = false;
                    position++; // Skip '!'
                }

                return type;
            }
            else
            {
                throw new Exception("Expected type");
            }
        }

        /// <summary>
        /// Parse a deprecated directive
        /// </summary>
        private static void ParseDeprecatedDirective<T>(List<Token> tokens, ref int position, T target)
            where T : class
        {
            // Skip '@'
            position++;

            if (position < tokens.Count && tokens[position].Value == "deprecated")
            {
                position++; // Skip 'deprecated'

                // Set deprecated flag
                if (target is GraphQLFieldDefinition field)
                {
                    field.IsDeprecated = true;
                }
                else if (target is GraphQLEnumValueDefinition enumValue)
                {
                    enumValue.IsDeprecated = true;
                }

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
                                string reason = quotedReason.Substring(1, quotedReason.Length - 2);

                                // Set reason
                                if (target is GraphQLFieldDefinition fieldWithReason)
                                {
                                    fieldWithReason.DeprecationReason = reason;
                                }
                                else if (target is GraphQLEnumValueDefinition enumValueWithReason)
                                {
                                    enumValueWithReason.DeprecationReason = reason;
                                }

                                position++; // Skip reason string
                            }
                        }
                    }

                    // Skip to closing parenthesis
                    while (position < tokens.Count && tokens[position].Value != ")")
                    {
                        position++;
                    }

                    if (position < tokens.Count && tokens[position].Value == ")")
                    {
                        position++; // Skip ')'
                    }
                }
            }
        }

        /// <summary>
        /// Skip to the next definition in the schema
        /// </summary>
        private static void SkipToNextDefinition(List<Token> tokens, ref int position)
        {
            // Skip to the next type, interface, union, enum, input, scalar, or schema definition
            while (position < tokens.Count &&
                  (tokens[position].Value != "type" &&
                   tokens[position].Value != "interface" &&
                   tokens[position].Value != "union" &&
                   tokens[position].Value != "enum" &&
                   tokens[position].Value != "input" &&
                   tokens[position].Value != "scalar" &&
                   tokens[position].Value != "schema"))
            {
                position++;
            }
        }
        
        /// <summary>
        /// Get the approximate line number for a token position
        /// </summary>
        private static int GetLineNumber(List<Token> tokens, int position)
        {
            // Count the number of newline tokens before the current position
            int lineNumber = 1;
            for (int i = 0; i < position && i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.Comment)
                {
                    // Comments often contain newlines
                    lineNumber += tokens[i].Value.Count(c => c == '\n');
                }
            }
            return lineNumber;
        }
    }
}
