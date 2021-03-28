using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public partial class RBTUnitSelector : MonoBehaviour
    {
        // Editor fields
        [SerializeField]
        Material _material;

        // Public
        public event Action<SelectionEndEvent> OnSelectionEnd
        {
            add
            {
                lock (_onUnitsSelectedLock)
                {
                    _onSelectionEnd += value;
                }
            }
            remove
            {
                lock (_onUnitsSelectedLock)
                {
                    _onSelectionEnd -= value;
                }
            }
        }

        public struct SelectionEndEvent
        {
            public SelectionEndEvent(RBTUnitSelector sender, List<RBTUnit> units)
            {
                this.sender = sender;
                this.selectedUnits = units;
            }
            public readonly RBTUnitSelector sender;
            public readonly List<RBTUnit> selectedUnits;
        }

        public static RBTUnitSelector Instance { get; private set; }

        // Private
        Vector2 _selectBoxStartCorner;
        event Action<SelectionEndEvent> _onSelectionEnd = delegate { };
        readonly object _onUnitsSelectedLock = new object();
        List<RBTUnit> _selectedUnits = new List<RBTUnit>();

        void SelectUnits(SelectBox selectBox)
        {
            _selectedUnits.Clear();

            foreach (var unit in RBTUnit.ActiveUnits)
            {
                var pointOnScreen = Camera.main.WorldToScreenPoint(unit.Bounds.Center);
                bool selected = selectBox.IsWithinBox(pointOnScreen);
                foreach (var point in unit.Bounds.Corners)
                {
                    if (selected)
                    {
                        _selectedUnits.Add(unit);
                        break;
                    }
                    pointOnScreen = Camera.main.WorldToScreenPoint(point);
                    selected = selectBox.IsWithinBox(pointOnScreen);

                }
                unit.Selected = selected;
            }
        }

        // Unity functions
        void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitSelector on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                Destroy(this);
                return;
            }
            else Instance = this;

            if (!_material)
                Debug.LogError("Please assign a material in the inspector");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
                _selectBoxStartCorner = Input.mousePosition;
            else if (Input.GetMouseButtonUp(0))
                _onSelectionEnd.Invoke(new SelectionEndEvent(this, _selectedUnits));
        }

        void OnPostRender()
        {
            if (!Input.GetMouseButton(0))
                return;

            var selectBox = new SelectBox(_selectBoxStartCorner, Input.mousePosition);

            if (_material)
                selectBox.Draw(_material);

            SelectUnits(selectBox);
        }
    }
}

