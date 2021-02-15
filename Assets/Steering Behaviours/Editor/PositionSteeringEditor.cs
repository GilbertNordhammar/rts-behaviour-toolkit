using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using MovingUnit = PositionSteering.MovingUnit;

[CustomEditor(typeof(PositionSteering))]
public class PositionSteeringEditor : Editor
{
    SerializedProperty _movingUnits;

    private void Awake()
    {
        _movingUnits = serializedObject.FindProperty("_movingUnits");
    }

    public void OnSceneGUI()
    {
        serializedObject.Update();

        if(_movingUnits == null)
        {
            return;
        }

        var originalColor = Handles.color;
        Handles.color = Color.gray;

        for(int i = 0; i < _movingUnits.arraySize; i++)
        {
            var movingCheckPoints = _movingUnits.GetArrayElementAtIndex(i).FindPropertyRelative("_movingPathCheckpoints");
            var positionOffset = _movingUnits.GetArrayElementAtIndex(i).FindPropertyRelative("TargetPositionOffsetRadius").floatValue;
            DrawPathWithCheckPointOffset(movingCheckPoints, positionOffset);
        }

        Handles.color = originalColor;
    }

    private void DrawPathWithCheckPointOffset(SerializedProperty checkPointsInPath, float offset)
    {
        for(int i = 0; i < checkPointsInPath.arraySize - 1; i++)
        {
            var checkPoint = checkPointsInPath.GetArrayElementAtIndex(i).vector3Value;
            var checkPoint2 = checkPointsInPath.GetArrayElementAtIndex(i + 1).vector3Value;
            Handles.DrawWireDisc(checkPoint, Vector3.up, offset);
            Handles.DrawLine(checkPoint, checkPoint2);
        }

        serializedObject.Update();
        var lastCheckPointIndex = Mathf.Max(checkPointsInPath.arraySize - 1, 0);
        var lastCheckPoint = checkPointsInPath.GetArrayElementAtIndex(lastCheckPointIndex).vector3Value;
        Handles.DrawWireDisc(lastCheckPoint, Vector3.up, offset);
    }
}
