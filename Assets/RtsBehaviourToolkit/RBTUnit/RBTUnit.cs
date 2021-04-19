using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public partial class RBTUnit : MonoBehaviour, IAttackable, IMovable
    {
        // Inspector and public
        [field: SerializeField]
        [field: Min(0)]
        public float Speed { get; private set; } = 2.0f;

        [field: SerializeField]
        public AttackInfo Attack;

        [SerializeField]
        Bounds _bounds;

        [field: SerializeField]
        public Team Team { get; private set; }

        public int Health { get; set; }
        public int MaximumHealth { get; set; }
        public bool Alive { get; }
        public Vector3 Position { get => _rigidBody.position; set => _rigidBody.position = value; }
        public GameObject GameObject { get => gameObject; }
        public Vector3 Velocity { get => _rigidBody.velocity; }

        public static List<RBTUnit> ActiveUnits { get; private set; } = new List<RBTUnit>();
        public static Dictionary<Team, List<RBTUnit>> ActiveUnitsPerTeam { get; private set; } = new Dictionary<Team, List<RBTUnit>>();
        public UnitBounds Bounds { get; private set; }
        public ActionState State
        {
            get => _actionState;
            private set
            {
                var prevState = _actionState;
                _actionState = value;
                if (prevState != _actionState)
                    _onStateChanged.Invoke(new OnStateChangedEvent(this, prevState, _actionState));
            }
        }
        public IAttackable AttackTarget { get; set; }

        public void ClearCommandGroup()
        {
            CommandGroupId = "";
        }

        public void AssignCommandGroup(string id)
        {
            if (id == null) id = "";
            CommandGroupId = id;
        }

        public string CommandGroupId { get; private set; } = "";

        public void AddMovement(Vector3 movement)
        {
            _movementSum += movement;
        }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value != _selected)
                {
                    if (value)
                        _onSelected.Invoke(new OnSelectedEvent(this));
                    else
                        _onDeselected.Invoke(new OnDeselectedEvent(this));
                }
                _selected = value;
            }
        }

        public enum ActionState
        {
            Idling = 1, Moving = 2, Attacking = 4
        }

        // Private
        bool _selected = false;
        Rigidbody _rigidBody;
        Vector3 _movementSum;
        ActionState _actionState;
        bool _isOnGround = false;

        void UpdateLookDirection()
        {
            Vector3 lookDirection;
            if (State.HasFlag(ActionState.Attacking))
                lookDirection = (AttackTarget.Position - Position).normalized;
            else
            {
                if (_movementSum == Vector3.zero) return;
                lookDirection = _movementSum;
            }

            lookDirection.y = 0;
            var lookRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10);
        }

        void UpdateState()
        {
            var newState = ActionState.Idling;

            if (AttackTarget != null)
            {
                var offset = AttackTarget.Position - Position;
                var sqrDistXZ = new Vector3(offset.x, 0, offset.z).sqrMagnitude;
                if (sqrDistXZ < Attack.Range * Attack.Range)
                    newState |= ActionState.Attacking;
            }
            else
                newState &= ~ActionState.Attacking;

            if (_movementSum != Vector3.zero)
                newState |= ActionState.Moving;

            var notIdling = (newState & ~ActionState.Idling) > 0;
            if (notIdling)
                newState &= ~ActionState.Idling;

            State = newState;
        }

        // Unity functions
        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            Bounds = new UnitBounds(transform, _bounds);

            if (Team && !ActiveUnitsPerTeam.ContainsKey(Team))
                ActiveUnitsPerTeam[Team] = new List<RBTUnit>();
        }

        void Start()
        {
            _onActivated.Invoke(new OnActivatedEvent(this));
        }

        void OnEnable()
        {
            ActiveUnits.Add(this);
            ActiveUnitsPerTeam[Team].Add(this);
            _onActivated.Invoke(new OnActivatedEvent(this));
        }

        void OnDisable()
        {
            ActiveUnits.Remove(this);
            ActiveUnitsPerTeam[Team].Remove(this);
            _onDeactivated.Invoke(new OnDeActivatedEvent(this));
        }

        void Update()
        {
            UpdateLookDirection();
            UpdateState();
        }

        void FixedUpdate()
        {
            if (!_isOnGround)
            {
                _rigidBody.isKinematic = false;
                return;
            }

            if (_movementSum == Vector3.zero)
            {
                _rigidBody.isKinematic = true;
                return;
            }
            _rigidBody.isKinematic = false;

            // Snapping unit to floor and setting surface normal
            var surfaceNormal = Vector3.one;
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1, RBTConfig.WalkableMask))
                surfaceNormal = hit.normal;

            // Calculating adjusted movement (i.e. making it parallell to unit's up vector)
            var movement = _movementSum.normalized;
            var diffAngle = Vector3.Angle(surfaceNormal, movement) - 90f;
            var right = Vector3.Cross(surfaceNormal, movement).normalized;
            movement = Quaternion.AngleAxis(-diffAngle, right) * movement;
            movement *= Speed;
            _rigidBody.velocity = movement;

            // reset movement until next update
            _movementSum = Vector3.zero;
        }

        void OnCollisionEnter(Collision other)
        {
            _isOnGround = true;
        }

        // Unity Editor functions
        void OnValidate()
        {
            Bounds = new UnitBounds(transform, _bounds);
        }

        void OnDrawGizmosSelected()
        {
            var originalColor = Gizmos.color;
            Gizmos.color = Color.white;

            var p = Bounds.Corners;

            // top
            Gizmos.DrawLine(p[0], p[1]);
            Gizmos.DrawLine(p[1], p[2]);
            Gizmos.DrawLine(p[2], p[3]);
            Gizmos.DrawLine(p[3], p[0]);
            // middle
            Gizmos.DrawLine(p[0], p[4]);
            Gizmos.DrawLine(p[1], p[5]);
            Gizmos.DrawLine(p[2], p[6]);
            Gizmos.DrawLine(p[3], p[7]);
            //bottom
            Gizmos.DrawLine(p[4], p[5]);
            Gizmos.DrawLine(p[5], p[6]);
            Gizmos.DrawLine(p[6], p[7]);
            Gizmos.DrawLine(p[7], p[4]);

            Gizmos.color = originalColor;
        }
    }
}