using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [System.Serializable]
    public abstract class RBTUnitBehaviour : MonoBehaviour
    {
        public virtual void OnCommandGroupCreated(CommandGroup commandGroup) { }
        public virtual void OnUpdate(CommandGroup group) { }
    }
}

