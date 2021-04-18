using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RtsBehaviourToolkit
{
    [System.Serializable]
    public class FieldOfView
    {
        // Inspector
        [field: SerializeField, Range(0, 5), Tooltip("Percentage of largest dimension of Unit.Bounds.Bounds")]
        public float Reach { get; private set; } = 1f;

        [field: SerializeField, Min(1f)]
        public float Fov { get; private set; }

        // Public
        public float GetMaxDistance(RBTUnit unit)
        {
            var scale = GetLargestDimension(unit.Bounds.Extents);
            return Reach * scale;
        }

        public void Init()
        {
            var cathetus = Mathf.Sqrt(Mathf.Pow(Reach, 2) / 2);
            _baseSearchBounds = new Vector3(cathetus, 0, cathetus);
            _minFovDotProd = Mathf.Cos(Mathf.Deg2Rad * Fov);
        }

        public HashSet<RBTUnit> FindNear(RBTUnit unit)
        {
            var grid = RBTUnitBehaviourManager.UnitGrid;
            var scale = GetLargestDimension(unit.Bounds.Extents);
            var scaledSearchBounds = _baseSearchBounds * scale;
            return grid.FindNear(unit.Position, scaledSearchBounds);
        }

        public List<RBTUnit> GetUnitsWithinFov(RBTUnit unit)
        {
            var nearby = FindNear(unit);
            var nearbyWithinFov = new List<RBTUnit>();
            nearbyWithinFov.Capacity = nearbyWithinFov.Count;
            foreach (var nu in nearby)
            {
                if (WithinFov(unit, nu))
                    nearbyWithinFov.Add(nu);
            }
            return nearbyWithinFov;
        }

        public bool WithinFov(RBTUnit source, RBTUnit target)
        {
            var dirSourceToTarget = (target.Position - source.Position).normalized;
            var sourceVelocityDir = source.Velocity.normalized;

            var dotProd = Vector3.Dot(dirSourceToTarget, sourceVelocityDir);
            return dotProd > _minFovDotProd;
        }

        public void DrawGizmos(CommandGroup group)
        {
#if UNITY_EDITOR
            var origColor = Handles.color;
            Handles.color = new Color(1, 1, 1, 0.2f);

            foreach (var unit in group.Units)
            {
                var maxDistance = GetMaxDistance(unit.Unit);

                var viewArcStartDirection = Quaternion.AngleAxis(-Fov / 2, Vector3.up) * unit.Unit.Velocity.normalized;
                Handles.DrawSolidArc(unit.Unit.Position, Vector3.up, viewArcStartDirection, Fov, maxDistance);
            }
            Handles.color = origColor;
#endif  
        }

        // Private
        Vector3 _baseSearchBounds;
        float _minFovDotProd;

        float GetLargestDimension(Vector3 extents)
        {
            return extents.x > extents.z ? extents.x : extents.z;
        }
    }
}

