using System.Collections.Generic;

namespace GraphQLSourceGen.Models
{
    /// <summary>
    /// Represents a GraphQL schema
    /// </summary>
    public class GraphQLSchema
    {
        /// <summary>
        /// All types defined in the schema
        /// </summary>
        public Dictionary<string, GraphQLTypeDefinition> Types { get; set; } = new();

        /// <summary>
        /// All interfaces defined in the schema
        /// </summary>
        public Dictionary<string, GraphQLInterfaceDefinition> Interfaces { get; set; } = new();

        /// <summary>
        /// All unions defined in the schema
        /// </summary>
        public Dictionary<string, GraphQLUnionDefinition> Unions { get; set; } = new();

        /// <summary>
        /// All enums defined in the schema
        /// </summary>
        public Dictionary<string, GraphQLEnumDefinition> Enums { get; set; } = new();

        /// <summary>
        /// All input types defined in the schema
        /// </summary>
        public Dictionary<string, GraphQLInputDefinition> InputTypes { get; set; } = new();

        /// <summary>
        /// All scalar types defined in the schema
        /// </summary>
        public Dictionary<string, GraphQLScalarDefinition> ScalarTypes { get; set; } = new();

        /// <summary>
        /// The query type name
        /// </summary>
        public string? QueryTypeName { get; set; }

        /// <summary>
        /// The mutation type name
        /// </summary>
        public string? MutationTypeName { get; set; }

        /// <summary>
        /// The subscription type name
        /// </summary>
        public string? SubscriptionTypeName { get; set; }

        /// <summary>
        /// Get a field definition from a type by name
        /// </summary>
        public GraphQLFieldDefinition? GetFieldDefinition(string typeName, string fieldName)
        {
            // Check if the type exists
            if (Types.TryGetValue(typeName, out var typeDefinition))
            {
                // Check if the field exists on the type
                if (typeDefinition.Fields.TryGetValue(fieldName, out var fieldDefinition))
                {
                    return fieldDefinition;
                }

                // If the field is not found directly, check the interfaces that this type implements
                foreach (var interfaceName in typeDefinition.Interfaces)
                {
                    var interfaceFieldDef = GetFieldDefinition(interfaceName, fieldName);
                    if (interfaceFieldDef != null)
                    {
                        return interfaceFieldDef;
                    }
                }
            }

            // Check if it's an interface
            if (Interfaces.TryGetValue(typeName, out var interfaceDefinition))
            {
                // Check if the field exists on the interface
                if (interfaceDefinition.Fields.TryGetValue(fieldName, out var fieldDefinition))
                {
                    return fieldDefinition;
                }
            }

            // Check if it's a union
            if (Unions.TryGetValue(typeName, out var unionDefinition))
            {
                // For unions, we need to find a field that exists in all possible types
                // and has the same type in all of them
                GraphQLFieldDefinition? commonField = null;
                
                foreach (var possibleType in unionDefinition.PossibleTypes)
                {
                    var fieldDef = GetFieldDefinition(possibleType, fieldName);
                    
                    if (fieldDef == null)
                    {
                        // If any possible type doesn't have this field, it's not a common field
                        return null;
                    }
                    
                    if (commonField == null)
                    {
                        // First possible type with this field
                        commonField = fieldDef;
                    }
                    else if (!AreTypesCompatible(commonField.Type, fieldDef.Type))
                    {
                        // If the field types are not compatible, it's not a common field
                        return null;
                    }
                }
                
                return commonField;
            }

            return null;
        }
        
        /// <summary>
        /// Check if two GraphQL types are compatible
        /// </summary>
        private bool AreTypesCompatible(GraphQLType type1, GraphQLType type2)
        {
            // If both are lists, check if their element types are compatible
            if (type1.IsList && type2.IsList)
            {
                return type1.OfType != null && type2.OfType != null && 
                       AreTypesCompatible(type1.OfType, type2.OfType);
            }
            
            // If one is a list and the other is not, they are not compatible
            if (type1.IsList != type2.IsList)
            {
                return false;
            }
            
            // Check if the names match
            if (type1.Name != type2.Name)
            {
                // Check if one is an interface that the other implements
                if (Interfaces.ContainsKey(type1.Name) && 
                    Types.TryGetValue(type2.Name, out var type2Def) && 
                    type2Def.Interfaces.Contains(type1.Name))
                {
                    return true;
                }
                
                if (Interfaces.ContainsKey(type2.Name) && 
                    Types.TryGetValue(type1.Name, out var type1Def) && 
                    type1Def.Interfaces.Contains(type2.Name))
                {
                    return true;
                }
                
                return false;
            }
            
            // If nullability doesn't match, they might still be compatible in some cases
            // A non-nullable field can be used where a nullable field is expected
            if (type1.IsNullable != type2.IsNullable)
            {
                return type1.IsNullable; // type1 is nullable, type2 is not
            }
            
            return true;
        }

