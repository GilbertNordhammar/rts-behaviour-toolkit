using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public class RBTUnitCommander : MonoBehaviour
    {
        // Unity editor
        [SerializeField]
        LayerMask _walkable;

        [SerializeField]
        LayerMask _blockingTerrain;

        // Public
        public static RBTUnitCommander Instance { get; private set; }

        public struct CommandGivenEvent
        {
            public CommandGivenEvent(RBTUnitCommander sender, Vector3 position, List<RBTUnit> units)
            {
                Sender = sender;
                Position = position;
                Group = new CommandGroup(units);
            }

            public readonly RBTUnitCommander Sender;
            public readonly Vector3 Position;
            public readonly CommandGroup Group;

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
        IEnumerator _currentCommand;
        List<RBTUnit> _selectedUnits = new List<RBTUnit>();

        void HandleSelectionEnd(RBTUnitSelector.SelectionEndEvent evnt)
        {
            _selectedUnits = evnt.selectedUnits;
        }

        IEnumerator CommandUnits(Vector3 mousePosition, List<RBTUnit> units)
        {
            var ray = Camera.main.ScreenPointToRay(mousePosition);
            var mask = _walkable | _blockingTerrain;

            RaycastHit hit = new RaycastHit();
            bool hasHit = false;
            while (!hasHit)
            {
                hasHit = Physics.Raycast(ray, out hit, 100f, mask);
                yield return new WaitForFixedUpdate();
            };

            var hitLayer = hit.collider.gameObject.layer;
            var isWalkable = _walkable == (_walkable | (1 << hitLayer));
            if (isWalkable)
                _onCommandGiven.Invoke(new CommandGivenEvent(this, hit.point, units));
            _currentCommand = null;
        }

        RBTUnitCommander()
        {
            if (!Instance)
                Instance = this;
        }

        ~RBTUnitCommander()
        {
            if (Instance == this)
                Instance = null;
        }

        // Unity functions
        void OnValidate()
        {
            if (Instance && Instance != this)
            {
                Debug.LogWarning($"RBTUnitCommander on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(this);
            }
        }

        void Awake()
        {
            if (_walkable == 0)
                Debug.LogWarning("Units can't be commanded since 'Walkable' is set to 'Nothing'");
        }

        void OnEnable()
        {
            if (RBTUnitSelector.Instance)
                RBTUnitSelector.Instance.OnSelectionEnd += HandleSelectionEnd;
        }

        void OnDisable()
        {
            if (RBTUnitSelector.Instance)
                RBTUnitSelector.Instance.OnSelectionEnd -= HandleSelectionEnd;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1) && _currentCommand == null && _selectedUnits.Count > 0)
            {
                _currentCommand = CommandUnits(Input.mousePosition, _selectedUnits);
                StartCoroutine(_currentCommand);
            }
        }
    }
}