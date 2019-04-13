using System;
using System.Collections.Generic;

namespace ParallelTasker
{
    public class ObjectPool<T> where T : new()
    {
        private Stack<T> m_stack = new Stack<T>();
        private Action<T> m_onGet, m_onRelease;

        public int countAll
        {
            get;
            internal set;
        }

        public int countInactive
        {
            get
            {
                return m_stack.Count;
            }
        }

        public int countActive
        {
            get
            {
                return countAll - countInactive;
            }
        }

        public ObjectPool(Action<T> onGet, Action<T> onRelease)
        {
            m_onGet = onGet;
            m_onRelease = onRelease;
        }

        public T Get()
        {
            T obj;
            if (m_stack.Count == 0)
            {
                obj = new T();
                countAll++;
            }
            else
            {
                obj = m_stack.Pop();
            }
            m_onGet?.Invoke(obj);
            return obj;
        }

        public void Release(T obj)
        {
            if (m_stack.Count > 0 && object.ReferenceEquals(m_stack.Peek(), obj))
            {
                PTLogger.Error("Trying to destroy object that has already been released");
            }
            m_onRelease?.Invoke(obj);
            m_stack.Push(obj);
        }
    }
}
