using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public class Path
    {
        public Path(Vector3[] nodes)
        {
            Nodes = nodes;
            if (nodes.Length == 0)
                Traversed = true;
        }

        public Vector3[] Nodes { get; }
        public Vector3 NextCorner
        {
            get
            {
                return Nodes[_indexNextCorner];
            }
        }

        public void Increment()
        {
            _indexNextCorner++;
            if (_indexNextCorner >= Nodes.Length)
            {
                _indexNextCorner = Nodes.Length - 1;
                Traversed = true;
            }
        }

        public int NextCornerIndex
        {
            get => _indexNextCorner;
        }

        public bool Traversed { get; private set; }

        // Private
        private int _indexNextCorner = 0;
    }
}
