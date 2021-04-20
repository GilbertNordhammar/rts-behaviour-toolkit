using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "GoTowardsEnemy", menuName = "RtsBehaviourToolkit/Behaviours/GoTowardsEnemy")]
    [System.Serializable]
    public class GoTowardsEnemy : UnitBehaviour
    {
        // Inspector and public
        [SerializeField]
        [Min(0f)]
        int _weight = 1;

        [SerializeField]
        [Min(0f)]
        float _minPursueDistance = 1;

        public override void OnUpdate(CommandGroup group)
        {
            if (group is AttackGroup)
            {
                var attackGroup = group as AttackGroup;
                foreach (var unit in attackGroup.Units)
                {
                    var offs = attackGroup.Target.Position - unit.Unit.Position;
                    var dist = offs.magnitude;

                    var minSqrDist = unit.Unit.Attack.Range + _minPursueDistance;
                    minSqrDist *= minSqrDist;
                    if (dist < _minSqrPursueDist && dist > unit.Unit.Attack.Range)
                        unit.Unit.AddMovement(offs.normalized * _weight);
                }
            }
        }

        // Private
        float _minSqrPursueDist;

        // Unity functions
        void Awake()
        {
            _minSqrPursueDist = _minPursueDistance * _minPursueDistance;
        }

        void OnValidate()
        {
            _minSqrPursueDist = _minPursueDistance * _minPursueDistance;
        }
    }
}

