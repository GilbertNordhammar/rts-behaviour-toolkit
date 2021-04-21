using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RtsBehaviourToolkit
{
    public class ZombieBehaviour : MonoBehaviour
    {
        // Inspector and public
        [SerializeField, Min(0.1f)]
        float _searchRadius = 2;

        // Private
        RBTUnit _unit;
        Vector3 _searchBounds;

        void Init()
        {
            var cathetus = Mathf.Sqrt((_searchRadius * _searchRadius) / 2);
            _searchBounds = new Vector3(cathetus, 0, cathetus);
            _unit = GetComponent<RBTUnit>();
        }

        // Unity functions
        void Start()
        {
            Init();
        }

        void FixedUpdate()
        {
            if (_unit.CommandGroupId != "" || !_unit.Alive)
                return;

            var nearbyUnits = RBTUnitBehaviourManager.UnitGrid.FindNear(_unit.Position, _searchBounds);
            var sqrRadius = _searchRadius * _searchRadius;
            foreach (var unit in nearbyUnits)
            {
                var dist = (unit.Position - _unit.Position).sqrMagnitude;
                if (dist < sqrRadius && unit.Team != _unit.Team && unit.Alive)
                {
                    RBTUnitBehaviourManager.Instance.CommandAttack(new List<RBTUnit>() { _unit }, unit);
                    break;
                }
            }
        }

        void OnValidate()
        {
            Init();
        }

        void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            var origColor = Handles.color;
            Handles.color = new Color(1, 0, 0, 0.1f);
            Handles.DrawSolidDisc(transform.position, Vector3.up, _searchRadius);
            Handles.color = origColor;
#endif
        }
    }
}

