using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [CreateAssetMenu(fileName = "Team", menuName = "RtsBehaviourToolkit/Team")]
    [System.Serializable]
    public class Team : ScriptableObject
    {
        // Inspector and public
        [field: SerializeField]
        public string Name { get; private set; }

        [field: SerializeField]
        public Color Color { get; private set; }

        [SerializeField]
        List<Team> _allies;

        [SerializeField]
        List<Team> _opponents;

        public IReadOnlyList<Team> Allies { get => _allies; }
        public IReadOnlyList<Team> Opponents { get => _opponents; }
    }
}

