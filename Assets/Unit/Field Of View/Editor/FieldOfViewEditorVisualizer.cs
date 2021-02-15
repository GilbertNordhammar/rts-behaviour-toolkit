using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public abstract class FieldOfViewEditorVisualizer<TComponent> : Editor where TComponent: MonoBehaviour
{
    private FieldOfView _fieldOfView;
    private bool _mayDraw = false;
    private Editor _cachedEditor;

    // Unity event functions
    protected virtual void OnEnable()
    {
        SerializedProperty fieldOfViewProperty = serializedObject.FindProperty("_fieldOfView");
        if (fieldOfViewProperty != null)
        {
            _fieldOfView = fieldOfViewProperty.objectReferenceValue as FieldOfView;
        }
        if (_fieldOfView == null || fieldOfViewProperty == null)
        {
            throw new MissingFieldException("Missing field. The derived class needs to have a non-null field named '_fieldOfView' of type " + typeof(FieldOfView) + ". ");
        }

        try
        {
            _mayDraw = true;
            _cachedEditor = null;
        }
        catch (MissingFieldException e)
        {
            Debug.LogWarning(target.name + ": " + e.Message);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (_cachedEditor == null)
        {
            CreateCachedEditor(_fieldOfView, typeof(FieldOfViewEditor), ref _cachedEditor);
        }

        (_cachedEditor as FieldOfViewEditor).TruncateInspectorFields();
        _cachedEditor.DrawDefaultInspector();
    }

    protected virtual void OnSceneGUI()
    {
        if (_mayDraw && _fieldOfView.VisualizeFieldOfView)
        {
            VisualizeFieldOfView();
        }
    }

    private void VisualizeFieldOfView()
    {
        var enabledGameObjectsInScene = FindObjectsOfType<TComponent>().Select(x => x.gameObject).Where(x => x.activeInHierarchy).ToList();
        
        DrawViewArc(enabledGameObjectsInScene);
        DrawLinesToVisibleObjects(enabledGameObjectsInScene);
    }

    private void DrawViewArc(List<GameObject> gameObjectList)
    {
        Color originalColor = Handles.color;
        Handles.color = Color.gray;

        foreach (var unit in gameObjectList)
        {
            var transform = unit.transform;
            var center = transform.position;
            var normal = transform.up;
            var viewArcStartDirection = Quaternion.AngleAxis(-_fieldOfView.Angle / 2, transform.up) * transform.forward;

            Handles.DrawWireArc(center, normal, viewArcStartDirection, _fieldOfView.Angle, _fieldOfView.Radius);

            var arcSideEndPoint = unit.transform.position + viewArcStartDirection * _fieldOfView.Radius;
            Handles.DrawLine(unit.transform.position, arcSideEndPoint);

            arcSideEndPoint = unit.transform.position + (Quaternion.AngleAxis(_fieldOfView.Angle / 2, transform.up) * transform.forward) * _fieldOfView.Radius;
            Handles.DrawLine(unit.transform.position, arcSideEndPoint);
        }

        Handles.color = originalColor;
    }

    private void DrawLinesToVisibleObjects(List<GameObject> unitList)
    {
        var originalColor = Handles.color;
        Handles.color = Color.magenta;

        foreach (var unit in unitList)
        {
            var visibleUnits = _fieldOfView.GetVisibleObjects(unit);

            foreach (var neighbour in visibleUnits)
            {
                Handles.DrawLine(unit.transform.position, neighbour.transform.position);
            }
        }

        Handles.color = originalColor;
    }
}
