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
        [SerializeField]
        [Min(0)]
        float _speed = 2.0f;
        [SerializeField]
        Bounds _bounds;

        // Public
        public void Damage(int damage)
        {

        }

        public int Health { get; set; }

        public int MaximumHealth { get; set; }

        public bool Alive { get; }

        public Vector3 Position { get => transform.position; }

        public static List<RBTUnit> ActiveUnits { get; private set; } = new List<RBTUnit>();

        public UnitBounds Bounds { get; private set; }

        public void ClearCommandGroup()
        {
            CommandGroupId = "none";
        }

        public void AssignCommandGroup(string id)
        {
            if (id == null) id = "none";
            CommandGroupId = id;
        }

        public string CommandGroupId { get; private set; } = "none";

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
                    var evnt = new UnitEvent() { sender = this };
                    if (value)
                        _onSelected.Invoke(evnt);
                    else
                        _onDeselected.Invoke(evnt);
                }
                _selected = value;
            }
        }

        // Private
        bool _selected = false;
        Rigidbody _rigidBody;
        Vector3 _movementSum;
        CapsuleCollider _collider;

        void SetLookDirection()
        {
            if (_rigidBody.velocity.magnitude < 0.1f) return;

            var lookDirection = _rigidBody.velocity.normalized;
            lookDirection.y = 0;
            transform.LookAt(transform.position + lookDirection);
        }

        // Unity functions
        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            Bounds = new UnitBounds(transform, _bounds);
        }

        void Start()
        {
            _onActivated.Invoke(new UnitEvent() { sender = this });
        }

        void OnEnable()
        {
            ActiveUnits.Add(this);
            _onActivated.Invoke(new UnitEvent() { sender = this });
        }

        void OnDisable()
        {
            ActiveUnits.Remove(this);
            _onDeactivated.Invoke(new UnitEvent() { sender = this });
        }

        void FixedUpdate()
        {
            // disbling physics when unit hasn't been set to move
            if (_movementSum == Vector3.zero)
            {
                _rigidBody.isKinematic = true;
                return;
            }
            _rigidBody.isKinematic = false;

            // Snapping unit to floor and setting surface normal
            var surfaceNormal = new Vector3();
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position + _rigidBody.velocity * Time.fixedDeltaTime, -Vector3.up, out hit, 1, RBTConfig.WalkableMask))
            {
                surfaceNormal = hit.normal;
                _rigidBody.position = hit.point;
                _rigidBody.position += new Vector3(0, 0.2f, 0);
            }

            SetLookDirection();

            // Calculationg adjusted movement (i.e. making it parallell to unit's up vector)
            var movement = _movementSum.normalized;
            var diffAngle = Vector3.Angle(surfaceNormal, movement) - 90f;
            movement = Quaternion.AngleAxis(-diffAngle, transform.right) * movement;
            movement *= _speed;

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