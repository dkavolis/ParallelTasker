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

namespace ParallelTasker
{
    public class PTGroupDict<T> : Dictionary<PTGroup, T>
    {
        public PTGroupDict() : this(0)
        { }
        public PTGroupDict(int capacity) : base()
        {
            Initialize(capacity);
        }

        private void Initialize(int capacity = 0)
        {
            foreach (var group in PTUtils.GetEnumValues<PTGroup>())
            {
                this[group] = Default(group, capacity);
            }
        }

        protected virtual T Default(PTGroup group, int capacity = 0)
        {
            return default(T);
        }

    }

    public class PTGroupDictList<T> : PTGroupDict<List<T>>
    {
        protected override List<T> Default(PTGroup group, int capacity = 0)
        {
            return new List<T>(capacity);
        }

    }

    public class PTGroupDictQueue<T> : PTGroupDict<Queue<T>>
    {
        protected override Queue<T> Default(PTGroup group, int capacity = 0)
        {
            return new Queue<T>(capacity);
        }

    }

    public class PTLoggers : PTGroupDict<PTGroupLogger>
    {
        protected override PTGroupLogger Default(PTGroup group, int capacity = 0)
        {
            return new PTGroupLogger(group);
        }
    }
}
