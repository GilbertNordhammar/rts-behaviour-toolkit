using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public partial class RBTUnit : MonoBehaviour
    {
        // Editor fields
        [SerializeField]
        [Min(0)]
        float _speed = 2.0f;
        [SerializeField]
        SelectableVolume _selectableVolume;

        // Public
        public static List<RBTUnit> ActiveUnits { get; private set; } = new List<RBTUnit>();
        public Vector3[] SelectablePoints { get => _selectableVolume.GetPoints(transform.position, transform.rotation); }

        public void AddMovement(Vector3 movement)
        {
            _movementSum += movement;
        }

        public float Height { get => _collider.height; }

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value != _selected)
                {
                    var evnt = new SelectionEvent() { sender = this };
                    if (value)
                        _onSelected.Invoke(evnt);
                    else
                        _onDeselected.Invoke(evnt);
                }
                _selected = value;
            }
        }

        public event Action<SelectionEvent> OnSelected
        {
            add
            {
                lock (_onSelectedLock)
                {
                    _onSelected += value;
                }
            }
            remove
            {
                lock (_onSelectedLock)
                {
                    _onSelected -= value;
                }
            }
        }

        public event Action<SelectionEvent> OnDeselected
        {
            add
            {
                lock (_onDeselectedLock)
                {
                    _onDeselected += value;
                }
            }
            remove
            {
                lock (_onDeselectedLock)
                {
                    _onDeselected -= value;
                }
            }
        }

        public struct SelectionEvent
        {
            public RBTUnit sender;
        }

        // Private
        bool _selected = false;
        event Action<SelectionEvent> _onSelected = delegate { };
        event Action<SelectionEvent> _onDeselected = delegate { };
        readonly object _onSelectedLock = new object();
        readonly object _onDeselectedLock = new object();
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
            var walkableMask = LayerMask.GetMask(new string[] { "RBT Floor" });
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(transform.position + _rigidBody.velocity * Time.fixedDeltaTime, -Vector3.up, out hit, 1, walkableMask))
            {
                surfaceNormal = hit.normal;
                _rigidBody.position = hit.point;
                _rigidBody.position += new Vector3(0, 0.1f, 0);
            }

            SetLookDirection();

            // Calculationg adjusted movement (i.e. making it parallell to unit's up vector)
            var movement = _movementSum.normalized;
            var diffAngle = Vector3.Angle(surfaceNormal, movement) - 90f;
            movement = Quaternion.AngleAxis(-diffAngle, transform.right) * movement;
            movement *= _speed;

            _rigidBody.velocity = movement;

            // Debug
            Debug.Log(_rigidBody.velocity.magnitude);
            Debug.DrawRay(transform.position, movement * 5, Color.red);
            Debug.DrawRay(transform.position, _movementSum.normalized * 4, Color.white);
            Debug.DrawRay(transform.position, surfaceNormal * 4, Color.green);

            // reset movement until next update
            _movementSum = Vector3.zero;
        }

        void OnEnable()
        {
            ActiveUnits.Add(this);
        }

        void OnDisable()
        {
            ActiveUnits.Remove(this);
        }

        // Unity Editor functions
        void OnDrawGizmosSelected()
        {
            var originalColor = Gizmos.color;
            Gizmos.color = Color.white;

            var p = SelectablePoints;
            // top
            Gizmos.DrawLine(p[0], p[1]);
            Gizmos.DrawLine(p[1], p[3]);
            Gizmos.DrawLine(p[3], p[2]);
            Gizmos.DrawLine(p[2], p[0]);
            // middle
            Gizmos.DrawLine(p[0], p[4]);
            Gizmos.DrawLine(p[1], p[5]);
            Gizmos.DrawLine(p[3], p[7]);
            Gizmos.DrawLine(p[2], p[6]);
            //bottom
            Gizmos.DrawLine(p[4], p[5]);
            Gizmos.DrawLine(p[5], p[7]);
            Gizmos.DrawLine(p[7], p[6]);
            Gizmos.DrawLine(p[6], p[4]);

            Gizmos.color = originalColor;
        }
    }
}