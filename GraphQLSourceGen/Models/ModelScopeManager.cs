using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLSourceGen.Models
{
    /// <summary>
    /// Manages the scoping of model classes to ensure proper nesting and avoid duplication.
    /// </summary>
    public class ModelScopeManager
    {
        private readonly Dictionary<string, HashSet<string>> _scopedModels = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, string> _modelScopes = new Dictionary<string, string>();
        private readonly Dictionary<string, HashSet<string>> _nestedModels = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Registers a model in a specific scope.
        /// </summary>
        /// <param name="modelName">The name of the model to register.</param>
        /// <param name="scope">The scope in which to register the model.</param>
        /// <returns>True if the model was newly registered, false if it already existed in this scope.</returns>
        public bool RegisterModel(string modelName, string scope = "global")
        {
            if (string.IsNullOrEmpty(modelName))
            {
                throw new ArgumentException("Model name cannot be null or empty", nameof(modelName));
            }

            if (string.IsNullOrEmpty(scope))
            {
                scope = "global";
            }

            // Initialize the scope if it doesn't exist
            if (!_scopedModels.ContainsKey(scope))
            {
                _scopedModels[scope] = new HashSet<string>();
            }
            
            // Track which scope this model belongs to
            _modelScopes[modelName] = scope;
            
            // Register the model in the scope
            return _scopedModels[scope].Add(modelName);
        }
        
        /// <summary>
        /// Registers a nested model relationship.
        /// </summary>
        /// <param name="parentModelName">The name of the parent model.</param>
        /// <param name="childModelName">The name of the child model.</param>
        public void RegisterNestedModel(string parentModelName, string childModelName)
        {
            if (string.IsNullOrEmpty(parentModelName))
            {
                throw new ArgumentException("Parent model name cannot be null or empty", nameof(parentModelName));
            }
            
            if (string.IsNullOrEmpty(childModelName))
            {
                throw new ArgumentException("Child model name cannot be null or empty", nameof(childModelName));
            }
            
            // Initialize the parent's nested models collection if it doesn't exist
            if (!_nestedModels.ContainsKey(parentModelName))
            {
                _nestedModels[parentModelName] = new HashSet<string>();
            }
            
            // Register the child model as nested under the parent
            _nestedModels[parentModelName].Add(childModelName);
        }
        
        /// <summary>
        /// Checks if a model is registered in a specific scope.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <param name="scope">The scope to check in.</param>
        /// <returns>True if the model is registered in the specified scope, false otherwise.</returns>
        public bool IsModelInScope(string modelName, string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                scope = "global";
            }
            
            return _scopedModels.ContainsKey(scope) && _scopedModels[scope].Contains(modelName);
        }
        
        /// <summary>
        /// Checks if a model is registered in any scope.
        /// </summary>
        /// <param name="modelName">The name of the model to check.</param>
        /// <returns>True if the model is registered in any scope, false otherwise.</returns>
        public bool IsModelRegistered(string modelName)
        {
            return _modelScopes.ContainsKey(modelName);
        }
        
        /// <summary>
        /// Gets the scope in which a model is registered.
        /// </summary>
        /// <param name="modelName">The name of the model.</param>
        /// <returns>The scope in which the model is registered, or null if the model is not registered.</returns>
        public string? GetModelScope(string modelName)
        {
            return _modelScopes.TryGetValue(modelName, out var scope) ? scope : null;
        }
        
        /// <summary>
        /// Gets all models registered in a specific scope.
        /// </summary>
        /// <param name="scope">The scope to get models for.</param>
        /// <returns>A collection of model names registered in the specified scope.</returns>
        public IEnumerable<string> GetModelsInScope(string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                scope = "global";
            }
            
            return _scopedModels.ContainsKey(scope) ? _scopedModels[scope] : Enumerable.Empty<string>();
        }
        
        /// <summary>
        /// Gets all models nested under a specific parent model.
        /// </summary>
        /// <param name="parentModelName">The name of the parent model.</param>
        /// <returns>A collection of model names nested under the specified parent model.</returns>
        public IEnumerable<string> GetNestedModels(string parentModelName)
        {
            return _nestedModels.ContainsKey(parentModelName) ? _nestedModels[parentModelName] : Enumerable.Empty<string>();
        }
        
        /// <summary>
        /// Checks if a model is nested under a specific parent model.
        /// </summary>
        /// <param name="parentModelName">The name of the parent model.</param>
        /// <param name="childModelName">The name of the child model.</param>
        /// <returns>True if the child model is nested under the parent model, false otherwise.</returns>
        public bool IsNestedModel(string parentModelName, string childModelName)
        {
            return _nestedModels.ContainsKey(parentModelName) && _nestedModels[parentModelName].Contains(childModelName);
        }
        
        /// <summary>
        /// Gets all registered models across all scopes.
        /// </summary>
        /// <returns>A collection of all registered model names.</returns>
        public IEnumerable<string> GetAllModels()
        {
            return _modelScopes.Keys;
        }
        
        /// <summary>
        /// Gets all scopes that have registered models.
        /// </summary>
        /// <returns>A collection of all scopes with registered models.</returns>
        public IEnumerable<string> GetAllScopes()
        {
            return _scopedModels.Keys;
        }
        
        /// <summary>
        /// Clears all registered models and scopes.
        /// </summary>
        public void Clear()
        {
            _scopedModels.Clear();
            _modelScopes.Clear();
            _nestedModels.Clear();
        }
    }
}