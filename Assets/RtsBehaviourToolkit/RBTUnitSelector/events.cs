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
            public OnUnitsSelectedEvent(RBTUnitSelector sender, List<RBTUnit> units, Team team)
            {
                Sender = sender;
                SelectedUnits = units;
                Team = team;
            }
            public readonly RBTUnitSelector Sender;
            public readonly List<RBTUnit> SelectedUnits;
            public readonly Team Team;

            public static implicit operator bool(OnUnitsSelectedEvent me)
            {
                return !object.ReferenceEquals(me, null);
            }
        }

        // Private
        event Action<OnUnitsSelectedEvent> _onUnitsSelected = delegate { };
        readonly object _onUnitsSelectedLock = new object();
    }
}