using System;
using System.Collections.Generic;
using Game.Scripts.Common.Database.Resources.Types;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Game.Scripts.Common.Database.Resources.Data
{
    [CreateAssetMenu(menuName = "Mahjong/GameResourceDatabase", fileName = "GameResourceDatabase")]
    public class ResourceData : ScriptableObject
    {
        [Serializable]
        public struct Category
        {
            public string _categoryName;
            public ResourceType _resourceType;
            public List<Object> _resources;
        }
        
        public IReadOnlyList<Category> Categories => _categories.AsReadOnly();
        public Dictionary<ResourceType, Type> Types => _typeMap;

        [SerializeField] private List<Category> _categories;
        
        private readonly Dictionary<ResourceType, Type> _typeMap = new()
        {
            { ResourceType.Texture2D, typeof(Texture2D) },
            { ResourceType.Sprite, typeof(Sprite) },
            { ResourceType.AudioClip, typeof(AudioClip) },
            { ResourceType.GameObject, typeof(GameObject) }
        };

        private void OnValidate()
        {

            var nameSet = new HashSet<string>();
            foreach (var category in _categories)
            {
                if (string.IsNullOrEmpty(category._categoryName)) continue;

                nameSet.Clear();
                Type expectedType = _typeMap[category._resourceType];
                for (int i = 0; i < category._resources.Count; i++)
                {
                    var resource = category._resources[i];
                    if (resource == null) continue;

                    if (!expectedType.IsInstanceOfType(resource))
                    {
                        Debug.LogWarning($"Resource at index {i} in category '{category._categoryName}' is not of type {expectedType.Name}.");
                        category._resources[i] = null;
                        continue;
                    }

                    string resourceName = resource.name;
                    if (!nameSet.Add(resourceName))
                    {
                        Debug.LogWarning($"Duplicate resource name '{resourceName}' in category '{category._categoryName}' at index {i}.");
                    }
                }
            }
        }
    }
}