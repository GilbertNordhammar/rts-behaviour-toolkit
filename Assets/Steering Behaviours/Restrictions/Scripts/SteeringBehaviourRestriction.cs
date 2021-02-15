using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SteeringBehaviourRestriction : ScriptableObject
{
    public abstract bool MayApplyBehaviour(Unit unit);
}
