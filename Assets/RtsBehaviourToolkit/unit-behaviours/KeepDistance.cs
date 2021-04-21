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

        [field: SerializeField]
        public float Tilt { get; private set; } = 0.5f;

        [SerializeField]
        FieldOfView _fov;

        [SerializeField]
        bool _drawGizmos;

        // Private
        float _fovDotProdThreshold;

        float CalcRepulsion(float distance, float maxDistance)
        {
            var numerator = Tilt * (1 - distance / maxDistance);
            var denominator = distance + Tilt;
            return Weight * Mathf.Max(0, numerator / denominator);
        }

        // Public
        public override void OnUpdate(CommandGroup group)
        {
            GameObject target = null;
            if (group is FollowGroup)
                target = (group as FollowGroup).Target;
            else if (group is AttackGroup)
                target = (group as AttackGroup).Target.GameObject;

            var commanderData = group.GetCustomData<CommonGroupData>();

            foreach (var unit in group.Units)
            {
                var unitsInFov = _fov.GetUnitsWithinFov(unit.Unit);
                var maxDistance = _fov.GetMaxDistance(unit.Unit);
                foreach (var nu in unitsInFov)
                {
                    if (!nu.Alive || nu.Team != unit.Unit.Team) continue;

                    var offset = nu.transform.position - unit.Unit.Position;
                    var distance = offset.magnitude;
                    var movement = CalcRepulsion(distance, maxDistance) * offset.normalized;

                    if (target)
                    {
                        var commander = commanderData.Commander;
                        if (target == nu.GameObject && unit != commander)
                            unit.Unit.AddMovement(-movement);
                        else if (nu != commander.Unit || (nu == commander.Unit && commander.Unit.State.HasFlag(RBTUnit.UnitState.Idling)))
                            nu.AddMovement(movement);
                    }
                    else
                        nu.AddMovement(movement);
                }
            }
        }

        public override void DrawGizmos(CommandGroup group)
        {
            if (_drawGizmos)
                _fov.DrawGizmos(group);
        }

        // Unity functions

        void Awake()
        {
            _fov.Init();
        }

        void OnValidate()
        {
            _fov.Init();
        }
    }
}

