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

        public event Action<UnitChangedGroupEvent> OnChangedGroup
        {
            add
            {
                lock (_onChangedGroupLock)
                {
                    _onChangedGroup += value;
                }
            }
            remove
            {
                lock (_onChangedGroupLock)
                {
                    _onChangedGroup -= value;
                }
            }
        }

        public event Action<NewPathEvent> OnNewPath
        {
            add
            {
                lock (_onNewPathLock)
                {
                    _onNewPath += value;
                }
            }
            remove
            {
                lock (_onNewPathLock)
                {
                    _onNewPath -= value;
                }
            }
        }

        public event Action<MainPathTraversedEvent> OnMainPathTraversed
        {
            add
            {
                lock (_onMainPathTraversedLock)
                {
                    _onMainPathTraversed += value;
                }
            }
            remove
            {
                lock (_onMainPathTraversedLock)
                {
                    _onMainPathTraversed -= value;
                }
            }
        }

        public class PathEvent
        {
            public PathEvent(CommandGroup sender, CommandUnit unit)
            {
                this.sender = sender;
                this.unit = unit;
            }
            public readonly CommandGroup sender;
            public readonly CommandUnit unit;
        }

        public class NewPathNodeEvent : PathEvent
        {
            public NewPathNodeEvent(CommandGroup sender, CommandUnit unit)
                : base(sender, unit)
            {
            }
        }

        public class UnitChangedGroupEvent : PathEvent
        {
            public UnitChangedGroupEvent(CommandGroup sender, CommandUnit unit)
                : base(sender, unit)
            {
                previousGroupId = sender.Id;
            }
            public readonly string previousGroupId;
        }

        public class NewPathEvent : PathEvent
        {
            public NewPathEvent(CommandGroup sender, CommandUnit unit)
                : base(sender, unit)
            {
            }
        }

        public class MainPathTraversedEvent : PathEvent
        {
            public MainPathTraversedEvent(CommandGroup sender, CommandUnit unit)
                : base(sender, unit)
            {
            }
        }

        // Private
        Action<NewPathNodeEvent> _onNewPathNode = delegate { };
        readonly object _onNewPathNodeLock = new object();
        Action<UnitChangedGroupEvent> _onChangedGroup = delegate { };
        readonly object _onChangedGroupLock = new object();
        Action<NewPathEvent> _onNewPath = delegate { };
        readonly object _onNewPathLock = new object();
        Action<MainPathTraversedEvent> _onMainPathTraversed = delegate { };
        readonly object _onMainPathTraversedLock = new object();
    }
}
