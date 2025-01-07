using System;

namespace Griffty.Utility.Patterns
{
    public static class Singleton<T> where T : class, new()
    {
        private static readonly Lazy<T> instance = new(() => new T());

        public static T Instance => instance.Value;
    }
}