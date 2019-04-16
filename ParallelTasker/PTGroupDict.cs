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

namespace ParallelTasker
{
    public class PTGroupDict<T> : Dictionary<PTTimePair, T>
    {
        public PTGroupDict() : this(null)
        { }
        public PTGroupDict(Func<PTTimePair, T> create)
        {

            Initialize(create);
        }

        private void Initialize(Func<PTTimePair, T> create)
        {
            var factory = create ?? (pair => default(T));
            foreach (var group in PTUtils.GetAllTimePairs())
            {
                try
                {
                    this[group] = factory(group);
                }
                catch (Exception)
                {
                    PTLogger.Error($"Error while initializing {this} with {group.ToString()}");
                    throw;
                }
            }
        }
    }
}
