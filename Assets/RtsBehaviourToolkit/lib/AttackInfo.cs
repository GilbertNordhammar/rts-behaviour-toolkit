using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [System.Serializable]
    public class AttackInfo
    {
        [field: SerializeField]
        [field: Min(0)]
        public int Damage { get; private set; } = 1;

        [field: SerializeField]
        [field: Min(0)]
        public float Frequency { get; set; } = 1f;

        [field: SerializeField]
        [field: Min(0)]
        public float Range { get; set; } = 1f;

        [field: SerializeField]
        [field: Min(0)]
        public float DamageDelay { get; set; } = 1f;
    }
}

