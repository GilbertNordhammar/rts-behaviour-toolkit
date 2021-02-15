using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CohesionSteering : SteeringBehaviour
{
    [SerializeField] float _weight = 1f;
    [SerializeField] FieldOfView _fieldOfView;

    // Public data
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
        DrawCohesion();
    }

    public override Vector3 GetSteeringForce(Unit unit)
    {
        return GetCohesion(unit);
    }

    // Private functions
    private Vector3 GetCohesion(Unit unit)
    {
        Vector3 cohesion = Vector3.zero;

        var neighbourList = _fieldOfView.GetVisibleObjects(unit.gameObject).Select(x => x.transform.root.gameObject).ToList();

        if (neighbourList.Count > 0)
        {
            var averagePosition = Vector3.zero;
            foreach (var neighbour in neighbourList)
            {
                averagePosition += neighbour.transform.position;
            }
            averagePosition /= neighbourList.Count;

            var positionOffset = averagePosition - unit.transform.position;

            cohesion = positionOffset.normalized * positionOffset.magnitude * _weight;
        }

        return cohesion;
    }

    private void DrawCohesion()
    {
        Gizmos.color = Color.cyan;
        foreach (var unit in Unit.ActiveUnitsInScene)
        {
            Gizmos.DrawLine(unit.transform.position, unit.transform.position + GetCohesion(unit));
        }
    }
}
