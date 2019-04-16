using System;
using System.Collections.Generic;

namespace ParallelTasker
{
    /// <summary>
    /// A generic implementation of object pool.
    /// </summary>
    /// <typeparam name="T">Type of objects in pool. Constrained to classes</typeparam>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> m_stack = new Stack<T>();
        private readonly Action<T> m_onBorrow, m_onRelease, m_dispose;
        private readonly Func<T> m_create;

        public int CountAll
        {
            get;
            internal set;
        }

        public int CountInactive
        {
            get
            {
                return m_stack.Count;
            }
        }

        public int CountActive
        {
            get
            {
                return CountAll - CountInactive;
            }
        }

        public ObjectPool(Func<T> create, Action<T> onBorrow, Action<T> onRelease)
        {
            m_create = create ??
                throw new ArgumentNullException(nameof(create));
            m_onBorrow = onBorrow;
            m_onRelease = onRelease;
            m_dispose = Release;
        }

        public T Borrow()
        {
            T obj;
            if (m_stack.Count == 0)
            {
                obj = m_create();
                CountAll++;
            }
            else
            {
                obj = m_stack.Pop();
            }
            m_onBorrow?.Invoke(obj);
            return obj;
        }

        public void Release(T obj)
        {
            if (m_stack.Count > 0 && ReferenceEquals(m_stack.Peek(), obj))
            {
                PTThreadSafeLogger.LogError("Trying to destroy object that has already been released");
            }
            m_stack.Push(obj);
            m_onRelease?.Invoke(obj);
        }

        public Disposable<T> BorrowDisposable()
        {
            return Disposable<T>.Borrow(Borrow(), m_dispose);
        }
    }

    public class KeyedPool<K, T> where T : class
    {
        private readonly Dictionary<K, Stack<T>> m_keyedStack = new Dictionary<K, Stack<T>>();

        private readonly Func<K, T> m_create;
        private readonly Func<T, K> m_onRelease;
        private readonly Action<K, T> m_onBorrow;
        private readonly Action<T> m_dispose;

        private KeyedPool()
        { }

        public KeyedPool(Func<K, T> create, Action<K, T> onBorrow, Func<T, K> onRelease)
        {
            m_create = create;
            m_onRelease = onRelease;
            m_onBorrow = onBorrow;
            m_dispose = Release;
        }

        public T Borrow(K key)
        {
            T result;
            if (m_keyedStack.TryGetValue(key, out var stack) && stack.Count > 0)
            {
                result = (T)stack.Pop();
            }
            else
            {
                result = m_create(key);
            }
            m_onBorrow?.Invoke(key, result);
            return result;
        }

        public void Release(T value)
        {
            var key = m_onRelease(value);
            if (!m_keyedStack.TryGetValue(key, out var stack))
            {
                stack = new Stack<T>();
                m_keyedStack[key] = stack;
            }
            stack.Push(value);
        }

        public Disposable<T> BorrowDisposable(K key)
        {
            return Disposable<T>.Borrow(Borrow(key), m_dispose);
        }
    }

    public class KeyedPoolWithDefaultKey<K, T> : KeyedPool<K, T> where T : class
    {

        private readonly K m_defaultKey;
        public KeyedPoolWithDefaultKey(Func<K, T> create, Action<K, T> onBorrow, Func<T, K> onRelease, K defaultKey) : base(create, onBorrow, onRelease)
        {
            this.m_defaultKey = defaultKey;
        }

        public T Borrow()
        {
            return Borrow(m_defaultKey);
        }

        public Disposable<T> BorrowDisposable()
        {
            return BorrowDisposable(m_defaultKey);
        }
    }

    public static class ListPool<T>
    {
        public static ObjectPool<List<T>> Instance
        {
            get;
        } = new ObjectPool<List<T>>(() => new List<T>(), null, list => list.Clear());
    }

    public static class DictionaryPool<K, V>
    {
        public static KeyedPoolWithDefaultKey<IEqualityComparer<K>, Dictionary<K, V>> Instance
        {
            get;
        }

        static DictionaryPool()
        {
            Dictionary<K, V> Create(IEqualityComparer<K> comparer)
            {
                return new Dictionary<K, V>(comparer);
            }

            IEqualityComparer<K> Reset(Dictionary<K, V> dictionary)
            {
                dictionary.Clear();
                return dictionary.Comparer;
            }

            Instance = new KeyedPoolWithDefaultKey<IEqualityComparer<K>, Dictionary<K, V>>(Create, null, Reset, EqualityComparer<K>.Default);
        }
    }
}
