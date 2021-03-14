using System.Collections;
using System.Collections.Generic;

namespace RtsBehaviourToolkit
{
    public class CommandGroup
    {
        public CommandGroup(List<RBTUnit> units)
        {
            Units = units;
            Id = System.Guid.NewGuid().ToString();
        }

        public readonly string Id;
        public readonly List<RBTUnit> Units;
    }
}

