using UnityEngine;

namespace ParallelTasker
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool m_shuttingDown;
        private static bool m_destroyingDuplicate;
        private static readonly object m_lock = new object();
        private static volatile T m_instance;

        public static T Instance
        {
            get
            {
                if (m_shuttingDown)
                {
                    PTLogger.Warning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                    return null;
                }

                lock(m_lock)
                {
                    if (m_instance != null)return m_instance;
                    // Search for existing instance.
                    m_instance = (T)FindObjectOfType(typeof(T));

                    // Create new instance if one doesn't already exist.
                    if (m_instance != null)return m_instance;
                    // Need to create a new GameObject to attach the singleton to.
                    var singletonObject = new GameObject
                    {
                        name = typeof(T) + " (Singleton)"
                    };
                    singletonObject.AddComponent<T>();

                    return m_instance;
                }
            }
        }

        private void Awake()
        {
            if (m_instance == null)
            {
                PTLogger.Debug($"Singleton {this} is awake");
                m_instance = this as T;
                DontDestroyOnLoad(this);
                OnAwake();
            }
            else
            {
                PTLogger.Warning($"{this} is a Singleton but an instance already exists, destroying this instance");
                m_destroyingDuplicate = true;
                Destroy(this);
            }
        }

        protected virtual void OnAwake()
        {

        }

        private void OnApplicationQuit()
        {
            m_shuttingDown = true;
        }

        private void OnDestroy()
        {
            if (!m_destroyingDuplicate)
            {
                OnSingletonDestroy();
            }
            m_destroyingDuplicate = false;
        }

        protected virtual void OnSingletonDestroy()
        {
            m_shuttingDown = true;
            m_instance = null;
        }
    }
}
