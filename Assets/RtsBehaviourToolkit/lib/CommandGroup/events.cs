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
