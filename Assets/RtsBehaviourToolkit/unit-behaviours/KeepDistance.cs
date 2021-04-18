using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "KeepDistance", menuName = "RtsBehaviourToolkit/Behaviours/KeepDistance")]
    [System.Serializable]
    public class KeepDistance : UnitBehaviour
    {
        // Inspector
        [field: SerializeField, Min(0)]
        public float Weight { get; private set; } = 1;

        [field: SerializeField, Range(0, 5), Tooltip("Percentage of largest dimension of Unit.Bounds.Bounds")]
        public float Bounds { get; private set; } = 1f;

        [field: SerializeField]
        public float Tilt { get; private set; } = 0.5f;

        [field: SerializeField, Min(1f)]
        public float Fov { get; private set; }

        [SerializeField]
        bool _drawGizmos;

        // Private
        float _fovDotProdThreshold;

        float GetScale(Vector3 extents)
        {
            return extents.x > extents.z ? extents.x : extents.z;
        }

        float CalcRepulsion(float distance, float maxDistance)
        {
            var numerator = Tilt * (1 - distance / maxDistance);
            var denominator = distance + Tilt;
            return Weight * Mathf.Max(0, numerator / denominator);
        }

        bool WithinFov(RBTUnit source, RBTUnit target)
        {
            var dirSourceToTarget = (target.Position - source.Position).normalized;
            var sourceVelocityDir = source.Velocity.normalized;

            var dotProd = Vector3.Dot(dirSourceToTarget, sourceVelocityDir);
            return dotProd > _fovDotProdThreshold;
        }

        // Unity functions
        void Awake()
        {
            _fovDotProdThreshold = Mathf.Cos(Mathf.Deg2Rad * Fov);
        }

        void OnValidate()
        {
            _fovDotProdThreshold = Mathf.Cos(Mathf.Deg2Rad * Fov);
        }

        // Public
        public override void OnUpdate(CommandGroup group)
        {
            var followGroup = group as FollowGroup;
            var commanderData = group.GetCustomData<CommonGroupData>();

            var grid = RBTUnitBehaviourManager.UnitGrid;
            var cathetus = Mathf.Sqrt(Mathf.Pow(Bounds, 2) / 2);
            foreach (var unit in group.Units)
            {
                var scale = GetScale(unit.Unit.Bounds.Extents);
                var scaledCathetus = cathetus * scale;
                var nearbyUnits = grid.FindNear(unit.Unit.Position, new Vector3(scaledCathetus, 0, scaledCathetus));
                var maxDistance = Bounds * scale;
                foreach (var nu in nearbyUnits)
                {
                    if (!WithinFov(unit.Unit, nu)) continue;

                    var offset = nu.transform.position - unit.Unit.Position;
                    var distance = offset.magnitude;
                    var movement = CalcRepulsion(distance, maxDistance) * offset.normalized;

                    if (followGroup)
                    {
                        var commander = commanderData.Commander;
                        if (followGroup.Target == nu.GameObject && unit != commander)
                            unit.Unit.AddMovement(-movement);
                        else if (nu != commander.Unit || (nu == commander.Unit && commander.Unit.State.HasFlag(RBTUnit.ActionState.Idling)))
                            nu.AddMovement(movement);
                    }
                    else
                        nu.AddMovement(movement);
                }
            }
        }

        public override void DrawGizmos(CommandGroup group)
        {
#if UNITY_EDITOR
            if (!_drawGizmos) return;
            var origColor = Handles.color;
            Handles.color = new Color(1, 1, 1, 0.2f);

            foreach (var unit in group.Units)
            {
                var maxDistance = Bounds * GetScale(unit.Unit.Bounds.Extents);
                // Handles.DrawWireDisc(unit.Unit.Position, Vector3.up, maxDistance);

                var viewArcStartDirection = Quaternion.AngleAxis(-Fov / 2, Vector3.up) * unit.Unit.Velocity.normalized;
                Handles.DrawSolidArc(unit.Unit.Position, Vector3.up, viewArcStartDirection, Fov, maxDistance);
            }
            Handles.color = origColor;
#endif  
        }
    }
}

