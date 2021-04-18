using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public class CommonGroupData
    {
        public void Update(IReadOnlyList<CommandUnit> units, int commanderIndex)
        {
            var updatedCommander = units[commanderIndex];
            if (!Commander || updatedCommander != Commander)
            {
                NewCommander = true;
                CommanderIndex = commanderIndex;
                Commander = units[commanderIndex];

                var center = Vector3.zero;
                foreach (var unit in units)
                    center += unit.Unit.Position;
                center /= units.Count;

                OffsetsUnitToCenter.Clear();
                OffsetsUnitToCenter.Capacity = units.Count;
                foreach (var unit in units)
                    OffsetsUnitToCenter.Add(center - unit.Unit.Position);

                OffsetCommanderToCenter = center - Commander.Unit.Position;
            }
            else NewCommander = false;
        }

        public void Reset()
        {
            NewCommander = false;
            Commander = null;
            OffsetsUnitToCenter.Clear();
            OffsetCommanderToCenter = Vector3.zero;
        }

        public bool NewCommander { get; private set; } = false;
        public CommandUnit Commander { get; private set; }
        public int CommanderIndex { get; private set; }
        public Vector3 Center { get => Commander.Unit.Position + OffsetCommanderToCenter; }
        public Vector3 OffsetCommanderToCenter { get; private set; }
        public List<Vector3> OffsetsUnitToCenter { get; private set; } = new List<Vector3>();
    }
}

