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
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace ParallelTasker
{
    public static class PTThreadSafeLogger
    {
        internal const string TimeFormat = "HH:mm:ss.fff";
        internal static readonly Dictionary<LogType, string> s_logMap = new Dictionary<LogType, string>()
        {
            {
            LogType.Log,
            "LOG"
            },
            {
            LogType.Warning,
            "WRN"
            },
            {
            LogType.Error,
            "ERR"
            },
            {
            LogType.Exception,
            "EXC"
            }
        };

        private const string m_logStart = "PTThreadSafeLogger collected:";
        private static volatile StringBuilder m_sb = new StringBuilder();
        private static readonly object m_lock = new object();

        public static bool Empty
        {
            get
            {
                return m_sb.Length == 0;
            }
        }

        static PTThreadSafeLogger()
        {
            PTSynchronizerStart.Instance.OnLateUpdate += Flush;
        }

        public static void Clear()
        {
            m_sb.Length = 0;
        }

        public static void Flush()
        {
            if (m_sb.Length <= 0)return;
            lock(m_lock)
            {
                m_sb.Insert(0, m_logStart);
                PTLogger.Info(m_sb.ToString());
                Clear();
            }
        }

        [Conditional("DEBUG")]
        public static void Debug(object message)
        {
            Log(LogType.Log, message);
        }

        [Conditional("DEBUG")]
        public static void DebugFormat(string format, params object[] args)
        {
            LogFormat(LogType.Log, format, args);
        }

        public static void Log(object message)
        {
            Log(LogType.Log, message);
        }

        public static void LogFormat(string format, params object[] args)
        {
            LogFormat(LogType.Log, format, args);
        }

        public static void LogWarning(object message)
        {
            Log(LogType.Warning, message);
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            LogFormat(LogType.Warning, format, args);
        }

        public static void LogError(object message)
        {
            Log(LogType.Error, message);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            LogFormat(LogType.Error, format, args);
        }

        public static void LogException(Exception exception)
        {
            Log(LogType.Exception, exception);
        }

        private static StringBuilder AppendLogStamp(LogType logType)
        {
            return m_sb.Append("\n   [").Append(s_logMap[logType]).Append(" ").Append(DateTime.Now.ToString(TimeFormat)).Append("] ");
        }

        private static void Log(LogType logType, object message)
        {
            if (!UnityEngine.Debug.unityLogger.IsLogTypeAllowed(logType))return;
            lock(m_lock)
            {
                AppendLogStamp(logType).Append(message);
            }
        }

        private static void LogFormat(LogType logType, string format, params object[] args)
        {
            if (!UnityEngine.Debug.unityLogger.IsLogTypeAllowed(logType))return;
            lock(m_lock)
            {
                AppendLogStamp(logType).AppendFormat(format, args);
            }
        }

    }
}
