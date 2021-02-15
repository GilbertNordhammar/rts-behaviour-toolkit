using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
[System.Serializable]
public class FieldOfView : ScriptableObject
{
    [SerializeField] [Range(0f, 360f)] private float _angle = 270f;
    [SerializeField] private float _radius = 2f;
    [SerializeField] private LayerMask _viewableMask;
    public bool VisualizeFieldOfView;

    public float Angle { get { return _angle; } set { if (value == 0) { value = 0; } _angle = value; } }
    public float Radius { get { return _radius; } set { if (value == 0) { value = 0; } _radius = value; } }
    public LayerMask ViewableMask { get { return _viewableMask; } }

    public List<GameObject> GetVisibleObjects(GameObject source)
    {
        var visibleUnits = new List<GameObject>();
        var collidersInSphere = Physics.OverlapSphere(source.transform.position, _radius, _viewableMask.value);

        foreach (var collider in collidersInSphere)
        {
            var closestPoint = collider.ClosestPoint(source.transform.position);
            var deltaDistance = Vector3.Distance(source.transform.position, closestPoint);
            var angleBetween = Vector3.Angle(source.transform.forward, source.transform.rotation * source.transform.worldToLocalMatrix.MultiplyPoint(closestPoint));

            if (deltaDistance < _radius && angleBetween < _angle / 2)
            {
                visibleUnits.Add(collider.gameObject);
            }
        }

        Vector3 leftmostDirection = Quaternion.AngleAxis(-_angle / 2, source.transform.up) * source.transform.forward;
        Vector3 rightmostDirection = Quaternion.AngleAxis(_angle / 2, source.transform.up) * source.transform.forward;

        var collidersHitAtLeftMost = Physics.RaycastAll(source.transform.position, leftmostDirection, _radius, _viewableMask.value).Select(x => x.collider).ToArray();
        var collidersHitAtRightMost = Physics.RaycastAll(source.transform.position, rightmostDirection, _radius, _viewableMask.value).Select(x => x.collider).ToArray();
        var collidersHitAtExtremeDirections = collidersHitAtLeftMost.Union(collidersHitAtRightMost);

        foreach (var collider in collidersHitAtExtremeDirections)
        {
            visibleUnits.Add(collider.gameObject);
        }

        return visibleUnits;
    }
}
