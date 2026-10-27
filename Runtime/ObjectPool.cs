using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Adeel.UnityToolbox
{
    /// <summary>
    /// A minimal, allocation-free <see cref="GameObject"/> pool.
    /// Pre-instantiates <see cref="initialSize"/> disabled clones during
    /// <c>Awake</c> and hands them out on demand.
    /// </summary>
    /// <remarks>
    /// Call <see cref="GetPooledObject"/> to borrow an instance and either
    /// <see cref="Return"/> or <c>SetActive(false)</c> to give it back.
    /// </remarks>
    public class ObjectPool : MonoBehaviour
    {
        [FormerlySerializedAs("objectToPool")]
        [Tooltip("The prefab that is cloned to fill this pool. Required.")]
        [SerializeField] private GameObject prefab;

        [FormerlySerializedAs("amountToPool")]
        [Tooltip("How many disabled clones are created up front.")]
        [SerializeField] private int initialSize = 10;

        [Tooltip("When every instance is in use, create another one instead of returning null.")]
        [SerializeField] private bool growWhenExhausted = true;

        private readonly List<GameObject> _pool = new List<GameObject>();

        private void Awake()
        {
            if (prefab == null)
            {
                Debug.LogError($"{nameof(ObjectPool)}: no prefab assigned — the pool is disabled.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < initialSize; i++)
            {
                _pool.Add(CreateInstance());
            }
        }

        private GameObject CreateInstance()
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            return instance;
        }

        /// <summary>
        /// Returns an inactive instance from the pool, creating a new one when the
        /// pool is exhausted and <see cref="growWhenExhausted"/> is enabled.
        /// Returns <c>null</c> when nothing is available.
        /// </summary>
        public GameObject GetPooledObject()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                GameObject pooled = _pool[i];
                if (pooled != null && !pooled.activeSelf)
                {
                    return pooled;
                }
            }

            if (!growWhenExhausted)
            {
                return null;
            }

            GameObject created = CreateInstance();
            _pool.Add(created);
            return created;
        }

        /// <summary>Deactivates an instance so it can be reused.</summary>
        public void Return(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.SetActive(false);
        }

        /// <summary>Deactivates every instance in the pool.</summary>
        public void ReturnAll()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null)
                {
                    _pool[i].SetActive(false);
                }
            }
        }
    }
}
