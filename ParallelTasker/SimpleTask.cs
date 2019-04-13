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
    public class SimpleTask
    {
        public readonly string groupString;
        public readonly PTGroup group;
        private static int count = 0;
        private int id;

        public SimpleTask(PTGroup group_)
        {
            id = count++;
            group = group_;
            groupString = Enum.GetName(typeof(PTGroup), group);
        }

        public object OnInitialize()
        {
            ParallelTasker.Log(group, $"Initializing {this}");
            return null;
        }

        public object Execute(object arg)
        {
            ParallelTasker.Log(group, $"Executing {this}");
            return null;
        }

        public void OnFinalize(object arg)
        {
            ParallelTasker.Log(group, $"Finalizing {this}");
        }

        public override string ToString()
        {
            return $"{base.ToString()}({id}, {groupString})";
        }
    }
}
