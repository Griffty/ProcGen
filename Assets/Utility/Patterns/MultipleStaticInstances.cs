using System;
using System.Collections.Generic;
using UnityEngine;

namespace Griffty.Utility.Patterns
{
    /**
     * <summary> Provides a way to create multiple static instances of a class. Does not recommended for often use. </summary>
     */
    public static class MultipleStaticInstances<T> where T : class, new()
    {
        private static readonly Dictionary<string, T> Instances = new();
        
        public static T GetInstance(string name)
        {
            if (Instances.ContainsKey(name))
            {
                return Instances[name];
            }
            Debug.Log("New instance created");
            T instance = new T();
            Instances.Add(name, instance);
            return instance;
        }

        public static void RemoveAllInstances()
        {
            Instances.Clear();
        }
    }
}