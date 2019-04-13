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
using System.Collections.Generic;

namespace ParallelTasker
{
    public class PTTaskList
    {
        private PTGroupDictList<PTTask> m_tasks = new PTGroupDictList<PTTask>();

        public List<PTTask> this[PTGroup group]
        {
            get
            {
                return m_tasks[group];
            }
        }
        public PTTaskList()
        { }

        public PTTask AddTask(PTGroup group, PTTask task)
        {
            m_tasks[group].Add(task);
            return task;
        }

        public PTTask AddTask(PTGroup group, Func<object, object> main)
        {
            return AddTask(group, PTTask.Get(null, main, null));
        }

        public PTTask AddTask(PTGroup group, Func<object> initialize, Func<object, object> main)
        {
            return AddTask(group, PTTask.Get(initialize, main, null));
        }

        public PTTask AddTask(PTGroup group, Func<object> initialize, Func<object, object> main, Action<object> finalize)
        {
            return AddTask(group, PTTask.Get(initialize, main, finalize));
        }

        public void ClearTasks()
        {
            foreach (var group in m_tasks.Keys)
            {
                ClearTasks(group);
            }
        }

        public void ClearTasks(PTGroup group)
        {
            var tasks = m_tasks[group];
            foreach (var task in tasks)
                task.Release();
            tasks.Clear();
        }
    }

    /// <summary>
    /// A container for ParallelTasker tasks
    /// </summary>
    public class PTTask
    {
        private static ObjectPool<PTTask> s_pool = new ObjectPool<PTTask>(null, OnRelease);
        public Func<object> initialize;
        public Func<object, object> main;
        public Action<object> finalize;

        public PTTask() : this(null, null, null)
        { }
        public PTTask(Func<object> initialize, Func<object, object> main, Action<object> finalize)
        {
            this.initialize = initialize;
            this.main = main;
            this.finalize = finalize;
        }

        protected static void OnRelease(PTTask task)
        {
            task.initialize = null;
            task.main = null;
            task.finalize = null;
        }

        public static PTTask Get(Func<object> initialize, Func<object, object> main, Action<object> finalize)
        {
            var task = s_pool.Get();
            task.initialize = initialize;
            task.main = main;
            task.finalize = finalize;
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

    /// <summary>
    /// A container for enqueued ParallelTasker tasks
    /// </summary>
    public class PTThreadTask : PTTask
    {
        public static ObjectPool<PTThreadTask> s_pool = new ObjectPool<PTThreadTask>(null, OnRelease);
        public PTGroup group;
        private object argument = null;

        public PTThreadTask() : this(null, null, null)
        { }

        public PTThreadTask(Func<object> initialize, Func<object, object> main, Action<object> finalize) : this(PTGroup.Update, initialize, main, finalize)
        { }

        public PTThreadTask(PTGroup group, Func<object> initialize, Func<object, object> main, Action<object> finalize) : base(initialize, main, finalize)
        {
            this.group = group;
        }

        private static void OnRelease(PTThreadTask task)
        {
            PTTask.OnRelease(task);
            task.argument = null;
        }

        public static PTThreadTask Get(PTGroup group)
        {
            var task = s_pool.Get();
            task.group = group;
            return task;
        }

        public static PTThreadTask Get(PTGroup group, PTTask task)
        {
            var ttask = s_pool.Get();
            ttask.initialize = task.initialize;
            ttask.main = task.main;
            ttask.finalize = task.finalize;
            ttask.group = group;
            return ttask;
        }

        public override void Release()
        {
            s_pool.Release(this);
        }

        public PTThreadTask RunInitializer()
        {
            argument = initialize?.Invoke();
            return this;
        }

        public PTThreadTask RunMainTask()
        {
            argument = main?.Invoke(argument);
            return this;
        }

        public void RunFinalizer()
        {
            finalize?.Invoke(argument);
            Release();
        }
    }
}
