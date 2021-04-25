using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField]
        KeyCode _patrolModiferKey = KeyCode.P;

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
                        if (Input.GetKey(_patrolModiferKey))
                            _unitBehaviourManager.CommandPatrol(units, clickHit.point);
                        else
                            _unitBehaviourManager.CommandGoTo(units, clickHit.point);
                        _onCommandGiven.Invoke(new CommandGivenEvent(this, clickHit.point, units));
                    }
                }
                else if (isTargetable)
                {
                    var mayFollow = true;
                    var mayAttack = false;

                    var attackable = clickedObject.GetComponent<IAttackable>();
                    if (attackable != null)
                    {
                        mayAttack = (attackable.Team != _onUnitsSelectedEvent.Team) && attackable.Alive;
                        mayFollow = !mayAttack && attackable.Alive;
                    }

                    if (mayAttack)
                        _unitBehaviourManager.CommandAttack(units, attackable);
                    else if (mayFollow)
                        _unitBehaviourManager.CommandFollow(units, clickedObject);

                    if (mayFollow || mayAttack)
                        StartCoroutine(Highlight(clickedObject));
                }
            }
        }

        // TODO: Make something that doesn't just highlight units
        IEnumerator Highlight(GameObject obj)
        {
            var unit = obj.GetComponent<RBTUnit>();
            if (unit)
            {
                var origSelected = unit.Selected;
                unit.Selected = true;
                yield return new WaitForSecondsRealtime(0.3f);
                unit.Selected = false;
                yield return new WaitForSecondsRealtime(0.3f);
                unit.Selected = origSelected;
            }
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
                var aliveSelected = _onUnitsSelectedEvent.SelectedUnits.Where(unit => unit.Alive).ToList();
                if (aliveSelected.Count > 0)
                    CommandUnits(_onUnitsSelectedEvent.SelectedUnits);
            }
        }
    }
}