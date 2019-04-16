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
using System.Threading;

namespace ParallelTasker
{
    public class PTThreadPool : IDisposable
    {

        private readonly Thread[] m_threads;
        private readonly object m_lock = new object();
        private readonly PTTaskQueue m_queue = new PTTaskQueue();

        public PTThreadPool() : this(Environment.ProcessorCount)
        { }

        public PTThreadPool(int number)
        {
            PTLogger.Info($"Initializing threadpool with {number.ToString()} threads.");
            m_threads = new Thread[number];
            for (var i = 0; i < number; i++)
            {
                m_threads[i] = new Thread(ExecuteTasks);
                m_threads[i].Start();
            }
        }

        public void Prioritize(PTTimePair eventTime)
        {
            m_queue.Priority = eventTime;
        }

        public void Enqueue(PTThreadTask task)
        {
            Enqueue(task.EndTime, task);
        }

        public void Enqueue(PTTimePair endTime, PTThreadTask task)
        {
            lock(m_lock)
            {
                m_queue.Enqueue(endTime, task);
                Monitor.Pulse(m_lock);
            }
        }

        private void ExecuteTasks()
        {
            while (true)
            {
                PTThreadTask task;
                lock(m_lock)
                {
                    // wait for queue to fill
                    while (m_queue.Empty)
                        Monitor.Wait(m_lock);

                    task = m_queue.Dequeue();
                }

                if (task == null)
                    return;
                PTAddon.Instance.Controller.EnqueueForFinalization(task.RunMainTask());
            }
        }

        public void Stop()
        {
            for (var i = 0; i < m_threads.Length; i++)
            {
                Enqueue(PTTimePair.DefaultFixedUpdate, null);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
