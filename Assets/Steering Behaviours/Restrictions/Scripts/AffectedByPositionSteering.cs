using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AffectedByPositionSteering", menuName = "Steering Behaviour Restrictions/AffectedByPositionSteering")]
public class AffectedByPositionSteering : SteeringBehaviourRestriction
{
    public override bool MayApplyBehaviour(Unit unit)
    {
        return PositionSteering.Instance.AffectedUnits.Contains(unit);
    }
}
