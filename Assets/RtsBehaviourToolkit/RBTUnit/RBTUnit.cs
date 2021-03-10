using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public partial class RBTUnit : MonoBehaviour
    {
        // Editor fields
        public SelectableVolume selectableVolume;

        // Public
        public static List<RBTUnit> ActiveUnits { get; private set; } = new List<RBTUnit>();
        public Vector3[] SelectablePoints { get => selectableVolume.GetPoints(transform.position, transform.rotation); }
        public bool Selected
        {
            get => _selected;
            set
            {
                if (value != _selected)
                {
                    var evnt = new SelectionEvent() { sender = this };
                    if (value)
                        _onSelected.Invoke(evnt);
                    else
                        _onDeselected.Invoke(evnt);
                }
                _selected = value;
            }
        }
        public event Action<SelectionEvent> OnSelected
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
        public event Action<SelectionEvent> OnDeselected
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

        public struct SelectionEvent
        {
            public RBTUnit sender;
        }

        // Private
        bool _selected = false;
        event Action<SelectionEvent> _onSelected = delegate { };
        event Action<SelectionEvent> _onDeselected = delegate { };
        readonly object _onSelectedLock = new object();
        readonly object _onDeselectedLock = new object();


        // Unity functions
        private void OnEnable()
        {
            ActiveUnits.Add(this);
        }

        private void OnDisable()
        {
            ActiveUnits.Remove(this);
        }

        private void OnDrawGizmosSelected()
        {
            var originalColor = Gizmos.color;
            Gizmos.color = Color.white;

            var p = SelectablePoints;
            // top
            Gizmos.DrawLine(p[0], p[1]);
            Gizmos.DrawLine(p[1], p[3]);
            Gizmos.DrawLine(p[3], p[2]);
            Gizmos.DrawLine(p[2], p[0]);
            // middle
            Gizmos.DrawLine(p[0], p[4]);
            Gizmos.DrawLine(p[1], p[5]);
            Gizmos.DrawLine(p[3], p[7]);
            Gizmos.DrawLine(p[2], p[6]);
            //bottom
            Gizmos.DrawLine(p[4], p[5]);
            Gizmos.DrawLine(p[5], p[7]);
            Gizmos.DrawLine(p[7], p[6]);
            Gizmos.DrawLine(p[6], p[4]);

            Gizmos.color = originalColor;
        }
    }
}