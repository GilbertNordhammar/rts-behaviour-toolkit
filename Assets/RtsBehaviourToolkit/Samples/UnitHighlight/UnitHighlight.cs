using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RtsBehaviourToolkit;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class UnitHighlight : MonoBehaviour
{
    // Unity editor
    [SerializeField]
    RBTUnit _unit;
    MeshRenderer _highlightRenderer;

    // Private
    void HandleUnitSelected(RBTUnit.UnitEvent evnt)
    {
        EnableHighlight(true);
    }

    void HandleUnitDeselected(RBTUnit.UnitEvent evnt)
    {
        EnableHighlight(false);
    }

    void EnableHighlight(bool enabled)
    {
        _highlightRenderer.enabled = enabled;
    }

    // Unity functions
    void Awake()
    {
        if (!_unit)
            Debug.Log("Please assign a RBTUnit in the editor");
        _highlightRenderer = GetComponent<MeshRenderer>();
        EnableHighlight(false);
    }

    void OnEnable()
    {
        if (_unit)
        {
            _unit.OnSelected += HandleUnitSelected;
            _unit.OnDeselected += HandleUnitDeselected;
        }

    }

    void OnDisable()
    {
        if (_unit)
        {
            _unit.OnSelected -= HandleUnitSelected;
            _unit.OnDeselected -= HandleUnitDeselected;
        }
    }
}
