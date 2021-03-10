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
        public event Action<UnitsSelectedEvent> OnUnitsSelected
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

        public struct UnitsSelectedEvent
        {
            public UnitsSelectedEvent(RBTUnitSelector sender, List<RBTUnit> units)
            {
                this.sender = sender;
                this.units = units;
            }
            readonly public RBTUnitSelector sender;
            readonly public List<RBTUnit> units;
        }

        // Private
        Vector2 _selectBoxStartCorner;
        event Action<UnitsSelectedEvent> _onUnitsSelected = delegate { };
        readonly object _onUnitsSelectedLock = new object();

        // Unity functions
        void Awake()
        {
            if (!_material)
                Debug.LogError("Please assign a material in the inspector");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _selectBoxStartCorner = Input.mousePosition;
            }
        }

        void OnPostRender()
        {
            if (Input.GetMouseButton(0))
            {
                var selectBox = new SelectBox(_selectBoxStartCorner, Input.mousePosition);
                if (_material)
                    selectBox.Draw(_material);

                var selectedUnits = new List<RBTUnit>();
                foreach (var unit in RBTUnit.ActiveUnits)
                {
                    bool selected = false;
                    foreach (var point in unit.SelectablePoints)
                    {
                        var pointOnScreen = Camera.main.WorldToScreenPoint(point);
                        selected = selectBox.IsWithinBox(pointOnScreen);
                        if (selected)
                        {
                            selectedUnits.Add(unit);
                            break;
                        }
                    }
                    unit.Selected = selected;
                }

                if (selectedUnits.Count > 0)
                    _onUnitsSelected.Invoke(new UnitsSelectedEvent(this, selectedUnits));
            }
        }
    }
}

