using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Common.Database.Resources
{
    public interface IResourceDatabase
    {
        T GetResource<T>(string resourceName, string categoryName = null) where T : Object;
        IReadOnlyList<T> GetResourcesByCategory<T>(string categoryName) where T : Object;
        IReadOnlyList<string> GetAllCategories();
    }
}