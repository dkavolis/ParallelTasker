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

using System.Collections.Generic;
using System.Threading;

namespace ParallelTasker
{
    public class PTController : Singleton<PTController>
    {
        // setting up everything before instance gets a chance to run anything
        private static PTThreadPool m_threadPool = new PTThreadPool();
        private static PTTaskList m_tasks = new PTTaskList();
        private static PTGroupDictQueue<PTThreadTask> m_toFinalize = new PTGroupDictQueue<PTThreadTask>();
        private static PTGroupDict<int> m_queuedTasks = new PTGroupDict<int>();
        private static PTLoggers m_loggers = new PTLoggers();
        private static object m_lock = new object();
        private bool m_resetOnSceneChange = true;
        public static bool ResetOnSceneChange
        {
            get
            {
                return Instance.m_resetOnSceneChange;
            }
            set
            {
                Instance.m_resetOnSceneChange = value;
            }
        }

        internal PTThreadPool ThreadPool
        {
            get
            {
                return m_threadPool;
            }
        }
        internal PTTaskList Tasks
        {
            get
            {
                return m_tasks;
            }
        }

        internal PTLoggers Loggers
        {
            get
            {
                return m_loggers;
            }
        }

        public static void ResetCurrentTasks()
        {
            m_tasks.ClearTasks();
        }

        internal void UpdateStart()
        {
            PTLogger.DebugSynchronization("Update start");
            EndGroupTasks(PTGroup.UpdateFrame);
            StartGroupTasks(PTGroup.Update);
            StartGroupTasks(PTGroup.UpdateFrame);
        }

        internal void UpdateEnd()
        {
            EndGroupTasks(PTGroup.Update);
            PTLogger.DebugSynchronization("Update end");
        }

        internal void LateUpdateStart()
        {
            PTLogger.DebugSynchronization("Late Update start");
            EndGroupTasks(PTGroup.LateUpdateFrame);
            StartGroupTasks(PTGroup.LateUpdate);
            StartGroupTasks(PTGroup.LateUpdateFrame);
        }

        internal void LateUpdateEnd()
        {
            EndGroupTasks(PTGroup.LateUpdate);
            PTLogger.DebugSynchronization("Late Update end");
        }

        internal void FixedUpdateStart()
        {
            PTLogger.DebugSynchronization("Fixed Update start");
            EndGroupTasks(PTGroup.FixedUpdateFrame);
            StartGroupTasks(PTGroup.FixedUpdate);
            StartGroupTasks(PTGroup.FixedUpdateFrame);
        }

        internal void FixedUpdateEnd()
        {
            EndGroupTasks(PTGroup.FixedUpdate);
            PTLogger.DebugSynchronization("Fixed Update end");
        }

        internal static void EnqueueForFinalization(PTThreadTask finalizer)
        {
            lock(m_lock)
            {
                m_toFinalize[finalizer.group].Enqueue(finalizer);
                Monitor.Pulse(m_lock);
            }
        }

        private void StartGroupTasks(PTGroup group)
        {
            List<PTTask> tasks = m_tasks[group];

            foreach (var task in tasks)
            {
                var threadTask = PTThreadTask.Get(group, task);
                m_threadPool.Enqueue(threadTask.RunInitializer());
            }

            m_queuedTasks[group] = tasks.Count;
        }

        private void EndGroupTasks(PTGroup group)
        {
            m_threadPool.Prioritize(group);
            int toFinalize = m_queuedTasks[group];
            PTThreadTask finalizer;

            while (toFinalize > 0)
            {
                lock(m_lock)
                {
                    while (m_toFinalize[group].Count == 0)
                    {
                        Monitor.Wait(m_lock);
                    }

                    finalizer = m_toFinalize[group].Dequeue();
                }

                finalizer.RunFinalizer();

                toFinalize--;
            }

            m_loggers[group].Flush();
        }
    }
}
