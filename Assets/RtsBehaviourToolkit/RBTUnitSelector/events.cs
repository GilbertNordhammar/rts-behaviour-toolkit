using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    // Public
    public partial class RBTUnitSelector : MonoBehaviour
    {
        public event Action<OnUnitsSelectedEvent> OnUnitsSelected
        {
            add
            {
                lock (_onUnitsSelectedLock)
                {
                    _onUnitsSelected += value;
                }
            }
            remove
            {
                lock (_onUnitsSelectedLock)
                {
                    _onUnitsSelected -= value;
                }
            }
        }

        public struct OnUnitsSelectedEvent
        {
            public OnUnitsSelectedEvent(RBTUnitSelector sender, List<RBTUnit> units)
            {
                this.sender = sender;
                this.selectedUnits = units;
            }
            public readonly RBTUnitSelector sender;
            public readonly List<RBTUnit> selectedUnits;
        }

        // Private
        event Action<OnUnitsSelectedEvent> _onUnitsSelected = delegate { };
        readonly object _onUnitsSelectedLock = new object();
    }
}