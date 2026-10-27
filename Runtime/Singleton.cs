using UnityEngine;

namespace Adeel.UnityToolbox
{
    /// <summary>
    /// Base class for a component that must exist exactly once in the scene.
    /// </summary>
    /// <typeparam name="T">The concrete component type.</typeparam>
    /// <remarks>
    /// Derived classes that override <c>Awake</c> must call <c>base.Awake()</c>.
    /// </remarks>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        /// <summary>The single live instance, or <c>null</c> when none exists.</summary>
        public static T Instance => _instance;

        /// <summary>Whether an instance currently exists.</summary>
        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning(
                    $"{typeof(T).Name}: a second instance was found on '{name}'. " +
                    "The duplicate component has been removed.", this);
                Destroy(this);
                return;
            }

            _instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(_instance, this))
            {
                _instance = null;
            }
        }
    }

    /// <summary>
    /// A <see cref="Singleton{T}"/> that survives scene loads.
    /// </summary>
    /// <typeparam name="T">The concrete component type.</typeparam>
    public abstract class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            // A different instance already owns the singleton: drop this whole object.
            if (Instance != null && !ReferenceEquals(Instance, this))
            {
                Destroy(gameObject);
                return;
            }

            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}
