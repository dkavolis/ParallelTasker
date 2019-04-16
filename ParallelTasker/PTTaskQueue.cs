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
using System.Text;

namespace ParallelTasker
{
    public class PTTaskQueue
    {
        private readonly PTGroupDict<Queue<PTThreadTask>> m_tasks = new PTGroupDict<Queue<PTThreadTask>>(pair => new Queue<PTThreadTask>());

        public PTTimePair? Priority
        {
            get;
            set;
        }

        private volatile int m_count;

        public int Count
        {
            get
            {
                return m_count;
            }
        }
        public bool Empty
        {
            get
            {
                return m_count == 0;
            }
        }

        public PTTaskQueue()
        {
            ResetCount();
#if DEBUG
            var sb = new StringBuilder($"{this} dequeue order:\n");
            var counter = 0;
            foreach (var group in m_tasks.Keys)
                sb.AppendLine($"  {(++counter).ToString()}. {group.ToString()}");
            PTLogger.Debug(sb.ToString());
#endif
        }

        public void Enqueue(PTThreadTask task)
        {
            Enqueue(task.EndTime, task);
        }

        internal void Enqueue(PTTimePair endTime, PTThreadTask task)
        {
            m_tasks[endTime].Enqueue(task);
            m_count++;
        }

        public PTThreadTask Dequeue()
        {
            try
            {
                // first try returning tasks from prioritized queue
                if (Priority != null)
                {
                    var priority = (PTTimePair)Priority;
                    if (m_tasks[priority].Count > 0)
                        return Dequeue(priority);
                    else
                        Priority = null;
                }

                foreach (var pair in m_tasks)
                {
                    if (pair.Value.Count > 0)
                        return Dequeue(pair.Key);
                }
            }
            catch (InvalidOperationException)
            {
                ResetCount();
                throw;
            }

            return null;
        }

        private PTThreadTask Dequeue(PTTimePair endTime)
        {
            m_count--;
            return m_tasks[endTime].Dequeue();
        }

        private void ResetCount()
        {
            m_count = 0;
            var count = 0;
            foreach (var pair in m_tasks)
            {
                count += pair.Value.Count;
            }
            m_count = count;
        }
    }
}
