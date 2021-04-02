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

        // Public
        public static RBTUnitBehaviourManager Instance { get; private set; }

        // Private
        List<RBTUnitBehaviour> _unitBehaviours;
        List<CommandGroup> _commandGroups = new List<CommandGroup>();
        UnitGrid _unitGrid = new UnitGrid();

        void HandleOnCommandGiven(RBTUnitCommander.CommandGivenEvent evnt)
        {
            // TODO: Offset target positions according to leader

            var commandUnits = new List<CommandUnit>();
            foreach (var unit in evnt.Units)
            {
                var path = new NavMeshPath();
                NavMesh.CalculatePath(unit.transform.position, evnt.Position, NavMesh.AllAreas, path);
                commandUnits.Add(new CommandUnit(unit, path));
            }

            _commandGroups.Add(new CommandGroup(commandUnits, _subgroupDistance));
        }

        void UpdateCommandGroups()
        {
            var commandGroupsToRemove = new List<CommandGroup>();
            foreach (var commandGroup in _commandGroups)
            {
                var unitsToRemove = new List<CommandUnit>();
                foreach (var unit in commandGroup.Units)
                {
                    if (unit.Unit.CommandGroupId != commandGroup.Id)
                        unitsToRemove.Add(unit);
                }

                foreach (var unit in unitsToRemove)
                    commandGroup.Units.Remove(unit);

                if (commandGroup.Finished)
                    commandGroupsToRemove.Add(commandGroup);
                // else
                //     commandGroup.UpdateSubgroups();
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

            _unitBehaviours = GetComponentsInChildren<RBTUnitBehaviour>().ToList();

            RBTUnit.OnActivated += (evnt) =>
            {
                _unitGrid.Add(evnt.sender);
            };

            RBTUnit.OnDeactivated += (evnt) =>
            {
                _unitGrid.Remove(evnt.sender);
            };
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
                foreach (var behaviour in _unitBehaviours)
                    behaviour.Execute(commandGroup);
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

            if (_commandGroups.Count > 0)
            {
                foreach (var group in _commandGroups)
                {
                    foreach (var commandUnit in group.Units)
                    {
                        foreach (var corner in commandUnit.Path.corners)
                        {
                            Gizmos.DrawSphere(corner, 0.3f);
                        }
                    }
                }
            }

            _unitGrid.DrawGizmos(UnitGrid.GizmosDrawMode.Wire, new Color(1, 0, 0, 0.5f));

            Gizmos.color = originalColor;
        }
    }
}

