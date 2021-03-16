using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [System.Serializable]
    public abstract class RBTUnitBehaviour : MonoBehaviour
    {
        public abstract void Execute(CommandGroup group);
    }
}

