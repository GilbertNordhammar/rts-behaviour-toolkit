using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public class RBTConfig : MonoBehaviour
    {
        // Inspector
        [SerializeField]
        LayerMask _walkableMask;
        [SerializeField]
        LayerMask _terrainMask;
        [SerializeField]
        LayerMask _unitMask;

        // Unity functions
        void Awake()
        {
            if (Instance)
            {
                Debug.LogWarning($"RBTConfig on '{gameObject.name}' was destroyed as there's already one attached on '{Instance.gameObject.name}'");
                Destroy(this);
                return;
            }
            else Instance = this;
        }

        // Public
        public static RBTConfig Instance { get; private set; }
        public static LayerMask WalkableMask { get => Instance._walkableMask; }
        public static LayerMask TerrainMask { get => Instance._walkableMask; }
        public static LayerMask UnitMask { get => Instance._unitMask; }

    }
}

