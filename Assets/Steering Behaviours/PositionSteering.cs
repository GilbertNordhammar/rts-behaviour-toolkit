using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//public class PositionSteering : SteeringBehaviour
//{
//    // Inspector fields
//    [SerializeField] private float _weight = 1f;
//    [SerializeField] private float _positionOffset = 0f;

//    // Public data
//    public static PositionSteering Instance { get; private set; }
//    public override List<Unit> AffectedUnits
//    {
//        get
//        {
//            return _movingUnits.Select(x => x.Unit).ToList();
//        }
//    }

//    // Private data
//    [SerializeField] [HideInInspector] private List<MovingUnit> _movingUnits = new List<MovingUnit>();

//    // Unity event functions
//    private void Start()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(this);
//        }
//    }

//    private void OnDrawGizmosSelected()
//    {
//        foreach (var movingUnit in _movingUnits)
//        {
//            DrawSteering(movingUnit);
//        }
//    }

//    // Public functions
//    protected override void InitializeSteering()
//    {
//        var markedForDelete = new List<MovingUnit>();
//        foreach (var movingUnit in _movingUnits)
//        {
//            if (movingUnit.InPosition())
//            {
//                markedForDelete.Add(movingUnit);
//            }
//        }

//        foreach (var deleted in markedForDelete)
//        {
//            _movingUnits.Remove(deleted);
//        }
//    }

//    public override Vector3 GetSteeringForce(Unit unit)
//    {
//        var movingUnit = _movingUnits.Where(x => x.Unit == unit).FirstOrDefault();
//        return GetSteering(movingUnit);
//    }

//    public void MoveUnitsTowardsPosition(List<Unit> units, Vector3 targetPosition)
//    {
//        var movingUnit = _movingUnits.Where(x => x.Unit == unit).FirstOrDefault();

//        if (movingUnit != null)
//        {
//            movingUnit.TargetPosition = targetPosition;
//        }
//        else
//        {
//            _movingUnits.Add(new MovingUnit(unit, targetPosition, _positionOffset));
//        }
//    }

//    private Vector3 GetSteering(MovingUnit movingUnit)
//    {
//        var positionOffset = movingUnit.TargetPosition - movingUnit.Unit.transform.position;
//        return positionOffset.normalized * positionOffset.magnitude * _weight;
//    }

//    // Private functions
//    private void DrawSteering(MovingUnit movingUnit)
//    {
//        var originalColor = Gizmos.color;
//        Gizmos.color = Color.red;

//        Gizmos.DrawLine(movingUnit.Unit.transform.position, movingUnit.Unit.transform.position + GetSteering(movingUnit));

//        Gizmos.color = originalColor;
//    }

//    // Internal classes
//    [System.Serializable]
//    public class MovingUnitGroup
//    {
//        public List<Unit> Units { get; private set; }
//        public Vector3 TargetPosition { get; private set; }
//        public float PositionOffset { get; private set; }

//        public MovingUnitGroup(List<Unit> units, Vector3 targetPosition)
//        {
//            Units = new List<Unit>();
//        }
//    }

//    [System.Serializable]
//    public class MovingUnit
//    {
//        public Unit Unit { get; private set; }
//        public Vector3 TargetPosition;
//        public float TargetPositionOffsetRadius { get; private set; }

//        public MovingUnit(Unit unit, Vector3 targetPosition, float targetPositionOffsetRadius)
//        {
//            Unit = unit;
//            TargetPosition = targetPosition;
//            TargetPositionOffsetRadius = targetPositionOffsetRadius;
//        }

//        public bool InPosition()
//        {
//            bool inPosition = false;

//            var distanceToTarget = Vector3.Distance(Unit.transform.position, TargetPosition);
//            if (distanceToTarget < TargetPositionOffsetRadius)
//            {
//                inPosition = true;
//            }

//            return inPosition;
//        }
//    }
//}

public class PositionSteering : SteeringBehaviour
{
    // Inspector fields
    [SerializeField] private float _weight = 1f;

    // Public data
    public static PositionSteering Instance { get; private set; }
    public override List<Unit> AffectedUnits
    {
        get
        {
            return _movingUnits.Select(x => x.Unit).ToList();
        }
    }

    // Private data
    [SerializeField] [HideInInspector] private List<MovingUnit> _movingUnits = new List<MovingUnit>();

    // Unity event functions
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var movingUnit in _movingUnits)
        {
            DrawSteering(movingUnit);
        }
    }

    // Public functions
    protected override void InitializeSteering()
    {
        var markedForDelete = new List<MovingUnit>();
        foreach (var movingUnit in _movingUnits)
        {
            if (movingUnit.AtTargetPosition())
            {
                markedForDelete.Add(movingUnit);
            }
        }

        foreach (var deleted in markedForDelete)
        {
            _movingUnits.Remove(deleted);
        }
    }

    public override Vector3 GetSteeringForce(Unit unit)
    {
        var movingUnit = _movingUnits.Where(x => x.Unit == unit).FirstOrDefault();
        var positionOffset = movingUnit.NextPosition - movingUnit.Unit.transform.position;
        var steeringForce = positionOffset.normalized * _weight;
        return steeringForce;
    }

    public void MoveUnitAlongPath(Unit unit, NavMeshPath path, float positionOffsetDistance)
    {
        var alreadyMovingUnit = _movingUnits.Where(x => x.Unit == unit).FirstOrDefault();

        if (alreadyMovingUnit != null)
        {
            alreadyMovingUnit.SetPath(path);
        }
        else
        {
            _movingUnits.Add(new MovingUnit(unit, path, positionOffsetDistance));
        }

    }

    // Private functions
    private void DrawSteering(MovingUnit movingUnit)
    {
        var originalColor = Gizmos.color;
        Gizmos.color = Color.red;

        Gizmos.DrawLine(movingUnit.Unit.transform.position, movingUnit.Unit.transform.position + GetSteeringForce(movingUnit.Unit));

        Gizmos.color = originalColor;
    }

    // Internal classes
    [System.Serializable]
    public class MovingUnit
    {
        public Unit Unit { get; private set; }
        public float TargetPositionOffsetRadius;
        public Vector3 NextPosition { get; private set; }

        [SerializeField] private List<Vector3> _movingPathCheckpoints = new List<Vector3>();

        public MovingUnit(Unit unit, NavMeshPath path, float targetPositionOffsetRadius)
        {
            Unit = unit;
            TargetPositionOffsetRadius = targetPositionOffsetRadius;
            SetPath(path);
        }

        public void SetPath(NavMeshPath path)
        {
            _movingPathCheckpoints.Clear();
            foreach(var corner in path.corners)
            {
                _movingPathCheckpoints.Add(new Vector3(corner.x, Unit.transform.position.y, corner.z));
            }

            NextPosition = _movingPathCheckpoints.FirstOrDefault();
        }

        public bool AtTargetPosition()
        {
            bool atTargetPosition = false;

            if (AtPosition(NextPosition))
            {
                _movingPathCheckpoints.Remove(NextPosition);
                NextPosition = _movingPathCheckpoints.FirstOrDefault();
            }

            if (_movingPathCheckpoints.Count == 0)
            {
                atTargetPosition = true;
            }

            return atTargetPosition;
        }

        private bool AtPosition(Vector3 position)
        {
            bool AtPosition = false;

            var distanceToTarget = Vector3.Distance(Unit.transform.position, position);
            if (distanceToTarget < TargetPositionOffsetRadius)
            {
                AtPosition = true;
            }

            return AtPosition;
        }
    }
}
