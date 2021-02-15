using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelecter : MonoBehaviour {

    // Inspector fields
    [SerializeField] private GUIStyle _boxStyle;

    // Private data
    private bool _userIsDragging;
    private Vector2 _mouseDownPoint, _mouseCurrentPoint;
    private List<Unit> _unitsOnScreen = new List<Unit>();
    private Vector2 _boxStartCorner, _boxEndCorner, _boxSize, _boxStartEndDelta, _boxCenter;
    private bool _mayUseSelectBox;
    private LayerMask _unitMask;

    // Public data
    public static List<Unit> SelectedUnits { get; private set; }
    public static UnitSelecter Instance { get; private set; }

    // Unity event functions
    private void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        SelectedUnits = new List<Unit>();
        _unitMask = LayerMask.GetMask("Unit");
    }

    private void OnEnable()
    {
        Unit.OnEnterScreen += AddToOnScreenList;
        Unit.OnExitScreen += RemoveFromOnScreenList;
    }

    private void OnDisable()
    {
        Unit.OnEnterScreen -= AddToOnScreenList;
        Unit.OnExitScreen -= RemoveFromOnScreenList;
    }

    private void OnGUI()
    {
        if (!_userIsDragging || !_mayUseSelectBox)
        {
            return;
        }

        DrawSelectBox();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mouseDownPoint = Input.mousePosition;
            _userIsDragging = true;

            var unit = GetClickedOnUnit();

            if (unit != null)
            {
                SelectSingleUnit(unit);
                _mayUseSelectBox = false;
                HighlightSelectedUnits();
            }
            else
            {
                _mayUseSelectBox = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _userIsDragging = false;
            if (_mayUseSelectBox)
            {
                SelectUnitsInSelectBox();
            }
        }

        if (_mayUseSelectBox)
        {
            CalculateSelectBox();
        }
    }

    Unit GetClickedOnUnit()
    {
        Unit unit = null;

        RaycastHit hitInfo;
        Ray selectRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(selectRay, out hitInfo, _unitMask);

        if(hitInfo.collider != null)
        {
            unit = hitInfo.collider.GetComponentInParent<Unit>();
        }

        return unit;
    }

    private void SelectSingleUnit(Unit unit)
    {
        ClearSelectedUnits();
        SelectedUnits.Add(unit);
        HighlightSelectedUnits();
    }

    private void SelectUnitsInSelectBox()
    {
        ClearSelectedUnits();
        foreach (var unit in _unitsOnScreen)
        {
            if (WithinSelectBox(unit))
            {
                SelectedUnits.Add(unit);
                TurnOnHighlight(unit);
            }
        }
    }

    private void DrawSelectBox()
    {
        GUI.Box(new Rect(_boxStartCorner, _boxStartEndDelta), "", _boxStyle);
    }

    private void AddToOnScreenList(Unit unit)
    {
        _unitsOnScreen.Add(unit);
    }

    private void RemoveFromOnScreenList(Unit unit)
    {
        _unitsOnScreen.Remove(unit);
    }

    private void HighlightSelectedUnits()
    {
        foreach (var unit in SelectedUnits)
        {
            TurnOnHighlight(unit);
        }
    }

    private void ClearSelectedUnits()
    {
        SelectedUnits.Clear();
        ClearHighlights();
    }

    private void ClearHighlights()
    {
        foreach (var unit in _unitsOnScreen)
        {
            TurnOffHighlight(unit);
        }
    }

    private void TurnOnHighlight(Unit unit)
    {
        unit.SelectHighlight.SetActive(true);
    }

    private void TurnOffHighlight(Unit unit)
    {
        unit.SelectHighlight.SetActive(false);
    }

    private void CalculateSelectBox()
    {
        _boxStartCorner = new Vector2(_mouseDownPoint.x, Screen.height - _mouseDownPoint.y);
        _boxEndCorner = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        _boxStartEndDelta = new Vector2(_boxEndCorner.x - _boxStartCorner.x, _boxEndCorner.y - _boxStartCorner.y);
        _boxSize = new Vector2(Mathf.Abs(_boxStartEndDelta.x), Mathf.Abs(_boxStartEndDelta.y));
        _boxCenter = (_boxStartCorner + _boxEndCorner) / 2;
    }

    private bool WithinSelectBox(Unit unit)
    {
        bool withinSelectBox = false;

        float halfWidth = _boxSize.x / 2;
        float halfHeight = _boxSize.y / 2;

        Vector2 unitPosition = new Vector2(unit.OnScreenPosition.x, Screen.height - unit.OnScreenPosition.y);

        if (unitPosition.x > _boxCenter.x - halfWidth && unitPosition.x < _boxCenter.x + halfWidth &&
           unitPosition.y > _boxCenter.y - halfHeight && unitPosition.y < _boxCenter.y + halfHeight)
        {
            withinSelectBox = true;
        }

        return withinSelectBox;
    }
}

