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
        public override void CommandMovement(List<RBTUnit> units, Vector3 destination)
        {
            // TODO
        }

        public override void CommandPatrol(List<RBTUnit> units, Vector3 destination)
        {
            // TODO
        }

        public override void CommandAttack(List<RBTUnit> units, RBTUnit unit)
        {
            // TODO
        }

        public override void CommandFollow(List<RBTUnit> units, GameObject obj)
        {
            // TODO
        }

        protected override List<CommandGroup> GenerateCommandGroups(List<RBTUnit> units, Vector3 destination)
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

            var commandGroups = new List<CommandGroup>();
            foreach (var unit in units)
            {
                if (unitsWithoutGroup.Contains(unit))
                {
                    var unitsInCommandGroup = new List<RBTUnit>();
                    calcConnectingUnits(unit, unitsInCommandGroup);
                    var commandUnits = CalcCommandUnits(unitsInCommandGroup, destination);
                    if (commandUnits.Count > 0)
                        commandGroups.Add(new CommandGroup(commandUnits, destination, CommandType.GoToAndStop));
                }
            }

            return commandGroups;
        }

        // Private
        List<CommandUnit> CalcCommandUnits(List<RBTUnit> units, Vector3 destination)
        {
            var center = new Vector3();
            foreach (var unit in units)
            {
                center += unit.transform.position;
            }
            center /= units.Count;

            var navMeshPath = new NavMeshPath();
            NavMesh.CalculatePath(center, destination, NavMesh.AllAreas, navMeshPath);

            var commandUnits = new List<CommandUnit>();
            commandUnits.Capacity = units.Count;
            if (navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                foreach (var unit in units)
                {
                    var posOffset = unit.transform.position - center;
                    var nodes = new Vector3[navMeshPath.corners.Length];
                    Array.Copy(navMeshPath.corners, nodes, navMeshPath.corners.Length);
                    for (int j = 0; j < nodes.Length; j++)
                        nodes[j] += posOffset;
                    commandUnits.Add(new CommandUnit(unit, nodes));
                }
            }

            return commandUnits;
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
