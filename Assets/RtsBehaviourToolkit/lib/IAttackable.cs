using UnityEngine;

namespace RtsBehaviourToolkit
{
    public interface IAttackable
    {
        int Health { get; set; }
        int MaximumHealth { get; set; }
        bool Alive { get; }
        Vector3 Position { get; set; }
        GameObject GameObject { get; }
        Team Team { get; }
    }
}

