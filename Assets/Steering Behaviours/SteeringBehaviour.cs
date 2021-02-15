using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SteeringBehaviour : MonoBehaviour
{
    // Public data
    public readonly static List<SteeringBehaviour> ActiveSteeringBehavioursInScene = new List<SteeringBehaviour>();

    // Unity event functions
    private void OnEnable()
    {
        ActiveSteeringBehavioursInScene.Add(this);
    }

    private void OnDisable()
    {
        
        ActiveSteeringBehavioursInScene.Remove(this);
    }

    // Public functions
    public void Execute(float forceMultiplier)
    {
        InitializeSteering();
        foreach (var unit in AffectedUnits)
        {
            var steeringForce = GetSteeringForce(unit);
            unit.AddForce(steeringForce * forceMultiplier);
        }
    }

    public void Execute(float forceMultiplier, SteeringBehaviourRestriction restriction)
    {
        InitializeSteering();
        foreach(var unit in AffectedUnits)
        {
            var steeringForce = GetSteeringForce(unit);
            if (restriction.MayApplyBehaviour(unit))
            {
                unit.AddForce(steeringForce * forceMultiplier);
            }
        }
    }

    public abstract Vector3 GetSteeringForce(Unit unit);
    public abstract List<Unit> AffectedUnits { get; }

    public void EnableBehaviour()
    {
        enabled = true;
    }

    public void DisableBehaviour()
    {
        enabled = false;
    }

    // Protected functions
    protected virtual void InitializeSteering() { }
}
