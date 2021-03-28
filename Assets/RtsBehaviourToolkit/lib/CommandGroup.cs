using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace RtsBehaviourToolkit
{
    public class CommandGroup
    {
        public CommandGroup(List<CommandUnit> units, float subgroupDistance)
        {
            Units = units;

            foreach (var unit in units)
                unit.Unit.AssignCommandGroup(Id);
            UpdateSubgroups(subgroupDistance);
        }

        public string Id { get; } = System.Guid.NewGuid().ToString();
        public List<CommandUnit> Units { get; }
        public List<CommandSubgroup> Subgroups { get; }
        public bool Finished
        {
            get
            {
                bool hasFinished = true;
                foreach (var unit in Units)
                {
                    hasFinished = unit.Finished;
                    if (!hasFinished) break;
                }
                return hasFinished;
            }
        }

        public void UpdateSubgroups(float subgroupMaxDistance)
        {
            /* 
                TODO: Det funkar inte att endast kolla avstånd mellan subgrupper en gång
                då det betyder att enheternas positoner i listan behöver motsvara ordningen 
                de befinner sig i världen. 

                Strategi 1 (dyr, O(n) = n! i värsta fall):
                 - För varje enhet, jänför med enheter i subgrupper initiala listor av enheter som kommer utgöra samma subgrupp (dvs befinner sig närmare än subgroupDistance)
                 - Iterera över listorna och merga två listor ifall åtminstone två enheter i respektive grupp befinner sig närmare än subgroupDistance.
                 - Slut iterera ifall det bara finns en lista kvar eller ifall senaste iterationen innebar 0 mergar
                Strategi 2 (Bästa: O(n) = n, Värsta: O(n) = n!)
                 - För varje enhet (enhet1), jämför avstånd med övriga enheter (enhet2). Om det är mindre än subgroupMaxDistance, gör ett av följande:
                    1. Ifall enhet2 befinner sig i en subgrupp, lägg till enhet1 i den subgruppen
                    2. Ifall enhet2 saknar subgrupp, skapa en ny subgrupp och lägg till både enhet1 och enhet2.
                       Ta därefter bort enhet2 från iterationslistan (eller hoppa över enhet2 när det är dess tur)
                 - Skapa ny subgrupp ifall 
                 - Börja jämföra med enheter i subgrupper (mer sannolikt att hitta en som befinner sig tillräckligt nära)
                 - Iterera tills att alla enheter har blivit tilldelade en subgrupp (gör en counter för detta och jämför med Units.length)
            */
            // var subgroups = new List<CommandSubgroup>();
            // foreach (var unit in Units)
            // {
            //     CommandSubgroup newGroup = subgroups.Where((sg) =>
            //     {
            //         // foreach (var unit in sg.Units)
            //         // {

            //         // }
            //         return null;
            //     }).FirstOrDefault();
            //     if (newGroup == null)
            //     {
            //         newGroup = new CommandSubgroup(new List<CommandUnit>() { unit });
            //         subgroups.Add(newGroup);
            //     }
            //     else newGroup.Units.Add(unit);
            // }

            // TODO: Assign commanders
        }
    }

    public class CommandSubgroup
    {
        public CommandSubgroup(List<CommandUnit> units)
        {
            Units = units;
        }
        public CommandUnit Commander { get => Units[CommanderIndex]; }
        public int CommanderIndex { get; set; } = 0;
        public List<CommandUnit> Units;
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

