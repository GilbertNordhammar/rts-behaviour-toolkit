using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "StopIfTooClose", menuName = "RtsBehaviourToolkit/Behaviours/StopIfTooClose")]
    [System.Serializable]
    public class StopIfTooClose : UnitBehaviour
    {
        [field: SerializeField, Range(0, 5), Tooltip("Percentage of largest dimension of Unit.Bounds.Bounds")]
        public float Bounds { get; private set; } = 1f;

        float GetScale(Vector3 extents)
        {
            return extents.x > extents.z ? extents.x : extents.z;
        }

        public override void OnCommandGroupCreated(CommandGroup group)
        {
            var grid = RBTUnitBehaviourManager.UnitGrid;
            var cathetus = Mathf.Sqrt(Mathf.Pow(Bounds, 2) / 2);

            foreach (var unit in group.Units)
            {
                var scale = GetScale(unit.Unit.Bounds.Extents);

                var scaledCathetus = cathetus * scale;
                var nearbyUnits = grid.FindNear(unit.Unit.Position, new Vector3(scaledCathetus, 0, scaledCathetus));
            }
        }
    }
}

