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
using UnityEngine;

namespace ParallelTasker.Unity
{
    public class SimpleTaskEditor : Singleton<SimpleTaskEditor>
    {
        private List<SimpleTask> m_tasks;
        [SerializeField] private uint m_period = 1;

        // float argument to work with slider
        public void SetPeriod(float value)
        {
            m_period = (uint)value;
        }

        protected override void OnAwake()
        {
            m_tasks = new List<SimpleTask>();
        }

        public void NewUpdateTask()
        {
            NewTask(new PTTimePair(PTUpdateEvent.Update, PTEventTime.Start), new PTTimePair(PTUpdateEvent.Update, PTEventTime.End));
        }

        public void NewLateUpdateTask()
        {
            NewTask(new PTTimePair(PTUpdateEvent.LateUpdate, PTEventTime.Start), new PTTimePair(PTUpdateEvent.LateUpdate, PTEventTime.End));
        }

        public void NewFixedUpdateTask()
        {
            NewTask(new PTTimePair(PTUpdateEvent.FixedUpdate, PTEventTime.Start), new PTTimePair(PTUpdateEvent.FixedUpdate, PTEventTime.End));
        }

        public void NewUpdateFrameTask()
        {
            NewTask(new PTTimePair(PTUpdateEvent.Update, PTEventTime.Start), new PTTimePair(PTUpdateEvent.Update, PTEventTime.Start));
        }

        public void NewFixedUpdateFrameTask()
        {
            NewTask(new PTTimePair(PTUpdateEvent.FixedUpdate, PTEventTime.Start), new PTTimePair(PTUpdateEvent.FixedUpdate, PTEventTime.Start));
        }

        public void NewLateUpdateFrameTask()
        {
            NewTask(new PTTimePair(PTUpdateEvent.LateUpdate, PTEventTime.Start), new PTTimePair(PTUpdateEvent.LateUpdate, PTEventTime.Start));
        }

        public void NewTask(PTTimePair start, PTTimePair end)
        {
            var task = new SimpleTask(start, end);
            ParallelTasker.AddTask(start, end, task.OnInitialize, task.Execute, task.OnFinalize, m_period);
            PTLogger.Warning($"Added {task}");
            m_tasks.Add(task);
        }

        public void RemoveLastTask()
        {
            var last = m_tasks.Count - 1;
            if (last < 0)
                return;
            var task = m_tasks[last];
            if (!ParallelTasker.RemoveTask(task.Start, task.Execute))
                PTLogger.Error($"Failed to remove task {task}");
            else
            {
                m_tasks.RemoveAt(last);
                PTLogger.Warning($"Removed task {task}");
            }
        }

        public void ClearTasks()
        {
            ParallelTasker.ClearTasks();
            m_tasks.Clear();
            PTLogger.Warning("Cleared all tasks");
        }
    }
}
