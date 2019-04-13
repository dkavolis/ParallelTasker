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
        private PTGroupDictQueue<PTThreadTask> m_tasks = new PTGroupDictQueue<PTThreadTask>();

        private PTGroup? m_priority = null;

        public PTGroup? Priority
        {
            get
            {
                return m_priority;
            }
            set
            {
                m_priority = value;
            }
        }
        private volatile int m_count = 0;

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
            StringBuilder sb = new StringBuilder($"{this} dequeue order:\n");
            int counter = 0;
            foreach (var group in m_tasks.Keys)
                sb.AppendLine($"  {++counter}. {group}");
            PTLogger.Debug(sb.ToString());
#endif
        }

        public void Enqueue(PTThreadTask task)
        {
            Enqueue(task.group, task);
        }

        internal void Enqueue(PTGroup group, PTThreadTask task)
        {
            m_tasks[group].Enqueue(task);
            m_count++;
        }

        public PTThreadTask Dequeue()
        {
            // first try returning tasks from prioritized queue
            try
            {
                if (m_priority != null)
                {
                    var priority = (PTGroup)m_priority;
                    if (m_tasks[priority].Count > 0)
                        return Dequeue(priority);
                    else
                        m_priority = null;
                }

                foreach (var pair in m_tasks)
                {
                    if (pair.Value.Count > 0)
                        return Dequeue(pair.Key);
                }
            }
            catch (InvalidOperationException e)
            {
                ResetCount();
                throw e;
            }

            return null;
        }

        private PTThreadTask Dequeue(PTGroup group)
        {
            m_count--;
            return m_tasks[group].Dequeue();
        }

        private void ResetCount()
        {
            m_count = 0;
            foreach (var pair in m_tasks)
            {
                m_count += pair.Value.Count;
            }
        }
    }
}
