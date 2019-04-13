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
using UnityEngine;

namespace ParallelTasker
{
    public class PTGroupLogger
    {
        internal static readonly string timeFormat = "HH:mm:ss.fff";
        internal static readonly Dictionary<LogType, string> logMap = new Dictionary<LogType, string>()
        {
            {
            LogType.Log, "LOG"
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
        private PTGroup m_group;
        private string m_logStart;
        private StringBuilder m_sb = new StringBuilder();
        private object m_lock = new object();

        public PTGroup Group
        {
            get
            {
                return m_group;
            }
            set
            {
                m_group = value;
                SetupLogger();
            }
        }

        public bool Empty
        {
            get
            {
                return m_sb.Length == 0;
            }
        }

        public PTGroupLogger(PTGroup group)
        {
            m_group = group;
            SetupLogger();
        }

        protected virtual void SetupLogger()
        {
            m_logStart = $"Log messages received in '{Enum.GetName(typeof(PTGroup), m_group)}':";
        }

        public void Clear()
        {
            m_sb.Length = 0;
        }

        public void Flush()
        {
            if (m_sb.Length > 0)
            {
                m_sb.Insert(0, m_logStart);
                PTLogger.Info(m_sb.ToString());
                Clear();
            }
        }

        public void Log(object message)
        {
            Log(LogType.Log, message);
        }

        public void LogFormat(string format, params object[] args)
        {
            LogFormat(LogType.Log, format, args);
        }

        public void LogWarning(object message)
        {
            Log(LogType.Warning, message);
        }

        public void LogWarningFormat(string format, params object[] args)
        {
            LogFormat(LogType.Warning, format, args);
        }

        public void LogError(object message)
        {
            Log(LogType.Error, message);
        }

        public void LogErrorFormat(string format, params object[] args)
        {
            LogFormat(LogType.Error, format, args);
        }

        public void LogException(Exception exception)
        {
            Log(LogType.Exception, exception);
        }

        private StringBuilder AppendLogStamp(LogType logType)
        {
            return m_sb.AppendLine().Append("   [").Append(logMap[logType]).Append(" ").Append(System.DateTime.Now.ToString(timeFormat)).Append("] ");
        }

        private void Log(LogType logType, object message)
        {
            if (Debug.unityLogger.IsLogTypeAllowed(logType))
            {
                lock(m_lock)
                {
                    AppendLogStamp(logType).Append(message);
                }
            }
        }

        private void LogFormat(LogType logType, string format, params object[] args)
        {
            if (Debug.unityLogger.IsLogTypeAllowed(logType))
            {
                lock(m_lock)
                {
                    AppendLogStamp(logType).AppendFormat(format, args);
                }
            }
        }

    }
}
