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
    public class PTThreadPool
    {

        private Thread[] m_threads;
        private readonly object m_lock = new object();
        private PTTaskQueue m_queue = new PTTaskQueue();

        public PTThreadPool() : this(Environment.ProcessorCount)
        { }

        public PTThreadPool(int number)
        {
            PTLogger.Info($"Initializing threadpool with {number} threads.");
            m_threads = new Thread[number];
            for (int i = 0; i < number; i++)
            {
                m_threads[i] = new Thread(ExecuteTasks);
                m_threads[i].Start();
            }
        }

        public void Prioritize(PTGroup group)
        {
            lock(m_lock)
            {
                m_queue.Priority = group;
            }
        }

        public void Enqueue(PTThreadTask task)
        {
            Enqueue(task.group, task);
        }

        public void Enqueue(PTGroup group, PTThreadTask task)
        {
            lock(m_lock)
            {
                m_queue.Enqueue(group, task);
                Monitor.Pulse(m_lock);
            }
        }

        private void ExecuteTasks()
        {
            PTThreadTask task = null;
            while (true)
            {
                lock(m_lock)
                {
                    // wait for queue to fill
                    while (m_queue.Empty)
                        Monitor.Wait(m_lock);

                    task = m_queue.Dequeue();
                }

                if (task == null)
                    return;
                PTController.EnqueueForFinalization(task.RunMainTask());
            }
        }

        ~PTThreadPool()
        {
            for (int i = 0; i < m_threads.Length; i++)
            {
                Enqueue(PTGroup.Update, null);
            }
        }
    }
}
