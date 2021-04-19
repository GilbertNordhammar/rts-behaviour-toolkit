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

        [SerializeField]
        LayerMask _selectable;

        [SerializeField]
        Team _selectableTeam;

        // Public
        public static RBTUnitSelector Instance { get; private set; }

        // Private
        Vector2 _selectBoxStartCorner;
        List<RBTUnit> _selectedUnits = new List<RBTUnit>();
        bool _hasSelectedSingle = false;

        void TrySelectMultipleUnits(SelectBox selectBox)
        {
            if (!_selectableTeam) return;

            foreach (var unit in RBTUnit.ActiveUnitsPerTeam[_selectableTeam])
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

        void TrySelectSingleUnit()
        {
            if (!_selectableTeam) return;

            _selectedUnits.Clear();

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var clickMask = RBTConfig.WalkableMask | _selectable;
            RaycastHit clickHit = new RaycastHit();

            if (Physics.Raycast(ray, out clickHit, 100f, clickMask))
            {
                var clickedObject = clickHit.collider.gameObject;
                var unit = clickedObject.GetComponent<RBTUnit>();
                if (unit && unit.Team == _selectableTeam)
                {
                    _hasSelectedSingle = true;
                    _selectedUnits.Add(unit);
                    unit.Selected = true;
                }
            }
        }

        void DeselectAll()
        {
            foreach (var unit in RBTUnit.ActiveUnits)
                unit.Selected = false;
        }

        // Unity functions
        void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitSelector on '{gameObject.name}' was destroyed as there's already one attached on '{name}'");
                Destroy(this);
                return;
            }
            else Instance = this;

            if (!_material)
                Debug.LogError("Please assign a material in the inspector");

            if (_selectable == 0)
                Debug.LogWarning($"Please assign layers to 'Selectable' on RBTUnitSelector attached to '{name}'");

            if (!_selectableTeam)
                Debug.LogWarning($"Please assign a team to 'Selectable Team' on RBTUnitSelector attached to '{name}'");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _selectedUnits.Clear();
                DeselectAll();
                _selectBoxStartCorner = Input.mousePosition;
                TrySelectSingleUnit();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _onUnitsSelected.Invoke(new OnUnitsSelectedEvent(this, _selectedUnits, _selectableTeam));
                _hasSelectedSingle = false;
            }
        }

        void OnPostRender()
        {
            if (!Input.GetMouseButton(0) || _hasSelectedSingle)
                return;

            var selectBox = new SelectBox(_selectBoxStartCorner, Input.mousePosition);

            if (_material)
                selectBox.Draw(_material);

            TrySelectMultipleUnits(selectBox);
        }
    }
}

