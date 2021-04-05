using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    public partial class CommandGroup
    {
        public CommandGroup(List<CommandUnit> units)
        {
            Units = new List<CommandUnit>(units);
            foreach (var unit in Units)
            {
                unit.Unit.AssignCommandGroup(Id);
            }
        }

        public void Update()
        {
            var unitsToRemove = new List<CommandUnit>();
            foreach (var unit in Units)
            {
                var prevCorner = unit.CurrentPath.NextCorner;

                unit.Update();

                if (prevCorner != unit.CurrentPath.NextCorner)
                    _onNewCorner.Invoke(new NewCornerEvent(this, unit, prevCorner));

                if (unit.Finished || unit.Unit.CommandGroupId != Id)
                {
                    if (unit.Finished)
                        _onFinished.Invoke(new FinishedEvent(this, unit));
                    else
                        _onNewGroup.Invoke(new NewGroupEvent(this, unit));
                    unitsToRemove.Add(unit);
                }
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
        public CommandUnit(RBTUnit unit, Path path)
        {
            Unit = unit;
            PathQueue.Add(path);
        }

        public CommandUnit(RBTUnit unit, Vector3[] pathNodes)
            : this(unit, new Path(pathNodes))
        {
        }

        public RBTUnit Unit { get; }

        public List<Path> PathQueue { get; } = new List<Path>();
        public Path CurrentPath { get => PathQueue.Last(); }

        public void PushPath(Path path)
        {
            PathQueue.Add(path);
        }

        public void PushPath(Vector3[] nodes)
        {
            PathQueue.Add(new Path(nodes));
        }

        public void Update()
        {
            var absOffset = CurrentPath.NextCorner - Unit.transform.position;
            absOffset = new Vector3(Mathf.Abs(absOffset.x), Mathf.Abs(absOffset.y), Mathf.Abs(absOffset.z));
            if (absOffset.x < 0.1 && absOffset.z < 0.1 && absOffset.y < 1.0) // base "absOffset.y < 1.0" off of unit height
            {
                CurrentPath.Increment();
                if (CurrentPath.Traversed)
                {
                    if (PathQueue.Count == 1)
                        Finished = true;
                    else
                        PathQueue.RemoveAt(PathQueue.Count - 1);
                }
            }
        }

        public int NextCornerIndex
        {
            get => CurrentPath.NextCornerIndex;
        }

        public void MarkAsFinished()
        {
            Finished = true;
        }

        public bool Finished { get; private set; }
    }
}
