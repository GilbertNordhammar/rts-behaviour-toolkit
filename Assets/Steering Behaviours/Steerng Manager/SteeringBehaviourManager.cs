using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class SteeringBehaviourManager : MonoBehaviour
{
    // Editor fields
    [SerializeField] float _forceMultiplier = 1;
    [SerializeField] float _maximumVelocityMagnitude = 1f;
    [SerializeField] private List<BehaviourWithRestriction> _behavioursToExecute;

    // Public data
    public List<BehaviourWithRestriction> BehavioursToExecute { get { return _behavioursToExecute; } }

    // Unity event functions
    private void FixedUpdate()
    {
        ExecuteSteeringBehaviours();
        ClampUnitVelocities();
    }

    // Private functions
    private void ExecuteSteeringBehaviours()
    {
        var unitSample = Unit.ActiveUnitsInScene.FirstOrDefault();
        var prevVelocity = unitSample.Velocity;

        var firstUnit = Unit.ActiveUnitsInScene.FirstOrDefault();
        var netForceBefore = firstUnit.NetForceOnUnit;
        var velocityBefore = firstUnit.Velocity;
        foreach (var behaviour in _behavioursToExecute)
        {
            behaviour.ExecuteBehaviour(_forceMultiplier);
        }
    }

    private void ClampUnitVelocities()
    {
        foreach (var unit in Unit.ActiveUnitsInScene)
        {
            unit.Velocity = Vector3.ClampMagnitude(unit.Velocity, _maximumVelocityMagnitude);
        }
    }

    // Internal classes
    [System.Serializable]
    public class BehaviourWithRestriction
    {
        public SteeringBehaviour Behaviour;
        public SteeringBehaviourRestriction Restriction;

        public void ExecuteBehaviour(float forceMultiplier)
        {
            if (Restriction != null)
            {
                Behaviour.Execute(forceMultiplier, Restriction);
            }
            else
            {
                Behaviour.Execute(forceMultiplier);
            }
        }
    }
}
