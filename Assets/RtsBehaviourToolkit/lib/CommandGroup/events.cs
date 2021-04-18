using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public partial class CommandGroup
    {
        // Public
        public event Action<OnUnitsRemove> OnUnitsWillBeRemoved
        {
            add
            {
                lock (_onUnitsWillBeRemovedLock)
                {
                    _onUnitsWillBeRemoved += value;
                }
            }
            remove
            {
                lock (_onUnitsWillBeRemovedLock)
                {
                    _onUnitsWillBeRemoved -= value;
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

        public class OnUnitsRemove
        {
            public OnUnitsRemove(int[] indices)
            {
                UnitsIndices = indices;
            }
            public readonly int[] UnitsIndices;
        }

        // Private
        Action<OnUnitsRemove> _onUnitsWillBeRemoved = delegate { };
        readonly object _onUnitsWillBeRemovedLock = new object();
    }
}
