using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RtsBehaviourToolkit;

public class PositionDisplayer : MonoBehaviour
{
    // Unity editor
    [SerializeField]
    GameObject _positionMarker;

    // private
    void HandleCommandGiven(RBTUnitCommander.CommandGivenEvent evnt)
    {
        if (_positionMarker)
        {
            Debug.Log("pos (post): " + evnt.Position);
            var marker = Instantiate(_positionMarker, evnt.Position, _positionMarker.transform.rotation);
            marker.transform.position = evnt.Position;
        }
    }

    // Unity functions
    void OnEnable()
    {
        if (RBTUnitCommander.Instance)
            RBTUnitCommander.Instance.OnCommandGiven += HandleCommandGiven;
    }

    void OnDisable()
    {
        if (RBTUnitCommander.Instance)
            RBTUnitCommander.Instance.OnCommandGiven -= HandleCommandGiven;
    }

    void Awake()
    {
        if (!_positionMarker)
            Debug.LogWarning($"Please assign a position marker to PositionDisplayer on '{gameObject.name}'");
    }
}
