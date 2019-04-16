using System;

namespace ParallelTasker
{
    public class Disposable<T> : IDisposable
    {
        private static readonly ObjectPool<Disposable<T>> s_pool = new ObjectPool<Disposable<T>>(() => new Disposable<T>(), null, wrapper =>
        {
            wrapper.m_dispose?.Invoke(wrapper.Value);
            wrapper.Value = default(T);
        });

        private Action<T> m_dispose;

        public T Value
        {
            get;
            private set;
        }

        private Disposable()
        { }

        public static Disposable<T> Borrow(T value, Action<T> dispose)
        {
            var disposable = s_pool.Borrow();
            disposable.Value = value;
            disposable.m_dispose = dispose;
            return disposable;
        }

        public void Dispose()
        {
            s_pool.Release(this);
        }

    }
}
