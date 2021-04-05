using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    [RequireComponent(typeof(NavMesh))]
    public partial class RBTUnitBehaviourManager : MonoBehaviour
    {
        // Unity editor
        [SerializeField]
        [Min(0f)]
        float _subgroupDistance = 2f;
        [SerializeField]
        bool _showUnitGrid = false;
        [SerializeField]
        bool _showPaths = false;
        [SerializeField]
        UnitBehaviour[] _behaviours;

        // Public
        public static RBTUnitBehaviourManager Instance { get; private set; }

        // Private
        List<CommandGroup> _commandGroups = new List<CommandGroup>();
        UnitGrid _unitGrid = new UnitGrid();

        void HandleOnCommandGiven(RBTUnitCommander.CommandGivenEvent evnt)
        {
            var commandGroups = CalcCommandgroups(evnt.Units, _subgroupDistance, evnt.Position);
            foreach (var behaviour in _behaviours)
            {
                foreach (var group in commandGroups)
                {
                    behaviour.OnCommandGroupCreated(group);
                }
            }
            _commandGroups.AddRange(commandGroups);
        }

        List<CommandGroup> CalcCommandgroups(List<RBTUnit> units, float maxDistance, Vector3 destination)
        {
            var unitsWithoutGroup = new HashSet<RBTUnit>(units);
            var b = Mathf.Sqrt(Mathf.Pow(maxDistance, 2) / 2);
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
                        commandGroups.Add(new CommandGroup(commandUnits));
                }
            }

            return commandGroups;
        }

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

        void UpdateCommandGroups()
        {
            var commandGroupsToRemove = new List<CommandGroup>();
            foreach (var commandGroup in _commandGroups)
            {
                commandGroup.Update();
                if (commandGroup.Units.Count == 0)
                    commandGroupsToRemove.Add(commandGroup);
            }

            foreach (var group in commandGroupsToRemove)
            {
                foreach (var unit in group.Units)
                    unit.Unit.ClearCommandGroup();
                _commandGroups.Remove(group);
            }
        }

        // Unity functions
        void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitBehaviourManager on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                Destroy(this);
                return;
            }
            else Instance = this;

            RBTUnit.OnActivated += (evnt) =>
            {
                _unitGrid.Add(evnt.sender);
            };

            RBTUnit.OnDeactivated += (evnt) =>
            {
                _unitGrid.Remove(evnt.sender);
            };

            for (int i = 0; i < _behaviours.Length; i++)
                _behaviours[i] = Instantiate(_behaviours[i]);
        }

        void Update()
        {
            // Gizmos flash like crazy if this is put in FixedUpdate(),
            // but performance improves
            foreach (var unit in RBTUnit.ActiveUnits)
                _unitGrid.Update(unit);
        }

        void FixedUpdate()
        {
            UpdateCommandGroups();

            foreach (var commandGroup in _commandGroups)
            {
                foreach (var behaviour in _behaviours)
                    behaviour.OnUpdate(commandGroup);
            }
        }

        // Unity editor functions
        void Start()
        {
            if (RBTUnitCommander.Instance)
                RBTUnitCommander.Instance.OnCommandGiven += HandleOnCommandGiven;
            else
                Debug.LogError("RBTUnitCommander couldn't subscribe to RBTUnitCommander.Instance.OnCommandGiven");

            foreach (var unit in RBTUnit.ActiveUnits)
                _unitGrid.Add(unit);
        }

        void OnDrawGizmos()
        {
            var originalColor = Gizmos.color;
            Gizmos.color = Color.red;

            if (_showPaths)
            {
                foreach (var group in _commandGroups)
                {
                    foreach (var commandUnit in group.Units)
                    {
                        var pathNodes = commandUnit.PathQueue.Last().Nodes;
                        for (int i = 0; i < pathNodes.Length; i++)
                        {
                            var node1 = pathNodes[i];
                            Gizmos.DrawSphere(node1, 0.2f);
                            if ((i + 1) < pathNodes.Length)
                            {
                                var node2 = pathNodes[i + 1];
                                Gizmos.DrawSphere(node2, 0.2f);
                                Gizmos.DrawLine(node1, node2);
                            }
                        }
                    }
                }
            }


            if (_showUnitGrid)
                _unitGrid.DrawGizmos(UnitGrid.GizmosDrawMode.Wire, new Color(1, 0, 0, 0.5f));

            Gizmos.color = originalColor;
        }
    }
}

