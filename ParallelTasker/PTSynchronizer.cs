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
using UnityEngine;

namespace ParallelTasker
{
    public interface IPTSynchronizer
    {
        event Action OnUpdate, OnLateUpdate, OnFixedUpdate;
        void Subscribe(PTUpdateEvent updateEvent, Action handler);
        void Unsubscribe(PTUpdateEvent updateEvent, Action handler);
        PTEventTime EventTime
        {
            get;
        }
    }

    public class PTSynchronizer<T> : Singleton<T>, IPTSynchronizer where T : MonoBehaviour
    {
        private class EventSubscriber
        {
            public readonly Action<Action> subscribe;
            public readonly Action<Action> unsubscribe;

            public EventSubscriber(Action<Action> subscribe, Action<Action> unsubscribe)
            {
                this.subscribe = subscribe;
                this.unsubscribe = unsubscribe;
            }

        }
        public event Action OnUpdate, OnLateUpdate, OnFixedUpdate;

        private Dictionary<PTUpdateEvent, EventSubscriber> m_eventDictionary;

        protected override void OnAwake()
        {
            m_eventDictionary = new Dictionary<PTUpdateEvent, EventSubscriber>()
            {
                {
                PTUpdateEvent.Update,
                new EventSubscriber(handler => OnUpdate += handler, handler => OnUpdate -= handler)
                },
                {
                PTUpdateEvent.LateUpdate,
                new EventSubscriber(handler => OnLateUpdate += handler, handler => OnLateUpdate -= handler)
                },
                {
                PTUpdateEvent.FixedUpdate,
                new EventSubscriber(handler => OnFixedUpdate += handler, handler => OnFixedUpdate -= handler)
                }
            };
        }

        public virtual PTEventTime EventTime
        {
            get
            {
                return PTEventTime.Normal;
            }
        }

        public void Subscribe(PTUpdateEvent updateEvent, Action handler)
        {
            m_eventDictionary[updateEvent].subscribe(handler);
        }

        public void Unsubscribe(PTUpdateEvent updateEvent, Action handler)
        {
            m_eventDictionary[updateEvent].unsubscribe(handler);
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        protected override void OnSingletonDestroy()
        {
            m_eventDictionary?.Clear();
            base.OnSingletonDestroy();
        }
    }

    public class PTSynchronizerStart : PTSynchronizer<PTSynchronizerStart>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.Start;
            }
        }
    }

    public class PTSynchronizerEnd : PTSynchronizer<PTSynchronizerEnd>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.End;
            }
        }
    }

    public class PTSynchronizerEarly : PTSynchronizer<PTSynchronizerEarly>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.Early;
            }
        }
    }

    public class PTSynchronizerPrecalc : PTSynchronizer<PTSynchronizerPrecalc>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.Precalc;
            }
        }
    }

    public class PTSynchronizerEarlyish : PTSynchronizer<PTSynchronizerEarlyish>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.Earlyish;
            }
        }
    }

    public class PTSynchronizerNormal : PTSynchronizer<PTSynchronizerNormal>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.Normal;
            }
        }
    }

    public class PTSynchronizerFashionablyLate : PTSynchronizer<PTSynchronizerFashionablyLate>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.FashionablyLate;
            }
        }
    }

    public class PTSynchronizerFlightIntegrator : PTSynchronizer<PTSynchronizerFlightIntegrator>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.FlightIntegrator;
            }
        }
    }

    public class PTSynchronizerLate : PTSynchronizer<PTSynchronizerLate>
    {
        public override PTEventTime EventTime
        {
            get
            {
                return PTEventTime.Late;
            }
        }
    }
}
