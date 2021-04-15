using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace RtsBehaviourToolkit
{
    [RequireComponent(typeof(NavMesh))]
    public sealed class RBTUnitBehaviourManager : UnitBehaviourManagerBase
    {
        // Inspector
        [SerializeField]
        [Min(0f)]
        float _subgroupDistance = 2f;

        // Public
        public static RBTUnitBehaviourManager Instance { get; private set; }
        public static UnitGrid UnitGrid { get => Instance._unitGrid; }

        // Protected

        protected override List<GoToGroup> GenerateGoToGroups(List<RBTUnit> units, Vector3 destination)
        {
            var proximityGroups = CalcProximityGroups(units);
            var goToGroups = new List<GoToGroup>();
            goToGroups.Capacity = proximityGroups.Count;
            foreach (var group in proximityGroups)
                goToGroups.Add(new GoToGroup(units, destination));
            return goToGroups;
        }

        protected override List<PatrolGroup> GeneratePatrolGroups(List<RBTUnit> units, Vector3 destination)
        {
            throw new NotImplementedException();
        }

        protected override List<AttackGroup> GenerateAttackGroups(List<RBTUnit> units, IAttackable target)
        {
            throw new NotImplementedException();
        }

        protected override List<FollowGroup> GenerateFollowGroups(List<RBTUnit> units, GameObject target)
        {
            throw new NotImplementedException();
            // Vector3 destination = target.transform.position;

            // var proximityGroups = CalcProximityGroups(units);
            // var followGroups = new List<FollowGroup>();
            // followGroups.Capacity = proximityGroups.Count;
            // foreach (var pg in proximityGroups)
            // {
            //     var commandUnits = CalcCommandUnits(pg, destination);
            //     followGroups.Add(new FollowGroup(commandUnits, 0, target));
            // }
            // return followGroups;
        }

        // Private
        List<List<RBTUnit>> CalcProximityGroups(List<RBTUnit> units)
        {
            var unitsWithoutGroup = new HashSet<RBTUnit>(units);
            var b = Mathf.Sqrt(Mathf.Pow(_subgroupDistance, 2) / 2);
            var bounds = new Vector3(b, 0, b);

            Action<RBTUnit, List<RBTUnit>> calcConnectingUnits = null;
            calcConnectingUnits = (startingUnit, unitsInCommandGroup) =>
            {
                unitsWithoutGroup.Remove(startingUnit);
                unitsInCommandGroup.Add(startingUnit);
                var nearbyUnits = _unitGrid.FindNear(startingUnit.transform.position, bounds);
                foreach (var unit in nearbyUnits)
                {
                    if (unitsWithoutGroup.Contains(unit))
                        calcConnectingUnits(unit, unitsInCommandGroup);
                }
            };

            var proximityGroups = new List<List<RBTUnit>>();
            foreach (var unit in units)
            {
                if (unitsWithoutGroup.Contains(unit))
                {
                    var group = new List<RBTUnit>();
                    calcConnectingUnits(unit, group);
                    proximityGroups.Add(group);
                }
            }

            return proximityGroups;
        }

        // Unity functions
        protected override void Awake()
        {
            base.Awake();
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitBehaviourManager on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                Destroy(this);
                return;
            }
            else Instance = this;
        }
    }
}
