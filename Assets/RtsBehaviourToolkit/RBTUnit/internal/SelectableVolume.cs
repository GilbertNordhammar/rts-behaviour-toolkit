using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public partial class RBTUnit
    {
        [Serializable]
        public struct SelectableVolume
        {
            public Vector3 Size
            {
                get => _size;
                set
                {
                    _size.x = Mathf.Max(0, value.x);
                    _size.y = Mathf.Max(0, value.y);
                    _size.z = Mathf.Max(0, value.z);
                }
            }
            public Vector3 Offset
            {
                get => _offset;
                set
                {
                    _offset.x = Mathf.Max(0, value.x);
                    _offset.y = Mathf.Max(0, value.y);
                    _offset.z = Mathf.Max(0, value.z);
                }
            }

            public Vector3[] GetPoints(Vector3 position, Quaternion rotation)
            {
                var halfSize = _size / 2;
                return new Vector3[] {
                        position + rotation * (_offset + new Vector3(halfSize.x, halfSize.y, halfSize.z)),
                        position + rotation * (_offset + new Vector3(halfSize.x, halfSize.y, -halfSize.z)),
                        position + rotation * (_offset + new Vector3(-halfSize.x, halfSize.y, halfSize.z)),
                        position + rotation * (_offset + new Vector3(-halfSize.x, halfSize.y, -halfSize.z)),
                        position + rotation * (_offset + new Vector3(halfSize.x, -halfSize.y, halfSize.z)),
                        position + rotation * (_offset + new Vector3(halfSize.x, -halfSize.y, -halfSize.z)),
                        position + rotation * (_offset + new Vector3(-halfSize.x, -halfSize.y, halfSize.z)),
                        position + rotation * (_offset + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z)),
                        position + _offset
                    };
            }

            [SerializeField]
            [Min(0)]
            private Vector3 _size;

            [SerializeField]
            private Vector3 _offset;
        }
    }
}