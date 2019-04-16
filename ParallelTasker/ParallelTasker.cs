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
using System.Diagnostics;

namespace ParallelTasker
{
    public static class ParallelTasker
    {
        public static bool ResetTasksOnSceneChange
        {
            get
            {
                return PTAddon.Instance.ResetOnSceneChange;
            }
            set
            {
                PTAddon.Instance.ResetOnSceneChange = value;
            }
        }

        public static bool SubscriptionStatus(PTTimePair taskGroup)
        {
            return PTAddon.Instance.Controller.Tasks[taskGroup].SubscriptionStatus;
        }

        public static bool SubscriptionStatus(PTUpdateEvent updateEvent, PTEventTime eventTime)
        {
            return SubscriptionStatus(new PTTimePair(updateEvent, eventTime));
        }

        public static void Subscribe(PTTimePair timePair, Action handler)
        {
            PTAddon.Instance.Synchronizers[timePair.EventTime].Subscribe(timePair.UpdateEvent, handler);
        }

        public static void Subscribe(PTUpdateEvent updateEvent, PTEventTime eventTime, Action handler)
        {
            Subscribe(new PTTimePair(updateEvent, eventTime), handler);
        }

        public static void Unsubscribe(PTTimePair timePair, Action handler)
        {
            PTAddon.Instance.Synchronizers[timePair.EventTime].Unsubscribe(timePair.UpdateEvent, handler);
        }

        public static void Unsubscribe(PTUpdateEvent updateEvent, PTEventTime eventTime, Action handler)
        {
            Unsubscribe(new PTTimePair(updateEvent, eventTime), handler);
        }

        public static PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object, object> task, uint period = 1)
        {
            return PTAddon.Instance.Controller.Tasks[startTime].AddTask(endTime, task, period);
        }

        public static PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object> initializer, Func<object, object> task, uint period = 1)
        {
            return PTAddon.Instance.Controller.Tasks[startTime].AddTask(endTime, initializer, task, period);
        }

        public static PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object, object> task, Action<object> finalizer, uint period = 1)
        {
            return PTAddon.Instance.Controller.Tasks[startTime].AddTask(endTime, null, task, finalizer, period);
        }

        public static PTTask AddTask(PTTimePair startTime, PTTimePair endTime, Func<object> initializer, Func<object, object> task, Action<object> finalizer, uint period = 1)
        {
            return PTAddon.Instance.Controller.Tasks[startTime].AddTask(endTime, initializer, task, finalizer, period);
        }

        public static PTTask AddTask(PTTimePair startTime, PTTimePair endTime, PTTask task)
        {
            return PTAddon.Instance.Controller.Tasks[startTime].AddTask(endTime, task);
        }

        public static bool RemoveTask(PTTimePair startTime, PTTask task)
        {
            return PTAddon.Instance.Controller.Tasks[startTime].RemoveTask(task);
        }

        public static bool RemoveTask(PTTimePair startTime, Func<object, object> task)
        {
            return PTAddon.Instance.Controller.Tasks[startTime].RemoveTask(task);
        }

        [Conditional("DEBUG")]
        public static void Debug(object message)
        {
            PTThreadSafeLogger.Debug(message);
        }

        [Conditional("DEBUG")]
        public static void DebugFormat(string format, params object[] args)
        {
            PTThreadSafeLogger.DebugFormat(format, args);
        }

        public static void Log(object message)
        {
            PTThreadSafeLogger.Log(message);
        }

        public static void LogFormat(string format, params object[] args)
        {
            PTThreadSafeLogger.LogFormat(format, args);
        }

        public static void LogError(object message)
        {
            PTThreadSafeLogger.LogError(message);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            PTThreadSafeLogger.LogErrorFormat(format, args);
        }

        public static void LogWarning(object message)
        {
            PTThreadSafeLogger.LogWarning(message);
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            PTThreadSafeLogger.LogWarningFormat(format, args);
        }

        public static void LogException(Exception exception)
        {
            PTThreadSafeLogger.LogException(exception);
        }

        public static void ClearTasks()
        {
            PTAddon.Instance.Controller.ResetCurrentTasks();
        }

        public static void ClearTasks(PTTimePair startTime)
        {
            PTAddon.Instance.Controller.Tasks[startTime].ClearTasks();
        }

    }
}
