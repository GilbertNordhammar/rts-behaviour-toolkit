using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [System.Serializable]
    public abstract class UnitBehaviour : ScriptableObject
    {
        public virtual void OnCommandGroupCreated(CommandGroup commandGroup) { }
        public virtual void OnUpdate(CommandGroup group) { }
        public virtual void DrawGizmos(CommandGroup group) { }
    }
}

