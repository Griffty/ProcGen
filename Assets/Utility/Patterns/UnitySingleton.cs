using UnityEngine;

namespace Griffty.Utility.Patterns
{
    public abstract class UnitySingleton<T> : UnityStaticInstance<T> where T : MonoBehaviour
    {
    }
}