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
    public static class ParallelTasker
    {
        public static bool ResetTasksOnSceneChange
        {
            get
            {
                return PTController.ResetOnSceneChange;
            }
            set
            {
                PTController.ResetOnSceneChange = value;
            }
        }

        public static PTTask AddTask(PTGroup group, Func<object, object> task, uint period = 1)
        {
            return PTController.Instance.Tasks.AddTask(group, task, period);
        }

        public static PTTask AddTask(PTGroup group, Func<object> initializer, Func<object, object> task, uint period = 1)
        {
            return PTController.Instance.Tasks.AddTask(group, initializer, task, period);
        }

        public static PTTask AddTask(PTGroup group, Func<object, object> task, Action<object> finalizer, uint period = 1)
        {
            return PTController.Instance.Tasks.AddTask(group, null, task, finalizer, period);
        }

        public static PTTask AddTask(PTGroup group, Func<object> initializer, Func<object, object> task, Action<object> finalizer, uint period = 1)
        {
            return PTController.Instance.Tasks.AddTask(group, initializer, task, finalizer, period);
        }

        public static PTTask AddTask(PTGroup group, PTTask task)
        {
            return PTController.Instance.Tasks.AddTask(group, task);
        }

        public static bool RemoveTask(PTGroup group, PTTask task)
        {
            var tasks = PTController.Instance.Tasks[group];
            int index = tasks.IndexOf(task);
            if (index < 0)
                return false;
            tasks[index].Release();
            tasks.RemoveAt(index);
            return true;
        }

        public static bool RemoveTask(PTGroup group, Func<object, object> task)
        {
            var tasks = PTController.Instance.Tasks[group];

            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i] != null)
                {
                    if (tasks[i].main == task)
                    {
                        tasks[i].Release();
                        tasks.RemoveAt(i);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Log(PTGroup group, object message)
        {
            PTController.Instance.Loggers[group].Log(message);
        }

        public static void LogFormat(PTGroup group, string format, params object[] args)
        {
            PTController.Instance.Loggers[group].LogFormat(format, args);
        }

        public static void LogError(PTGroup group, object message)
        {
            PTController.Instance.Loggers[group].LogError(message);
        }

        public static void LogErrorFormat(PTGroup group, string format, params object[] args)
        {
            PTController.Instance.Loggers[group].LogErrorFormat(format, args);
        }

        public static void LogWarning(PTGroup group, object message)
        {
            PTController.Instance.Loggers[group].LogWarning(message);
        }

        public static void LogWarningFormat(PTGroup group, string format, params object[] args)
        {
            PTController.Instance.Loggers[group].LogWarningFormat(format, args);
        }

        public static void LogException(PTGroup group, Exception exception)
        {
            PTController.Instance.Loggers[group].LogException(exception);
        }

        public static void ClearTasks()
        {
            PTController.Instance.Tasks.ClearTasks();
        }

        public static void ClearTasks(PTGroup group)
        {
            PTController.Instance.Tasks.ClearTasks(group);
        }

    }
}
