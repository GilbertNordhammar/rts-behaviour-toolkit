using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public class RBTFollowPath : RBTUnitBehaviour
    {
        public override void Execute(CommandGroup group)
        {
            foreach (var unit in group.CommandUnits)
            {
                var direction = unit.OffsetToNextCorner.normalized;
                unit.Unit.AddMovement(direction);
            }
        }
    }
}
