using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleAvoidanceSteering : SteeringBehaviour
{
    [SerializeField] private float _weight = 2f;
    [SerializeField] private float _pushiness = 2f;
    [SerializeField] private FieldOfView _fieldOfView;

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
        var originalColor = Gizmos.color;

        foreach(var unit in Unit.ActiveUnitsInScene)
        {
            DrawSteering(unit);
        }

        Gizmos.color = originalColor;
    }

    // Public functions
    public override Vector3 GetSteeringForce(Unit unit)
    {
        var visibleObstacles = _fieldOfView.GetVisibleObjects(unit.gameObject);

        if (_fieldOfView.Radius == 0f || visibleObstacles.Count == 0)
        {
            return Vector3.zero;
        }

        var closestObstacle = GetClosestObstacleToUnit(visibleObstacles, unit);
        var closestPoint = closestObstacle.GetComponent<Collider>().ClosestPoint(unit.transform.position);
        closestPoint.y = unit.transform.position.y;

        Debug.DrawLine(unit.transform.position, closestPoint, Color.gray);

        var offset = unit.transform.position - closestPoint;
        var distanceToClosestPoint = offset.magnitude;
        var distanceToClosestPointRelative = distanceToClosestPoint / _fieldOfView.Radius;

        var multiplier = 1 / Mathf.Pow(distanceToClosestPointRelative, _pushiness);
        var direction = offset.normalized;
        var steeringForce = direction * multiplier * _weight;

        return steeringForce;
    }

    // Private functions
    private GameObject GetClosestObstacleToUnit(List<GameObject> obstacles, Unit unit)
    {
        var unitPosition = unit.transform.position;
        var closestObstacle = obstacles.Aggregate((i1, i2) => (i1.transform.position - unitPosition).magnitude > (i2.transform.position - unitPosition).magnitude ? i1 : i2);

        return closestObstacle;
    }

    private Vector3 ConvertFromWorldToUnitLocalCoordinates(Vector3 pointToConvert, Unit unit)
    {
        return unit.transform.rotation * unit.transform.worldToLocalMatrix.MultiplyPoint(pointToConvert);
    }

    private bool ToTheRightOfUnit(Vector3 worldCoordinate, Unit unit)
    {
        var localPoint = ConvertFromWorldToUnitLocalCoordinates(worldCoordinate, unit);

        bool toTheRight = false;
        if(localPoint.x > 0)
        {
            toTheRight = true;
        }

        return toTheRight;
    }

    private void DrawSteering(Unit unit)
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(unit.transform.position, unit.transform.position + GetSteeringForce(unit));
    }
}
