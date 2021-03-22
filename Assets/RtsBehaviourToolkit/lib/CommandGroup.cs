using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    public class CommandGroup
    {
        public CommandGroup(List<CommandUnit> units, RBTUnit leader)
        {
            Leader = leader;
            CommandUnits = units;

            foreach (var unit in units)
                unit.Unit.CommandGroupId = Id;
        }

        public string Id { get; } = System.Guid.NewGuid().ToString();
        public RBTUnit Leader { get; }
        public List<CommandUnit> CommandUnits { get; }
    }

    public class CommandUnit
    {
        // Public
        public CommandUnit(RBTUnit unit, NavMeshPath path)
        {
            Unit = unit;
            Path = path;
        }
        public RBTUnit Unit { get; }

        public NavMeshPath Path { get; }

        public Vector3 NextCorner
        {
            get
            {
                return Path.corners[NextCornerIndex];
            }
        }

        public Vector3 OffsetToNextCorner
        {
            get => Path.corners[NextCornerIndex] - Unit.transform.position;
        }

        public float DistToNextCorner
        {
            get => Vector3.Distance(Path.corners[NextCornerIndex], Unit.transform.position);
        }

        public int NextCornerIndex
        {
            get
            {
                var absOffset = Path.corners[_indexNextCorner] - Unit.transform.position;
                absOffset = new Vector3(Mathf.Abs(absOffset.x), Mathf.Abs(absOffset.y), Mathf.Abs(absOffset.z));
                if (absOffset.x < 0.1 && absOffset.z < 0.1 && absOffset.y < 1.0) // base absOffset.y < 1.0 off of unit height 
                {
                    _indexNextCorner++;
                    if (_indexNextCorner >= Path.corners.Length)
                    {
                        _indexNextCorner = Path.corners.Length - 1;
                        HasTraversedPath = true;
                    }
                }
                return _indexNextCorner;
            }
        }

        public bool HasTraversedPath { get; private set; }

        // Private
        private int _indexNextCorner = 0;
    }

}

