using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RtsBehaviourToolkit;

public class PositionDisplayer : MonoBehaviour
{
    // Unity editor
    [SerializeField]
    GameObject _positionMarker;

    [SerializeField]
    [Min(0.1f)]
    float _markerDuration = 1.0f;

    [SerializeField]
    RBTUnitCommander _unitCommander;

    // Private
    GameObject _activePositionMarker;
    IEnumerator _setMarker;

    IEnumerator SetMarker(Vector3 position)
    {
        if (!_activePositionMarker)
            yield return null;

        _activePositionMarker.transform.position = position;
        _activePositionMarker.SetActive(true);
        yield return new WaitForSecondsRealtime(_markerDuration);
        _activePositionMarker.SetActive(false);
    }

    void HandleCommandGiven(RBTUnitCommander.CommandGivenEvent evnt)
    {
        if (_setMarker != null)
            StopCoroutine(_setMarker);
        _setMarker = SetMarker(evnt.Position);
        StartCoroutine(_setMarker);
    }

    void OnEnable()
    {
        if (_unitCommander)
            _unitCommander.OnCommandGiven += HandleCommandGiven;
    }

    void OnDisable()
    {
        if (_unitCommander)
            _unitCommander.OnCommandGiven -= HandleCommandGiven;
    }

    void Awake()
    {
        if (_positionMarker)
        {
            _activePositionMarker = Instantiate(_positionMarker);
            _activePositionMarker.transform.rotation = _positionMarker.transform.rotation;
            _activePositionMarker.SetActive(false);
        }
        else
            Debug.LogWarning($"Please assign a position marker to PositionDisplayer on '{gameObject.name}'");
    }
}
