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
    internal static class PTLogger
    {

        public const string Tag = "[ParallelTasker] ";

        [Conditional("DEBUG_SYNCHRONIZATION")]
        public static void DebugSynchronization(object message)
        {
            UnityEngine.Debug.Log(Tag + message);
        }

        #region Info
        public static void Info(object message)
        {
            UnityEngine.Debug.Log(Tag + message);
        }

        public static void Info(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(Tag + message, context);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(Tag + format, args);
        }

        public static void InfoFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(context, Tag + format, args);
        }

        #endregion // Info

        #region Debug
        [Conditional("DEBUG")]
        public static void Debug(object message)
        {
            UnityEngine.Debug.Log(Tag + message);
        }

        [Conditional("DEBUG")]
        public static void Debug(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.Log(Tag + message, context);
        }

        [Conditional("DEBUG")]
        public static void DebugFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(Tag + format, args);
        }

        [Conditional("DEBUG")]
        public static void DebugFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(context, Tag + format, args);
        }

        #endregion // Debug

        #region Warning
        public static void Warning(object message)
        {
            UnityEngine.Debug.LogWarning(Tag + message);
        }

        public static void Warning(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogWarning(Tag + message, context);
        }

        public static void WarningFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(Tag + format, args);
        }

        public static void WarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(context, Tag + format, args);
        }

        #endregion // Warning

        #region Error
        public static void Error(object message)
        {
            UnityEngine.Debug.LogError(Tag + message);
        }

        public static void Error(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogError(Tag + message, context);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(Tag + format, args);
        }

        public static void ErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(context, Tag + format, args);
        }

        #endregion // Error

        #region Assertion
        [Conditional("UNITY_ASSERTIONS")]
        public static void Assertion(object message)
        {
            UnityEngine.Debug.LogAssertion(Tag + message);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void Assertion(object message, UnityEngine.Object context)
        {
            UnityEngine.Debug.LogAssertion(Tag + message, context);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AssertionFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogAssertionFormat(Tag + format, args);
        }

        [Conditional("UNITY_ASSERTIONS")]
        public static void AssertionFormat(UnityEngine.Object context, string format, params object[] args)
        {
            UnityEngine.Debug.LogAssertionFormat(context, Tag + format, args);
        }

        #endregion // Assertion

        #region Exception
        public static void Exception(Exception exception)
        {
            Error("Logged exception:");
            UnityEngine.Debug.LogException(exception);
        }

        public static void Exception(Exception exception, UnityEngine.Object context)
        {
            Error("Logged exception:");
            UnityEngine.Debug.LogException(exception, context);
        }

        #endregion // Exception
    }
}
