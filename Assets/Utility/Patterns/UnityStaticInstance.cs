using UnityEngine;

namespace Griffty.Utility.Patterns
{
    public abstract class UnityStaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }

                return _instance;
            }
            private set => _instance = value;
        }

        public virtual void Awake()
        {
            Instance = this as T;
        }

        protected virtual void OnApplicationQuit()
        {
            Instance = null;
        }
    }
}