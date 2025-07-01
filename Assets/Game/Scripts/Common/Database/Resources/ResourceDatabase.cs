using System;
using System.Collections.Generic;
using Game.Scripts.Common.Database.Resources.Data;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Game.Scripts.Common.Database.Resources
{
    public class ResourceDatabase : MonoBehaviour, IResourceDatabase
    {
        [SerializeField] private ResourceData _database;
        
        private bool _isInitialized;
        private Dictionary<(string category, string name), Object> _resourceCache;
        private Dictionary<string, (Type type, List<Object> resources)> _categoryCache;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized || _database == null) return;

            _resourceCache = new Dictionary<(string, string), Object>();
            _categoryCache = new Dictionary<string, (Type, List<Object>)>();

            foreach (var category in _database.Categories)
            {
                if (string.IsNullOrEmpty(category._categoryName)) continue;

                if (!_database.Types.TryGetValue(category._resourceType, out var expectedType))
                {
                    Debug.LogError($"Invalid resource type {category._resourceType} for category '{category._categoryName}'.");
                    continue;
                }

                var resources = new List<Object>(category._resources.Count);
                foreach (var resource in category._resources)
                {
                    if (resource != null && expectedType.IsInstanceOfType(resource))
                    {
                        _resourceCache[(category._categoryName, resource.name)] = resource;
                        resources.Add(resource);
                    }
                }

                if (resources.Count > 0)
                {
                    _categoryCache[category._categoryName] = (expectedType, resources);
                }
            }

            _isInitialized = true;
        }

        public T GetResource<T>(string resourceName, string categoryName = null) where T : Object
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (categoryName != null)
            {
                if (_resourceCache.TryGetValue((categoryName, resourceName), out var resource) && resource is T typedResource)
                {
                    if (_categoryCache.TryGetValue(categoryName, out var category) && category.type.IsAssignableFrom(typeof(T)))
                    {
                        return typedResource;
                    }
                    Debug.LogWarning($"Resource '{resourceName}' in category '{categoryName}' is not of expected type {typeof(T).Name}.");
                    return null;
                }
            }
            else
            {
                foreach (var cacheEntry in _resourceCache)
                {
                    if (cacheEntry.Key.name == resourceName && cacheEntry.Value is T typedResource)
                    {
                        if (_categoryCache.TryGetValue(cacheEntry.Key.category, out var category) && category.type.IsAssignableFrom(typeof(T)))
                        {
                            return typedResource;
                        }
                    }
                }
            }

            Debug.LogError($"Resource '{resourceName}' of type {typeof(T).Name} not found in {(categoryName != null ? $"category '{categoryName}'" : "database")}.");
            return null;
        }

        public IReadOnlyList<T> GetResourcesByCategory<T>(string categoryName) where T : Object
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_categoryCache.TryGetValue(categoryName, out var category))
            {
                if (!category.type.IsAssignableFrom(typeof(T)))
                {
                    Debug.LogError($"Category '{categoryName}' contains resources of type {category.type.Name}, but requested type is {typeof(T).Name}.");
                    return new List<T>().AsReadOnly();
                }

                var typedResources = new List<T>();
                foreach (var resource in category.resources)
                {
                    if (resource is T typedResource)
                    {
                        typedResources.Add(typedResource);
                    }
                }
                return typedResources.AsReadOnly();
            }

            Debug.LogError($"Category '{categoryName}' not found in database.");
            return new List<T>().AsReadOnly();
        }

        public IReadOnlyList<string> GetAllCategories()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            return new List<string>(_categoryCache.Keys);
        }
    }
}