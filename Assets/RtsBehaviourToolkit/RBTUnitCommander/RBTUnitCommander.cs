using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public class RBTUnitCommander : MonoBehaviour
    {
        // Public
        public static RBTUnitCommander Instance { get; private set; }

        public struct CommandGivenEvent
        {
            public CommandGivenEvent(RBTUnitCommander sender, Vector3 position, List<RBTUnit> units)
            {
                Sender = sender;
                Position = position;
                Units = units;
            }

            public readonly RBTUnitCommander Sender;
            public readonly Vector3 Position;
            public readonly List<RBTUnit> Units;
        }

        public event Action<CommandGivenEvent> OnCommandGiven
        {
            add
            {
                lock (_onCommandGivenLock)
                {
                    _onCommandGiven += value;
                }
            }
            remove
            {
                lock (_onCommandGivenLock)
                {
                    _onCommandGiven -= value;
                }
            }
        }

        // Private
        event Action<CommandGivenEvent> _onCommandGiven = delegate { };
        readonly object _onCommandGivenLock = new object();
        List<RBTUnit> _selectedUnits = new List<RBTUnit>();

        void HandleSelectionEnd(RBTUnitSelector.SelectionEndEvent evnt)
        {
            _selectedUnits = evnt.selectedUnits;
        }

        void CommandUnits(Vector3 mousePosition, List<RBTUnit> units)
        {
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            var mask = RBTConfig.WalkableMask | RBTConfig.TerrainMask;
            RaycastHit hit = new RaycastHit();
            bool clickOnWalkableSurface = Physics.Raycast(ray, out hit, 100f, mask);

            if (clickOnWalkableSurface)
            {
                var hitLayer = hit.collider.gameObject.layer;
                var isWalkable = RBTConfig.WalkableMask == (RBTConfig.WalkableMask | (1 << hitLayer));
                if (isWalkable)
                    _onCommandGiven.Invoke(new CommandGivenEvent(this, hit.point, units));
            }
        }

        // Unity functions
        void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitCommander on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                Destroy(this);
                return;
            }
            else Instance = this;
        }

        void Start()
        {
            if (RBTUnitSelector.Instance)
                RBTUnitSelector.Instance.OnSelectionEnd += HandleSelectionEnd;

            if (RBTConfig.WalkableMask == 0)
                Debug.LogWarning("Units can't be commanded since 'WalkableMask' is set to 'Nothing'");
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1) && _selectedUnits.Count > 0)
            {
                CommandUnits(Input.mousePosition, _selectedUnits);
            }
        }
    }
}