using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public partial class CommandGroup
    {
        // Public
        public event Action<NewPathNodeEvent> OnNewPathNode
        {
            add
            {
                lock (_onNewPathNodeLock)
                {
                    _onNewPathNode += value;
                }
            }
            remove
            {
                lock (_onNewPathNodeLock)
                {
                    _onNewPathNode -= value;
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

        public event Action<PathTraversedEvent> OnPathTraversed
        {
            add
            {
                lock (_onPathTraversedLock)
                {
                    _onPathTraversed += value;
                }
            }
            remove
            {
                lock (_onPathTraversedLock)
                {
                    _onPathTraversed -= value;
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

        public class NewPathNodeEvent : Event
        {
            public NewPathNodeEvent(CommandGroup sender, CommandUnit unit, Vector3 prevCorner)
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

        public class PathTraversedEvent : Event
        {
            public PathTraversedEvent(CommandGroup sender, CommandUnit unit, bool lastPath)
                : base(sender, unit)
            {
                LastPath = lastPath;
            }

            bool LastPath { get; }
        }

        // Private
        Action<NewPathNodeEvent> _onNewPathNode = delegate { };
        readonly object _onNewPathNodeLock = new object();
        Action<NewGroupEvent> _onNewGroup = delegate { };
        readonly object _onNewGroupLock = new object();
        Action<PathTraversedEvent> _onPathTraversed = delegate { };
        readonly object _onPathTraversedLock = new object();
    }
}
