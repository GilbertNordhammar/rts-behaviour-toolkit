using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    SerializedProperty _angle, _radius;

    // Unity event functions
    private void OnEnable()
    {
        _angle = serializedObject.FindProperty("_angle");
        _radius = serializedObject.FindProperty("_radius");

        TruncateInspectorFields();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TruncateInspectorFields();
    }

    // Public functions
    public void TruncateInspectorFields()
    {
        serializedObject.Update();
        _angle.floatValue = Mathf.Max(_angle.floatValue, 0f);
        _radius.floatValue = Mathf.Max(_radius.floatValue, 0f);
        serializedObject.ApplyModifiedProperties();
    }
}
