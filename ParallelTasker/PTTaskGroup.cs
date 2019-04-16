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
using System.Threading;

namespace ParallelTasker
{
    public class PTTaskGroup : IDisposable
    {
        private class Subscriber
        {
            public volatile bool subscribed;
            private readonly Action m_handler;
            private readonly PTTimePair m_currentTime;
            private readonly IPTSynchronizer m_synchronizer;
            // can't use UnityEngine.Object.ToString() in threads
            private readonly string m_synchronizerName;

            public Subscriber(Action handler, PTTimePair currentTime, IPTSynchronizer synchronizer)
            {
                subscribed = false;
                m_handler = handler;
                m_currentTime = currentTime;
                m_synchronizer = synchronizer;
                m_synchronizerName = synchronizer.ToString();
            }

            public void Subscribe()
            {
                if (subscribed)return;
                subscribed = true;
                m_synchronizer.Subscribe(m_currentTime.UpdateEvent, m_handler);
                PTThreadSafeLogger.Debug($"Subscribing to {m_synchronizerName}.{PTUpdateEventMap.ToName(m_currentTime.UpdateEvent)}");
            }

            public void Unsubscribe()
            {
                if (!subscribed)return;
                subscribed = false;
                m_synchronizer.Unsubscribe(m_currentTime.UpdateEvent, m_handler);
                PTThreadSafeLogger.Debug($"Unsubscribing from {m_synchronizerName}.{PTUpdateEventMap.ToName(m_currentTime.UpdateEvent)}");
            }
        }

        public bool SubscriptionStatus
        {
            get
            {
                return m_subscriber.subscribed;
            }
        }

        private readonly object m_lock;
        private readonly Subscriber m_subscriber;
        private readonly List<PTTask> m_tasks;
        private readonly Queue<PTThreadTask> m_tasksToFinish;
        private readonly PTTimePair m_timePair;
        private int m_finalizeSubscribers = 0;
        private PTController m_controller;

        public int TotalSubscribers
        {
            get
            {
                return m_tasks.Count + m_finalizeSubscribers;
            }
        }

        public PTTaskGroup(IPTSynchronizer synchronizer, PTTimePair timePair, PTController controller)
        {
            m_subscriber = new Subscriber(HandleTasks, timePair, synchronizer);
            m_timePair = timePair;
            m_lock = new object();
            m_tasks = new List<PTTask>();
            m_tasksToFinish = new Queue<PTThreadTask>();
            m_controller = controller;
        }

        public void EnqueueForFinalization(PTThreadTask task)
        {
            lock(m_lock)
            {
                m_tasksToFinish.Enqueue(task);
                Monitor.Pulse(m_lock);
            }
        }

        private void HandleTasks()
        {
            EndTasks();
            StartTasks();
        }

        private void StartTasks()
        {
            foreach (var task in m_tasks)
            {
                if (!task.ShouldExecute())continue;
                var threadTask = PTThreadTask.Borrow(task.EndTime, task);
                m_controller.Enqueue(threadTask.RunInitializer());
            }
        }

        private void EndTasks()
        {
            var instance = m_controller;
            var toFinalize = instance.PopFinalizationCounter(m_timePair);
            if (toFinalize == 0)
            {
                return;
            }
            instance.SetPriority(m_timePair);

            while (toFinalize > 0)
            {
                PTThreadTask finalizer;
                lock(m_lock)
                {
                    while (m_tasksToFinish.Count == 0)
                    {
                        Monitor.Wait(m_lock);
                    }

                    finalizer = m_tasksToFinish.Dequeue();
                }

                finalizer.RunFinalizer();
                toFinalize--;
            }
        }

        public PTTask this[int index]
        {
            get
            {
                return m_tasks[index];
            }
        }

        public PTTask AddTask(PTTimePair timePair, PTTask task)
        {
            task.EndTime = timePair;
            m_tasks.Add(task);
            m_subscriber.Subscribe();
            m_controller.SubscribeFinalizer(timePair);
            return task;
        }

        public PTTask AddTask(PTTimePair endTime, Func<object, object> main, uint period = 1)
        {
            return AddTask(endTime, PTTask.Borrow(endTime, null, main, null, period));
        }

        public PTTask AddTask(PTTimePair endTime, Func<object> initialize, Func<object, object> main, uint period = 1)
        {
            return AddTask(endTime, PTTask.Borrow(endTime, initialize, main, null, period));
        }

        public PTTask AddTask(PTTimePair endTime, Func<object> initialize, Func<object, object> main, Action<object> finalize, uint period = 1)
        {
            return AddTask(endTime, PTTask.Borrow(endTime, initialize, main, finalize, period));
        }

        public void ClearTasks()
        {
            foreach (var task in m_tasks)
            {
                task.Release();
                m_controller.UnsubscribeFinalizer(task.EndTime);
            }
            m_tasks.Clear();
            Unsubscribe();
        }

        public bool RemoveTask(PTTask task)
        {
            var index = m_tasks.IndexOf(task);
            if (index < 0)
                return false;
            RemoveAt(index, false);
            return true;
        }

        public bool RemoveTask(Func<object, object> task)
        {
            for (var i = 0; i < m_tasks.Count; i++)
            {
                if (m_tasks[i] == null)continue;
                if (m_tasks[i].main != task)continue;
                RemoveAt(i, true);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index, bool release = true)
        {
            if (release)m_tasks[index].Release();
            m_controller.UnsubscribeFinalizer(m_tasks[index].EndTime);
            m_tasks.RemoveAt(index);
            Unsubscribe();
        }

        public void SubscribeFinalizer()
        {
            Interlocked.Increment(ref m_finalizeSubscribers);
            m_subscriber.Subscribe();
        }

        public void UnsubscribeFinalizer()
        {
            Interlocked.Decrement(ref m_finalizeSubscribers);
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (TotalSubscribers > 0 || m_finalizeSubscribers > 0)return;
            m_finalizeSubscribers = 0;
            m_subscriber.Unsubscribe();
        }

        public void Dispose()
        {
            ClearTasks();
            m_controller = null;
        }
    }
}
