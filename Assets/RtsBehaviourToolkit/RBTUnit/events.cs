using System;
using System.Collections.Generic;

namespace RtsBehaviourToolkit
{
    public partial class RBTUnit
    {
        public event Action<OnSelectedEvent> OnSelected
        {
            add
            {
                lock (_onSelectedLock)
                {
                    _onSelected += value;
                }
            }
            remove
            {
                lock (_onSelectedLock)
                {
                    _onSelected -= value;
                }
            }
        }

        public event Action<OnDeselectedEvent> OnDeselected
        {
            add
            {
                lock (_onDeselectedLock)
                {
                    _onDeselected += value;
                }
            }
            remove
            {
                lock (_onDeselectedLock)
                {
                    _onDeselected -= value;
                }
            }
        }

        public static event Action<OnActivatedEvent> OnActivated
        {
            add
            {
                lock (_onActivatedLock)
                {
                    _onActivated += value;
                }
            }
            remove
            {
                lock (_onActivatedLock)
                {
                    _onActivated -= value;
                }
            }
        }

        public static event Action<OnDeActivatedEvent> OnDeactivated
        {
            add
            {
                lock (_onDeactivatedLock)
                {
                    _onDeactivated += value;
                }
            }
            remove
            {
                lock (_onDeactivatedLock)
                {
                    _onDeactivated -= value;
                }
            }
        }

        public event Action<OnStateChangedEvent> OnStateChanged
        {
            add
            {
                lock (_onStateChangedLock)
                {
                    _onStateChanged += value;
                }
            }
            remove
            {
                lock (_onStateChangedLock)
                {
                    _onStateChanged -= value;
                }
            }
        }

        public abstract class UnitEvent
        {
            public UnitEvent(RBTUnit sender)
            {
                Sender = sender;
            }
            public readonly RBTUnit Sender;
        }

        public class OnSelectedEvent : UnitEvent
        {
            public OnSelectedEvent(RBTUnit sender)
                : base(sender)
            {
            }
        }

        public class OnDeselectedEvent : UnitEvent
        {
            public OnDeselectedEvent(RBTUnit sender)
                : base(sender)
            {
            }
        }

        public class OnActivatedEvent : UnitEvent
        {
            public OnActivatedEvent(RBTUnit sender)
                : base(sender)
            {
            }
        }

        public class OnDeActivatedEvent : UnitEvent
        {
            public OnDeActivatedEvent(RBTUnit sender)
                : base(sender)
            {
            }
        }

        public class OnStateChangedEvent : UnitEvent
        {
            public OnStateChangedEvent(RBTUnit sender, UnitState prevState, UnitState newState)
                : base(sender)
            {
                PrevState = prevState;
                NewState = newState;
            }

            public readonly UnitState PrevState, NewState;
        }

        // Private
        event Action<OnSelectedEvent> _onSelected = delegate { };
        readonly object _onSelectedLock = new object();

        event Action<OnDeselectedEvent> _onDeselected = delegate { };
        readonly object _onDeselectedLock = new object();

        static event Action<OnActivatedEvent> _onActivated = delegate { };
        static readonly object _onActivatedLock = new object();

        static event Action<OnDeActivatedEvent> _onDeactivated = delegate { };
        static readonly object _onDeactivatedLock = new object();

        event Action<OnStateChangedEvent> _onStateChanged = delegate { };
        static readonly object _onStateChangedLock = new object();
    }
}