using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SeparationSteering : SteeringBehaviour
{
    // Inspector fields
    [SerializeField] float _weight = 1f;
    [SerializeField] private FieldOfView _fieldOfView;
    [SerializeField] private float _pushiness = 2f;

    // Private data
    private List<UnitSteeringSet> _currentUnitSteeringSets = new List<UnitSteeringSet>();

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
        foreach (var unitSteeringSet in _currentUnitSteeringSets)
        {
            var originalColor = Gizmos.color;
            DrawSeparation(unitSteeringSet);
            Gizmos.color = originalColor;
        }
    }

    // Public functions
    public override Vector3 GetSteeringForce(Unit unit)
    {
        var steering = GetSeperation(unit);
        _currentUnitSteeringSets.Add(new UnitSteeringSet(unit, steering));
        return steering;
    }

    // Protected functions
    protected override void InitializeSteering()
    {
        _currentUnitSteeringSets.Clear();
    }

    // Private functions
    private Vector3 GetSeperation(Unit unit)
    {
        if (_fieldOfView.Radius == 0f)
        {
            return Vector3.zero;
        }

        Vector3 separation = Vector3.zero;

        var neighbourList = _fieldOfView.GetVisibleObjects(unit.gameObject).ToList();
        if (neighbourList.Count > 0)
        {
            foreach (var neighbour in neighbourList)
            {
                var offset = unit.transform.position - neighbour.transform.position;

                var distanceToNeighbour = offset.magnitude;
                var relativeDistanceToNeighbour = Mathf.Max(distanceToNeighbour / _fieldOfView.Radius, 0.01f);

                var multiplier = 1 / Mathf.Pow(relativeDistanceToNeighbour, _pushiness);
                var direction = offset.normalized;

                separation += direction * multiplier;
            }

            separation *= _weight;
        }
        return separation;
    }

    private void DrawSeparation(UnitSteeringSet unitSteeringSet)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(unitSteeringSet.Unit.transform.position, unitSteeringSet.Unit.transform.position + unitSteeringSet.Steering);
    }

    private class UnitSteeringSet
    {
        public Unit Unit { get; private set; }
        public Vector3 Steering { get; private set; }

        public UnitSteeringSet(Unit unit, Vector3 steering)
        {
            Unit = unit;
            Steering = steering;
        }
    }
}
