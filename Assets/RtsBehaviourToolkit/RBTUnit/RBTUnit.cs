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
            _movement += movement;
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
        Vector3 _movement;
        CapsuleCollider _collider;

        // Unity functions
        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
        }

        void FixedUpdate()
        {
            var movement = _movement.normalized * _speed;
            // _rigidBody.AddForce(movement);
            _rigidBody.velocity = movement;
            _movement = Vector3.zero;
        }

        private void Update()
        {
            if (_rigidBody.velocity.magnitude > 0.1f)
            {
                transform.LookAt(transform.position + _rigidBody.velocity.normalized);
            }
        }

        // Unity Editor functions
        void OnEnable()
        {
            ActiveUnits.Add(this);
        }

        void OnDisable()
        {
            ActiveUnits.Remove(this);
        }

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