using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public readonly struct UnitBounds
    {
        public UnitBounds(Transform transform, Bounds bounds)
        {
            _transform = transform;
            _bounds = bounds;
        }

        public Vector3 Center { get => _transform.position + _bounds.center; }
        public Vector3 Extents { get => Vector3.Scale(_bounds.extents, _transform.localScale); }
        public Vector3 Size { get => Vector3.Scale(_bounds.size, _transform.localScale); }

        /// <summary>
        /// Returns [(x,y,z), (x,y,-z), (-x, y,-z), (-x,y,z), (x,-y,z), (x,-y,-z), (-x, -y,-z), (-x,-y,z)]
        /// </summary>
        public Vector3[] Corners
        {
            get
            {
                var position = _transform.position;
                var rotation = _transform.rotation;
                var extents = Vector3.Scale(_bounds.extents, _transform.localScale);
                var center = Vector3.Scale(_bounds.center, _transform.localScale);

                return new Vector3[] {
                        // top
                        position + rotation * (center + new Vector3(extents.x, extents.y, extents.z)),
                        position + rotation * (center + new Vector3(extents.x, extents.y, -extents.z)),
                        position + rotation * (center + new Vector3(-extents.x, extents.y, -extents.z)),
                        position + rotation * (center + new Vector3(-extents.x, extents.y, extents.z)),
                        // bottom
                        position + rotation * (center + new Vector3(extents.x, -extents.y, extents.z)),
                        position + rotation * (center + new Vector3(extents.x, -extents.y, -extents.z)),
                        position + rotation * (center + new Vector3(-extents.x, -extents.y, -extents.z)),
                        position + rotation * (center + new Vector3(-extents.x, -extents.y, extents.z)),
                    };
            }
        }

        // private
        readonly Transform _transform;
        readonly Bounds _bounds;
    }
}

