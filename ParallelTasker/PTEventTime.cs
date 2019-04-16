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
using System.Linq;

namespace ParallelTasker
{
    public enum PTUpdateEvent
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    public enum PTEventTime
    {
        Start = -8008,
        Precalc = -101,
        Early = -99,
        Earlyish = -1,
        Normal = 0,
        FashionablyLate = 7,
        FlightIntegrator = 9,
        Late = 19,
        End = 8008
    }

    public struct PTTimePair : IEquatable<PTTimePair>
    {
        public readonly PTUpdateEvent UpdateEvent;
        public readonly PTEventTime EventTime;

        public PTTimePair(PTUpdateEvent updateEvent, PTEventTime eventTime)
        {
            UpdateEvent = updateEvent;
            EventTime = eventTime;
        }

        public override string ToString()
        {
            return $"{PTUpdateEventMap.ToName(UpdateEvent)}.{PTEventTimeMap.ToName(EventTime)}";
        }

        public bool Equals(PTTimePair other)
        {
            return UpdateEvent == other.UpdateEvent && EventTime == other.EventTime;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)return false;
            return obj is PTTimePair other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)UpdateEvent * 397) ^ (int)EventTime;
            }
        }

        public static PTTimePair DefaultUpdate
        {
            get
            {
                return new PTTimePair(PTUpdateEvent.Update, PTEventTime.Normal);
            }
        }

        public static PTTimePair DefaultLateUpdate
        {
            get
            {
                return new PTTimePair(PTUpdateEvent.LateUpdate, PTEventTime.Normal);
            }
        }

        public static PTTimePair DefaultFixedUpdate
        {
            get
            {
                return new PTTimePair(PTUpdateEvent.FixedUpdate, PTEventTime.Normal);
            }
        }
    }

    public class PTEnumNameMap<T>
    {
        private static readonly Dictionary<T, string> s_nameMap = PTUtils.GetEnumNameMap<T>();
        private static readonly Dictionary<string, T> s_mapName = s_nameMap.SwapKeysValues();

        public static string ToName(T value)
        {
            return s_nameMap[value];
        }

        public static T FromName(string name)
        {
            if (!s_mapName.ContainsKey(name))throw new ArgumentException($"Invalid name '{name}'");
            return s_mapName[name];
        }
    }

    public class PTEventTimeMap : PTEnumNameMap<PTEventTime>
    { }

    public class PTUpdateEventMap : PTEnumNameMap<PTUpdateEvent>
    { }

    public class PTTimePairMap
    {
        private static readonly Dictionary<string, PTTimePair> s_mapName = PTUtils.GetAllTimePairs().ToDictionary(pair => pair.ToString());
        private static readonly Dictionary<PTTimePair, string> s_nameMap = s_mapName.SwapKeysValues();

        public static string ToName(PTTimePair value)
        {
            return s_nameMap[value];
        }

        public static string ToName(PTUpdateEvent updateEvent, PTEventTime eventTime)
        {
            return ToName(new PTTimePair(updateEvent, eventTime));
        }

        public static PTTimePair FromName(string name)
        {
            if (!s_mapName.ContainsKey(name))throw new ArgumentException($"Invalid name '{name}'");
            return s_mapName[name];
        }

        public static PTTimePair FromName(string updateEvent, string eventTime)
        {
            return new PTTimePair(PTUpdateEventMap.FromName(updateEvent), PTEventTimeMap.FromName(eventTime));
        }
    }
}
