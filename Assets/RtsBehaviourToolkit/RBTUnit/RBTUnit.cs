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

        [SerializeField]
        int _health = 5;

        [SerializeField, Min(1)]
        int _maxHealth = 5;

        [field: SerializeField]
        public AttackInfo Attack;

        [SerializeField]
        Bounds _bounds;

        [field: SerializeField]
        public Team Team { get; private set; }

        [SerializeField]
        bool _disablePhysicsOnDeath = true;

        public int Health
        {
            get => _health;
            set
            {
                if (value > _maxHealth)
                    _health = _maxHealth;
                else if (value < 0)
                    _health = 0;
                else
                    _health = value;
            }
        }
        public int MaximumHealth
        {
            get => _maxHealth;
            set
            {
                if (value < 1)
                {
                    _maxHealth = 1;
                    Debug.LogWarning("Can't set MaximumHealth to less than 1");
                }
                else
                    _maxHealth = value;

                if (_maxHealth < _health)
                    _health = _maxHealth;
            }
        }
        public bool Alive { get => _health > 0; }
        public Vector3 Position { get => _rigidBody.position; set => _rigidBody.position = value; }
        public GameObject GameObject { get => gameObject; }
        public Vector3 Velocity { get => _rigidBody.velocity; }
        public Vector3 MovementSum { get => _movementSum; }

        public static List<RBTUnit> ActiveUnits { get; private set; } = new List<RBTUnit>();
        public static Dictionary<Team, List<RBTUnit>> ActiveUnitsPerTeam { get; private set; } = new Dictionary<Team, List<RBTUnit>>();
        public UnitBounds Bounds { get; private set; }
        public UnitState State
        {
            get => _unitState;
            private set
            {
                var prevState = _unitState;
                _unitState = value;
                if (prevState != _unitState)
                    _onStateChanged.Invoke(new OnStateChangedEvent(this, prevState, _unitState));
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

        public enum UnitState
        {
            Idling, Moving, Attacking, Dead
        }

        // Private
        bool _selected = false;
        Rigidbody _rigidBody;
        CapsuleCollider _collider;
        Vector3 _movementSum;
        UnitState _unitState;
        bool _isOnGround = false;
        IEnumerator _attackLoop;

        void UpdateLookDirection()
        {
            Vector3 lookDirection;
            if (State == UnitState.Attacking && AttackTarget != null)
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
            var newState = UnitState.Idling;
            if (_health == 0)
                newState = UnitState.Dead;
            else if (_rigidBody.velocity != Vector3.zero)
                newState = UnitState.Moving;
            else if (AttackTarget != null)
            {
                var offset = AttackTarget.Position - Position;
                var sqrDistXZ = new Vector3(offset.x, 0, offset.z).sqrMagnitude;
                if (sqrDistXZ < Attack.Range * Attack.Range)
                    newState = UnitState.Attacking;
            }

            State = newState;
        }

        void UpdateMovementVelocity()
        {
            if (_movementSum == Vector3.zero)
                return;

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

            // reset movementSum until next update
            _movementSum = Vector3.zero;
        }

        void DoAttack()
        {
            if (_attackLoop != null) return;

            _attackLoop = AttackLoop();
            StartCoroutine(_attackLoop);
        }

        void DontAttack()
        {
            if (_attackLoop == null) return;

            StopCoroutine(_attackLoop);
            _attackLoop = null;
        }

        IEnumerator AttackLoop()
        {
            while (State == UnitState.Attacking && AttackTarget != null)
            {
                yield return new WaitForSeconds(Attack.TimePerAttack);
                AttackTarget.Health -= Attack.Damage;
            }
        }

        void TogglePhysics(bool enabled)
        {
            _rigidBody.isKinematic = !enabled;
            _collider.enabled = enabled;
        }

        // Unity functions
        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            Bounds = new UnitBounds(transform, _bounds);

            if (Team && !ActiveUnitsPerTeam.ContainsKey(Team))
                ActiveUnitsPerTeam[Team] = new List<RBTUnit>();

            _onStateChanged += (evnt) =>
            {
                if (evnt.NewState == RBTUnit.UnitState.Dead)
                {
                    TogglePhysics(!_disablePhysicsOnDeath);
                    Selected = false;
                }
                else TogglePhysics(true);
            };
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

            var mayAttack = State == UnitState.Attacking && AttackTarget != null;
            if (mayAttack)
                DoAttack();
            else
                DontAttack();
        }

        void FixedUpdate()
        {
            if (!_isOnGround)
            {
                _rigidBody.isKinematic = false;
                return;
            }
            _rigidBody.isKinematic = _movementSum == Vector3.zero;
            UpdateMovementVelocity();
            UpdateState();
        }

        void OnCollisionEnter(Collision other)
        {
            _isOnGround = true;
        }

        // Unity Editor functions
        void OnValidate()
        {
            Bounds = new UnitBounds(transform, _bounds);
            Health = _health;
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