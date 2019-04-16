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
    public class PTController : IDisposable
    {
        // setting up everything before instance gets a chance to run anything
        private readonly PTThreadPool m_threadPool;

        // track counters in here so since all TaskGroups queue new tasks through this
        private readonly PTGroupDict<int> m_counters;

        internal PTGroupDict<PTTaskGroup> Tasks
        {
            get;
        }

        public PTController()
        {
            m_threadPool = new PTThreadPool();
            Tasks = new PTGroupDict<PTTaskGroup>(pair => new PTTaskGroup(PTAddon.Instance.Synchronizers[pair.EventTime], pair, this));
            m_counters = new PTGroupDict<int>();
        }

        public void ResetCurrentTasks()
        {
            foreach (var tasks in Tasks.Values)
            {
                tasks.ClearTasks();
            }
        }

        /// <summary>
        /// Only ever called from TaskGroup.EndTasks(), threadpool has the lock
        /// </summary>
        /// <param name="timePair"></param>
        public void SetPriority(PTTimePair timePair)
        {
            m_threadPool.Prioritize(timePair);
        }

        /// <summary>
        /// Only ever called from main thread by TaskGroup.StartTasks()
        /// </summary>
        /// <param name="task"></param>
        internal void Enqueue(PTThreadTask task)
        {
            m_threadPool.Enqueue(task);
            m_counters[task.EndTime]++;
        }

        /// <summary>
        /// Can be called from any thread, TaskGroup has the lock
        /// </summary>
        /// <param name="finalizer"></param>
        internal void EnqueueForFinalization(PTThreadTask finalizer)
        {
            Tasks[finalizer.EndTime].EnqueueForFinalization(finalizer);
        }

        /// <summary>
        /// Called by TaskGroup when finishing its tasks, always on main thread
        /// </summary>
        /// <param name="timePair"></param>
        /// <returns></returns>
        internal int PopFinalizationCounter(PTTimePair timePair)
        {
            var count = m_counters[timePair];
            m_counters[timePair] = 0;
            return count;
        }

        internal void SubscribeFinalizer(PTTimePair timePair)
        {
            Tasks[timePair].SubscribeFinalizer();
        }

        internal void UnsubscribeFinalizer(PTTimePair timePair)
        {
            Tasks[timePair].UnsubscribeFinalizer();
        }

        public void Dispose()
        {
            m_threadPool.Dispose();
            foreach (var group in Tasks.Values)
            {
                group.Dispose();
            }
        }
    }
}
