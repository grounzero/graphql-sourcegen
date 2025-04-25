using System;
using System.Collections.Generic;
using GraphQLSourceGen.Configuration;

namespace GraphQLSourceGen.Models
{
    /// <summary>
    /// Service responsible for generating consistent model names across the GraphQL source generator.
    /// </summary>
    public class ModelNamingService
    {
        private readonly HashSet<string> _generatedNames = new HashSet<string>();
        private readonly Dictionary<string, string> _customModelNameMappings;
        
        /// <summary>
        /// Initializes a new instance of the ModelNamingService class.
        /// </summary>
        /// <param name="options">Optional configuration options.</param>
        public ModelNamingService(GraphQLSourceGenOptions? options = null)
        {
            _customModelNameMappings = options?.CustomModelNameMappings ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets a properly formatted model name for a field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="parentTypeName">Optional parent type name for context.</param>
        /// <returns>A properly formatted model name.</returns>
        public string GetModelName(string fieldName, string? parentTypeName = null)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));
            }

            // Check if there's a custom mapping for this field name
            if (_customModelNameMappings.TryGetValue(fieldName, out var customName))
            {
                // Ensure the custom name ends with "Model"
                string customModelName = customName.EndsWith("Model") ? customName : customName + "Model";
                
                // Add parent type name for uniqueness if provided
                if (!string.IsNullOrEmpty(parentTypeName))
                {
                    customModelName = $"{parentTypeName}_{customModelName}";
                }
                
                // Register the name
                _generatedNames.Add(customModelName);
                
                return customModelName;
            }

            // Ensure PascalCase
            string modelName = char.ToUpper(fieldName[0]) + fieldName.Substring(1) + "Model";
            
            // Add parent type name for uniqueness if provided
            if (!string.IsNullOrEmpty(parentTypeName))
            {
                modelName = $"{parentTypeName}_{modelName}";
            }
            
            // Register the name
            _generatedNames.Add(modelName);
            
            return modelName;
        }

        /// <summary>
        /// Checks if a model name has already been generated.
        /// </summary>
        /// <param name="modelName">The model name to check.</param>
        /// <returns>True if the model name has been generated, false otherwise.</returns>
        public bool IsModelGenerated(string modelName)
        {
            return _generatedNames.Contains(modelName);
        }

        /// <summary>
        /// Gets all generated model names.
        /// </summary>
        /// <returns>A collection of all generated model names.</returns>
        public IEnumerable<string> GetAllGeneratedNames()
        {
            return _generatedNames;
        }

        /// <summary>
        /// Formats a property type name with proper casing and nullability.
        /// </summary>
        /// <param name="typeName">The base type name.</param>
        /// <param name="isList">Whether the type is a list.</param>
        /// <param name="isNullable">Whether the type is nullable.</param>
        /// <returns>A properly formatted property type.</returns>
        public string FormatPropertyTypeName(string typeName, bool isList, bool isNullable)
        {
            // Check if there's a custom mapping for this type name
            string formattedTypeName;
            if (_customModelNameMappings.TryGetValue(typeName, out var customName))
            {
                // Use the custom name but ensure it doesn't already have "Model" suffix
                formattedTypeName = customName.EndsWith("Model")
                    ? customName.Substring(0, customName.Length - 5)
                    : customName;
            }
            else
            {
                // Ensure PascalCase for the type name
                formattedTypeName = char.ToUpper(typeName[0]) + typeName.Substring(1);
            }
            
            string propertyType;
            
            if (isList)
            {
                propertyType = $"List<{formattedTypeName}Model>";
            }
            else
            {
                propertyType = $"{formattedTypeName}Model";
            }
            
            // Add nullability marker if needed
            if (isNullable && !propertyType.EndsWith("?"))
            {
                propertyType += "?";
            }
            
            return propertyType;
        }
    }
}