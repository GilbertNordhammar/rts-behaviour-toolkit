using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace RtsBehaviourToolkit
{
    public class Path
    {
        public Path(Vector3[] nodes)
        {
            Nodes = nodes;
            if (nodes.Length == 0)
                Assert.AreEqual(nodes.Length, 0, "A path must be longer than 0");
        }

        public Vector3[] Nodes { get; }
        public Vector3 NextNode
        {
            get => Nodes[NextNodeIndex];
            set => Nodes[NextNodeIndex] = value;
        }

        public Vector3 PreviousNode
        {
            get => Nodes[PreviousNodeIndex];
            set => Nodes[PreviousNodeIndex] = value;
        }

        public void Increment()
        {
            _indexIncrement++;
        }

        public int NextNodeIndex
        {
            get => Mathf.Min(_indexIncrement, Nodes.Length - 1);
        }

        public int PreviousNodeIndex
        {
            get => Mathf.Clamp(_indexIncrement - 1, 0, Nodes.Length - 1);
        }

        public int PreviousNextNodeIndex { get; private set; } = 0;

        public Vector3 PreviousNextNode { get => Nodes[PreviousNextNodeIndex]; }

        public void UpdatePreviousNextNode()
        {
            PreviousNextNodeIndex = NextNodeIndex;
        }

        public bool Traversed { get => _indexIncrement >= Nodes.Length; }

        public static implicit operator bool(Path me)
        {
            return !object.ReferenceEquals(me, null);
        }

        // Private
        int _indexIncrement = 0;
    }
}


