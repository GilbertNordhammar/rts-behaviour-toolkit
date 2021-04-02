using System;
using System.Collections.Generic;

namespace RtsBehaviourToolkit
{
    public partial class RBTUnit
    {
        public event Action<UnitEvent> OnSelected
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

        public event Action<UnitEvent> OnDeselected
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

        public static event Action<UnitEvent> OnActivated
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

        public static event Action<UnitEvent> OnDeactivated
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

        public struct UnitEvent
        {
            public RBTUnit sender;
        }

        // Private
        event Action<UnitEvent> _onSelected = delegate { };
        event Action<UnitEvent> _onDeselected = delegate { };
        static event Action<UnitEvent> _onActivated = delegate { };
        static event Action<UnitEvent> _onDeactivated = delegate { };
        readonly object _onSelectedLock = new object();
        readonly object _onDeselectedLock = new object();
        static readonly object _onActivatedLock = new object();
        static readonly object _onDeactivatedLock = new object();

    }
}