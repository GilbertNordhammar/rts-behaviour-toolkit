using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    public class RBTUnitCommander : MonoBehaviour
    {
        // Inspector
        [SerializeField]
        UnitBehaviourManagerBase _unitBehaviourManager;

        [SerializeField]
        LayerMask _targetable;

        // Public
        public static RBTUnitCommander Instance { get; private set; }

        public struct CommandGivenEvent
        {
            public CommandGivenEvent(RBTUnitCommander sender, Vector3 position, List<RBTUnit> units)
            {
                Sender = sender;
                Position = position;
                Units = units;
            }

            public readonly RBTUnitCommander Sender;
            public readonly Vector3 Position;
            public readonly List<RBTUnit> Units;
        }

        public event Action<CommandGivenEvent> OnCommandGiven
        {
            add
            {
                lock (_onCommandGivenLock)
                {
                    _onCommandGiven += value;
                }
            }
            remove
            {
                lock (_onCommandGivenLock)
                {
                    _onCommandGiven -= value;
                }
            }
        }

        // Private
        event Action<CommandGivenEvent> _onCommandGiven = delegate { };
        readonly object _onCommandGivenLock = new object();
        RBTUnitSelector.OnUnitsSelectedEvent _onUnitsSelectedEvent;

        void HandleOnUnitsSelected(RBTUnitSelector.OnUnitsSelectedEvent evnt)
        {
            _onUnitsSelectedEvent = evnt;
        }

        void CommandUnits(List<RBTUnit> units)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var clickMask = RBTConfig.WalkableMask | _targetable;
            RaycastHit clickHit = new RaycastHit();

            if (Physics.Raycast(ray, out clickHit, 100f, clickMask))
            {
                var clickedObject = clickHit.collider.gameObject;
                bool isWalkable = RBTConfig.WalkableMask == (RBTConfig.WalkableMask | (1 << clickedObject.layer));
                bool isTargetable = _targetable == (_targetable | (1 << clickedObject.layer));
                if (isWalkable)
                {
                    NavMeshHit navMeshHit;
                    var walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
                    var walkablePos = NavMesh.SamplePosition(clickHit.point, out navMeshHit, 1f, walkableMask);
                    if (walkablePos)
                    {
                        _unitBehaviourManager.CommandGoTo(units, clickHit.point);
                        _onCommandGiven.Invoke(new CommandGivenEvent(this, clickHit.point, units));
                    }
                }
                else if (isTargetable)
                {
                    StartCoroutine(Highlight(clickedObject));

                    var attackable = clickedObject.GetComponent<IAttackable>();
                    var mayFollow = true;

                    if (attackable != null)
                    {
                        var mayAttack = attackable.Team != _onUnitsSelectedEvent.Team;
                        if (mayAttack)
                        {
                            mayFollow = false;
                            foreach (var unit in units)
                                unit.AttackTarget = attackable;
                            _unitBehaviourManager.CommandAttack(units, attackable);
                        }
                    }

                    if (mayFollow)
                        _unitBehaviourManager.CommandFollow(units, clickedObject);
                }
            }
        }

        // TODO: Make something that doesn't just highlight units
        IEnumerator Highlight(GameObject obj)
        {
            var unit = obj.GetComponent<RBTUnit>();
            if (unit)
            {
                unit.Selected = true;
                yield return new WaitForSecondsRealtime(0.3f);
                unit.Selected = false;
            }

            yield return null;
        }

        // Unity functions
        void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitCommander on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                Destroy(this);
                return;
            }
            else Instance = this;

            if (!_unitBehaviourManager)
                Debug.LogWarning($"There's no unit behaviour manager assigned to RBTUnitCommander on '{gameObject.name}''");
        }

        void Start()
        {
            if (RBTUnitSelector.Instance)
                RBTUnitSelector.Instance.OnUnitsSelected += HandleOnUnitsSelected;

            if (RBTConfig.WalkableMask == 0)
                Debug.LogWarning("Units can't be commanded since 'WalkableMask' is set to 'Nothing'");

            if ((RBTConfig.WalkableMask & _targetable) > 0)
                Debug.LogWarning($"'Targetable' shares layers with RBTConfig.WalkableMask, which might cause unexpected behaviour");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1) && _onUnitsSelectedEvent)
            {
                if (_onUnitsSelectedEvent.SelectedUnits.Count > 0)
                    CommandUnits(_onUnitsSelectedEvent.SelectedUnits);
            }
        }
    }
}