using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VelocityRestriction", menuName = "Steering Behaviour Restrictions/VelocityRestriction")]
public class VelocityRestriction : SteeringBehaviourRestriction
{
    [SerializeField] [Range(0, 100)] private float _minimumVelcotiyMagnitude;

    public float MinimumVelcotiyMagnitude { get { return _minimumVelcotiyMagnitude; } }

    public override bool MayApplyBehaviour(Unit unit)
    {
        return unit.Velocity.magnitude > _minimumVelcotiyMagnitude;
    }
}
