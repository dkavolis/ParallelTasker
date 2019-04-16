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
        private List<SimpleTask> tasks;
        [SerializeField] uint m_period = 1;

        // float argument to work with slider
        public void SetPeriod(float value)
        {
            m_period = (uint)value;
        }

        protected override void OnAwake()
        {
            tasks = new List<SimpleTask>();
        }

        public void NewUpdateTask()
        {
            NewTask(PTGroup.Update);
        }

        public void NewLateUpdateTask()
        {
            NewTask(PTGroup.LateUpdate);
        }

        public void NewFixedUpdateTask()
        {
            NewTask(PTGroup.FixedUpdate);
        }

        public void NewUpdateFrameTask()
        {
            NewTask(PTGroup.UpdateFrame);
        }

        public void NewFixedUpdateFrameTask()
        {
            NewTask(PTGroup.FixedUpdateFrame);
        }

        public void NewLateUpdateFrameTask()
        {
            NewTask(PTGroup.LateUpdateFrame);
        }

        public void NewTask(PTGroup group)
        {
            var task = new SimpleTask(group);
            ParallelTasker.AddTask(group, task.OnInitialize, task.Execute, task.OnFinalize, m_period);
            Debug.LogWarning($"Added {task}");
            tasks.Add(task);
        }

        public void RemoveLastTask()
        {
            int last = tasks.Count - 1;
            if (last < 0)
                return;
            SimpleTask task = tasks[last];
            if (!ParallelTasker.RemoveTask(task.group, task.Execute))
                Debug.LogError($"Failed to remove task {task}");
            else
            {
                tasks.RemoveAt(last);
            }
        }
    }
}
