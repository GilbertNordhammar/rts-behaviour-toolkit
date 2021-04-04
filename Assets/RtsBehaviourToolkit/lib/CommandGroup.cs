using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    public class CommandGroup
    {
        public CommandGroup(List<RBTUnit> units, Vector3 destination)
        {
            var center = new Vector3();
            foreach (var unit in units)
            {
                center += unit.transform.position;
            }
            center /= units.Count;

            var navMeshPath = new NavMeshPath();
            NavMesh.CalculatePath(center, destination, NavMesh.AllAreas, navMeshPath);

            if (navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                Units.Capacity = units.Count;
                for (int i = 0; i < units.Count; i++)
                {
                    var posOffset = units[i].transform.position - center;
                    var path = new Vector3[navMeshPath.corners.Length];
                    Array.Copy(navMeshPath.corners, path, navMeshPath.corners.Length);
                    for (int j = 0; j < path.Length; j++)
                        path[j] += posOffset;
                    Units.Add(new CommandUnit(units[i], path));
                }

                foreach (var unit in Units)
                    unit.Unit.AssignCommandGroup(Id);
            }
        }

        public void Update()
        {
            var unitsToRemove = new List<CommandUnit>();
            foreach (var unit in Units)
            {
                if (unit.Finished || unit.Unit.CommandGroupId != Id)
                    unitsToRemove.Add(unit);
            }

            foreach (var unit in unitsToRemove)
            {
                Units.Remove(unit);
            }
        }

        public List<CommandUnit> Units { get; private set; } = new List<CommandUnit>();
        public string Id { get; } = System.Guid.NewGuid().ToString();
    }

    public class CommandUnit
    {
        // Public
        public CommandUnit(RBTUnit unit, Vector3[] path)
        {
            Unit = unit;
            Path = path;
        }
        public RBTUnit Unit { get; }

        public Vector3[] Path { get; }

        public Vector3 NextCorner
        {
            get
            {
                return Path[NextCornerIndex];
            }
        }

        public Vector3 OffsetToNextCorner
        {
            get => Path[NextCornerIndex] - Unit.transform.position;
        }

        public float DistToNextCorner
        {
            get => Vector3.Distance(Path[NextCornerIndex], Unit.transform.position);
        }

        public int NextCornerIndex
        {
            get
            {
                var absOffset = Path[_indexNextCorner] - Unit.transform.position;
                absOffset = new Vector3(Mathf.Abs(absOffset.x), Mathf.Abs(absOffset.y), Mathf.Abs(absOffset.z));
                if (absOffset.x < 0.1 && absOffset.z < 0.1 && absOffset.y < 1.0) // base "absOffset.y < 1.0" off of unit height
                {
                    _indexNextCorner++;
                    if (_indexNextCorner >= Path.Length)
                    {
                        _indexNextCorner = Path.Length - 1;
                        Finished = true;
                    }
                }
                return _indexNextCorner;
            }
        }

        public bool Finished { get; set; }

        // Private
        private int _indexNextCorner = 0;
    }

}