        /// <summary>
        /// Get a type definition by name
        /// </summary>
        public GraphQLTypeDefinition? GetTypeDefinition(string typeName)
        {
            if (Types.TryGetValue(typeName, out var typeDefinition))
            {
                return typeDefinition;
            }
            return null;
        }

        /// <summary>
        /// Check if a type implements an interface
        /// </summary>
        public bool TypeImplementsInterface(string typeName, string interfaceName)
        {
            if (Types.TryGetValue(typeName, out var typeDefinition))
            {
                return typeDefinition.Interfaces.Contains(interfaceName);
            }
            return false;
        }

        /// <summary>
        /// Get all types that implement an interface
        /// </summary>
        public List<string> GetTypesImplementingInterface(string interfaceName)
        {
            var implementingTypes = new List<string>();
            foreach (var type in Types)
            {
                if (type.Value.Interfaces.Contains(interfaceName))
                {
                    implementingTypes.Add(type.Key);
                }
            }
            return implementingTypes;
        }

        /// <summary>
        /// Get all possible types for a union
        /// </summary>
        public List<string> GetPossibleTypesForUnion(string unionName)
        {
            if (Unions.TryGetValue(unionName, out var unionDefinition))
            {
                return unionDefinition.PossibleTypes;
            }
            return new List<string>();
        }
    }

    /// <summary>
    /// Base class for all GraphQL type system definitions
    /// </summary>
    public abstract class GraphQLTypeSystemDefinition
    {
        /// <summary>
        /// The name of the type
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the type
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Represents a GraphQL object type definition
    /// </summary>
    public class GraphQLTypeDefinition : GraphQLTypeSystemDefinition
    {
        /// <summary>
        /// The interfaces this type implements
        /// </summary>
        public List<string> Interfaces { get; set; } = new();

        /// <summary>
        /// The fields defined on this type
        /// </summary>
        public Dictionary<string, GraphQLFieldDefinition> Fields { get; set; } = new();
    }

    /// <summary>
    /// Represents a GraphQL interface definition
    /// </summary>
    public class GraphQLInterfaceDefinition : GraphQLTypeSystemDefinition
    {
        /// <summary>
        /// The fields defined on this interface
        /// </summary>
        public Dictionary<string, GraphQLFieldDefinition> Fields { get; set; } = new();
    }

    /// <summary>
    /// Represents a GraphQL union definition
    /// </summary>
    public class GraphQLUnionDefinition : GraphQLTypeSystemDefinition
    {
        /// <summary>
        /// The possible types for this union
        /// </summary>
        public List<string> PossibleTypes { get; set; } = new();
    }

    /// <summary>
    /// Represents a GraphQL enum definition
    /// </summary>
    public class GraphQLEnumDefinition : GraphQLTypeSystemDefinition
    {
        /// <summary>
        /// The values defined for this enum
        /// </summary>
        public List<GraphQLEnumValueDefinition> Values { get; set; } = new();
    }

    /// <summary>
    /// Represents a GraphQL enum value definition
    /// </summary>
    public class GraphQLEnumValueDefinition
    {
        /// <summary>
        /// The name of the enum value
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the enum value
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether the enum value is deprecated
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// The reason the enum value is deprecated
        /// </summary>
        public string? DeprecationReason { get; set; }
    }

    /// <summary>
    /// Represents a GraphQL input type definition
    /// </summary>
    public class GraphQLInputDefinition : GraphQLTypeSystemDefinition
    {
        /// <summary>
        /// The input fields defined on this input type
        /// </summary>
        public Dictionary<string, GraphQLInputValueDefinition> InputFields { get; set; } = new();
    }

    /// <summary>
    /// Represents a GraphQL scalar type definition
    /// </summary>
    public class GraphQLScalarDefinition : GraphQLTypeSystemDefinition
    {
        // Scalar types don't have additional properties
    }

    /// <summary>
    /// Represents a GraphQL field definition
    /// </summary>
    public class GraphQLFieldDefinition
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the field
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The type of the field
        /// </summary>
        public GraphQLType Type { get; set; } = new();

        /// <summary>
        /// The arguments defined on this field
        /// </summary>
        public Dictionary<string, GraphQLInputValueDefinition> Arguments { get; set; } = new();

        /// <summary>
        /// Whether the field is deprecated
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// The reason the field is deprecated
        /// </summary>
        public string? DeprecationReason { get; set; }
    }

    /// <summary>
    /// Represents a GraphQL input value definition (used for arguments and input fields)
    /// </summary>
    public class GraphQLInputValueDefinition
    {
        /// <summary>
        /// The name of the input value
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the input value
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The type of the input value
        /// </summary>
        public GraphQLType Type { get; set; } = new();

        /// <summary>
        /// The default value of the input value
        /// </summary>
        public string? DefaultValue { get; set; }
    }
}