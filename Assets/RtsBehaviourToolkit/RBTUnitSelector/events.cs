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

        public class OnUnitsSelectedEvent
        {
            public OnUnitsSelectedEvent(RBTUnitSelector sender, List<RBTUnit> units, Team team)
            {
                Sender = sender;
                SelectedUnits = new List<RBTUnit>(units);
                Team = team;
            }

            public RBTUnitSelector Sender { get; }
            public List<RBTUnit> SelectedUnits { get; }
            public Team Team { get; }

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