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
    public static class PTUtils
    {
        public static T[] GetEnumValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static Dictionary<T, string> GetEnumNameMap<T>()
        {
            return GetEnumValues<T>().ToDictionary(value => value, value => Enum.GetName(typeof(T), value));
        }

        public static List<PTTimePair> GetAllTimePairs()
        {
            return (from update in GetEnumValues<PTUpdateEvent>()from time in GetEnumValues<PTEventTime>()select new PTTimePair(update, time)).ToList();
        }

        public static Dictionary<K, T> SwapKeysValues<T, K>(this Dictionary<T, K> dictionary)
        {
            return dictionary.ToDictionary(kp => kp.Value, kp => kp.Key);
        }
    }
}
