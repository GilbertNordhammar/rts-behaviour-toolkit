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

        [SerializeField]
        bool _drawGizmos;

        // Private
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
            if (!_drawGizmos) return;

#if UNITY_EDITOR
            foreach (var unit in group.Units)
            {
                var maxDistance = Bounds * GetScale(unit.Unit.Bounds.Extents);
                Handles.DrawWireDisc(unit.Unit.Position, Vector3.up, maxDistance);
            }
#endif
        }
    }
}

