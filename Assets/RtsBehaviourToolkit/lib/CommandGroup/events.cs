using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public partial class CommandGroup
    {
        // Public
        public event Action<NewCornerEvent> OnNewCorner
        {
            add
            {
                lock (_onNewCornerLock)
                {
                    _onNewCorner += value;
                }
            }
            remove
            {
                lock (_onNewCornerLock)
                {
                    _onNewCorner -= value;
                }
            }
        }

        public event Action<NewGroupEvent> OnNewGroup
        {
            add
            {
                lock (_onNewGroupLock)
                {
                    _onNewGroup += value;
                }
            }
            remove
            {
                lock (_onNewGroupLock)
                {
                    _onNewGroup -= value;
                }
            }
        }

        public event Action<FinishedEvent> OnFinished
        {
            add
            {
                lock (_onFinishedLock)
                {
                    _onFinished += value;
                }
            }
            remove
            {
                lock (_onFinishedLock)
                {
                    _onFinished -= value;
                }
            }
        }

        public class Event
        {
            public Event(CommandGroup sender, CommandUnit unit)
            {
                this.sender = sender;
                this.unit = unit;
            }
            public readonly CommandGroup sender;
            public readonly CommandUnit unit;
        }

        public class NewCornerEvent : Event
        {
            public NewCornerEvent(CommandGroup sender, CommandUnit unit, Vector3 prevCorner)
                : base(sender, unit)
            {
                previousCorner = prevCorner;
            }
            public readonly Vector3 previousCorner;
        }

        public class NewGroupEvent : Event
        {
            public NewGroupEvent(CommandGroup sender, CommandUnit unit)
                : base(sender, unit)
            {
                previousGroupId = sender.Id;
            }
            public readonly string previousGroupId;
        }

        public class FinishedEvent : Event
        {
            public FinishedEvent(CommandGroup sender, CommandUnit unit)
                : base(sender, unit)
            {
            }
        }

        // Private
        Action<NewCornerEvent> _onNewCorner = delegate { };
        readonly object _onNewCornerLock = new object();
        Action<NewGroupEvent> _onNewGroup = delegate { };
        readonly object _onNewGroupLock = new object();
        Action<FinishedEvent> _onFinished = delegate { };
        readonly object _onFinishedLock = new object();
    }
}
