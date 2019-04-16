/*
Copyright 2019, Daumantas Kavolis

   This file is part of ParallelTasker.

   ParallelTasker is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   ParallelTasker is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with ParallelTasker.  If not, see <http: //www.gnu.org/licenses/>.

 */

using System;

namespace ParallelTasker
{
    /// <summary>
    /// A container for ParallelTasker tasks
    /// </summary>
    public class PTTask
    {
        private static readonly ObjectPool<PTTask> s_pool = new ObjectPool<PTTask>(() => new PTTask(), null, OnRelease);
        public Func<object> initialize;
        public Func<object, object> main;
        public Action<object> finalize;
        public uint period;
        public PTTimePair EndTime
        {
            get;
            internal set;
        }
        private uint m_counter;

        public PTTask() : this(PTTimePair.DefaultUpdate, null, null, null)
        { }

        public PTTask(PTTimePair endTime, Func<object> initialize, Func<object, object> main, Action<object> finalize, uint period = 1)
        {
            this.EndTime = endTime;
            this.initialize = initialize;
            this.main = main;
            this.finalize = finalize;
            this.period = period;
            m_counter = period;
        }

        public bool ShouldExecuteNext()
        {
            return m_counter == period;
        }

        internal bool ShouldExecute()
        {
            if (m_counter == period)
            {
                m_counter = 1;
                return true;
            }
            m_counter++;
            return false;
        }

        protected static void OnRelease(PTTask task)
        {
            task.initialize = null;
            task.main = null;
            task.finalize = null;
            task.period = 0;
        }

        public static PTTask Borrow(PTTimePair endTime, Func<object> initialize, Func<object, object> main, Action<object> finalize, uint period = 1)
        {
            var task = s_pool.Borrow();
            task.initialize = initialize;
            task.main = main;
            task.finalize = finalize;
            task.period = period;
            task.EndTime = endTime;
            task.m_counter = 1;
            return task;
        }

        public static void Release(PTTask task)
        {
            task.Release();
        }

        public virtual void Release()
        {
            s_pool.Release(this);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// A container for enqueued ParallelTasker tasks
    /// </summary>
    public class PTThreadTask : PTTask
    {
        private static readonly ObjectPool<PTThreadTask> s_pool = new ObjectPool<PTThreadTask>(() => new PTThreadTask(), null, OnRelease);
        private object argument;

        public PTThreadTask() : base()
        { }

        private static void OnRelease(PTThreadTask task)
        {
            PTTask.OnRelease(task);
            task.argument = null;
        }

        public static PTThreadTask Borrow(PTTimePair endTime, PTTask task)
        {
            var ttask = s_pool.Borrow();
            ttask.initialize = task.initialize;
            ttask.main = task.main;
            ttask.finalize = task.finalize;
            ttask.EndTime = endTime;
            return ttask;
        }

        public override void Release()
        {
            s_pool.Release(this);
        }

        public static void Release(PTThreadTask task)
        {
            task.Release();
        }

        public PTThreadTask RunInitializer()
        {
            try
            {
                argument = initialize?.Invoke();
            }
            catch (Exception ex)
            {
                main = null;
                finalize = null;
                PTLogger.Exception(ex);
            }
            return this;
        }

        public PTThreadTask RunMainTask()
        {
            try
            {
                argument = main?.Invoke(argument);
            }
            catch (Exception ex)
            {
                finalize = null;
                PTThreadSafeLogger.LogException(ex);
            }
            return this;
        }

        public void RunFinalizer()
        {
            try
            {
                finalize?.Invoke(argument);
            }
            catch (Exception ex)
            {
                PTLogger.Exception(ex);
            }
            Release();
        }
    }
}
