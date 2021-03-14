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
            public CommandGivenEvent(RBTUnitCommander sender, Vector3 position)
            {
                this.sender = sender;
                this.position = position;
            }

            public readonly RBTUnitCommander sender;
            public readonly Vector3 position;
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
        bool _generatingCommandPosition;
        Vector3 _commandMousePosition;

        void HandleSelectionEnd(RBTUnitSelector.SelectionEndEvent evnt)
        {
            Debug.Log("Units selected: " + evnt.selectedUnits.Count);
        }

        void StartGeneratingCommandPositon()
        {
            _commandMousePosition = Input.mousePosition;
            _generatingCommandPosition = true;
        }

        void GenerateCommandPosition()
        {
            var ray = Camera.main.ScreenPointToRay(_commandMousePosition);
            var mask = LayerMask.GetMask("RBT Floor");
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, mask))
            {
                _onCommandGiven.Invoke(new CommandGivenEvent(this, hit.point));
                _generatingCommandPosition = false;
            }
        }

        // Unity functions
        void OnValidate()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTUnitCommander on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(this);
            }
            else Instance = this;
        }

        void Awake()
        {
            Instance = this;
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
            if (Input.GetMouseButtonDown(1) && !_generatingCommandPosition)
                StartGeneratingCommandPositon();
        }

        void FixedUpdate()
        {
            if (_generatingCommandPosition)
                GenerateCommandPosition();
        }
    }
}