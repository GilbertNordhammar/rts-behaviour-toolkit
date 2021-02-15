using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AlignmentSteering : SteeringBehaviour
{
    // Inspector fields
    [SerializeField] private float _weight = 1f;
    [SerializeField] private FieldOfView _fieldOfView;

    // public data
    public FieldOfView FieldOfView { get { return _fieldOfView; } }

    public override List<Unit> AffectedUnits
    {
        get
        {
            return Unit.ActiveUnitsInScene;
        }
    }

    // Unity event functions
    private void OnDrawGizmosSelected()
    {
        DrawAlignment();
    }

    private void OnValidate()
    {
        if(_weight < 0f)
        {
            _weight = 0f;
        }
        else if(_fieldOfView.Radius < 0f)
        {
            _fieldOfView.Radius = 0f;
        }
    }

    // Public functions
    public override Vector3 GetSteeringForce(Unit unit)
    {
        return GetAlignment(unit);
    }

    // Private functions
    private void DrawAlignment()
    {
        Color orignalColor = Gizmos.color;
        Gizmos.color = Color.blue;

        foreach (var unit in Unit.ActiveUnitsInScene)
        {
            Gizmos.DrawLine(unit.transform.position, unit.transform.position + GetAlignment(unit));
        }

        Gizmos.color = orignalColor;
    }

    private Vector3 GetAlignment(Unit unit)
    {
        Vector3 alignment = Vector3.zero;

        var neighbourList = _fieldOfView.GetVisibleObjects(unit.gameObject).Select(x => x.transform.root.GetComponentInChildren<Unit>()).ToList();

        if (neighbourList.Count > 0)
        {
            var averageVelocity = Vector3.zero;
            foreach (var neighbour in neighbourList)
            {
                averageVelocity += neighbour.Velocity;
            }
            averageVelocity /= neighbourList.Count;

            alignment = averageVelocity.normalized*_weight;
        }

        return alignment;
    }
}
