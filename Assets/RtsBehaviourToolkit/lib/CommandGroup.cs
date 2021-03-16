using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    public class CommandGroup
    {
        public string Id { get; } = System.Guid.NewGuid().ToString();
        public RBTUnit Leader { get; set; }
        public List<CommandUnit> CommandUnits { get; set; }
    }

    public class CommandUnit
    {
        // Public
        public RBTUnit Unit { get; set; }
        public NavMeshPath Path { get; set; }

        public Vector3 GetNextCorner
        {
            get => Path.corners[NextCornerIndex];
        }

        public float DistToNextCorner
        {
            get
            {
                var nextCornerPos = Path.corners[NextCornerIndex];
                var unitPos = Unit.transform.position;
                return Vector3.Distance(nextCornerPos, unitPos);
            }
        }

        public void IncrementCorner()
        {
            _indexNextCorner++;
        }

        public int NextCornerIndex
        {
            get => Mathf.Min(_indexNextCorner, Path.corners.Length - 1);
        }

        public bool HasTraversedPath
        {
            get => _indexNextCorner >= Path.corners.Length;
        }

        // Private
        private int _indexNextCorner = 0;
    }

}

