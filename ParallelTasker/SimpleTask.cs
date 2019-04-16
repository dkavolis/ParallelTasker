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

namespace ParallelTasker
{
    public class SimpleTask
    {
        public readonly string StartString, EndString;
        public readonly PTTimePair Start, End;
        private static int s_count;
        private readonly int m_id;
        private int m_counter;

        public SimpleTask(PTTimePair start, PTTimePair end)
        {
            m_id = s_count++;
            Start = start;
            End = end;
            StartString = start.ToString();
            EndString = end.ToString();
            m_counter = 0;
        }

        public object OnInitialize()
        {
            m_counter++;
            ParallelTasker.Log($"Initializing {GetString(m_counter)}");
            return m_counter;
        }

        public object Execute(object arg)
        {
            ParallelTasker.Log($"Executing    {GetString((int)arg)}");
            return arg;
        }

        public void OnFinalize(object arg)
        {
            ParallelTasker.Log($"Finalizing   {GetString((int)arg)}");
        }

        public override string ToString()
        {
            return GetString(m_counter);
        }

        public string GetString(int counter)
        {
            return $"{base.ToString()}({m_id.ToString("D2")}, {counter.ToString("D4")}, {StartString,28} -> {EndString,-28})";
        }
    }
}
