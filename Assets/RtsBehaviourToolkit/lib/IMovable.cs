using UnityEngine;

namespace RtsBehaviourToolkit
{
    public interface IMovable
    {
        Vector3 Velocity { get; }
        Vector3 Position { get; set; }
        GameObject GameObject { get; }
    }
}

