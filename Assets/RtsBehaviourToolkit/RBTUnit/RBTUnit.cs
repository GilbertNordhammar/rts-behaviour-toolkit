using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public partial class RBTUnit : MonoBehaviour, IAttackable
    {
        // Editor fields
        [field: SerializeField]
        [field: Min(0)]
        public float Speed { get; private set; } = 2.0f;

        [field: SerializeField]
        public AttackInfo Attack;

        [SerializeField]
        Bounds _bounds;

        // Public
        public int Health { get; set; }
        public int MaximumHealth { get; set; }
        public bool Alive { get; }
        public Vector3 Position { get => _rigidBody.position; set => _rigidBody.position = value; }
        public GameObject GameObject { get => gameObject; }

        public static List<RBTUnit> ActiveUnits { get; private set; } = new List<RBTUnit>();
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
            Idling = 0, Moving = 1, Attacking = 2
        }

        // Private
        bool _selected = false;
        Rigidbody _rigidBody;
        Vector3 _movementSum;
        ActionState _actionState;

        // Unity functions
        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            Bounds = new UnitBounds(transform, _bounds);
        }

        void Start()
        {
            _onActivated.Invoke(new OnActivatedEvent(this));
        }

        void OnEnable()
        {
            ActiveUnits.Add(this);
            _onActivated.Invoke(new OnActivatedEvent(this));
        }

        void OnDisable()
        {
            ActiveUnits.Remove(this);
            _onDeactivated.Invoke(new OnDeActivatedEvent(this));
        }

        void Update()
        {
            var isAttacking = false;
            if (AttackTarget != null)
            {
                var offset = AttackTarget.Position - Position;
                var sqrDistXZ = new Vector3(offset.x, 0, offset.z).sqrMagnitude;
                if (sqrDistXZ < Attack.Range * Attack.Range)
                    isAttacking = true;
            }

            if (isAttacking)
                State |= ActionState.Attacking;
            else
                State &= ~ActionState.Attacking;
        }

        void FixedUpdate()
        {
            // disbling physics when unit hasn't been set to move
            if (_movementSum == Vector3.zero)
            {
                State &= ~ActionState.Moving;
                _rigidBody.isKinematic = true;
                return;
            }
            State |= ActionState.Moving;
            _rigidBody.isKinematic = false;

            // Snapping unit to floor and setting surface normal
            var surfaceNormal = Vector3.one;
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 1, RBTConfig.WalkableMask))
                surfaceNormal = hit.normal;

            // Setting look direction
            var movement = _movementSum.normalized;
            var lookDirection = movement;
            lookDirection.y = 0;
            transform.LookAt(transform.position + lookDirection);

            // Calculating adjusted movement (i.e. making it parallell to unit's up vector)
            var diffAngle = Vector3.Angle(surfaceNormal, movement) - 90f;
            movement = Quaternion.AngleAxis(-diffAngle, transform.right) * movement;
            movement *= Speed;

            _rigidBody.velocity = movement;

            // reset movement until next update
            _movementSum = Vector3.zero;
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