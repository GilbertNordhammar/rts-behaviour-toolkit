using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitCommander : MonoBehaviour
{
    // Inspector fields
    [SerializeField] private GameObject _positionMarkerPrefab;
    [SerializeField] float _destinationOffsetPerUnit = 0.1f;

    // Public data
    public UnitCommander Instance { get; private set; }
    
    // Unity event functions
    private void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hitInfo = GetRaycastHitFromMainCamera();
            if(hitInfo.collider != null)
            {
                StartCoroutine(HighlightPosition(hitInfo.point));
                CommandUnitsToAdjacentPositions(UnitSelecter.SelectedUnits, hitInfo.point);
            }
        }
    }

    private void OnValidate()
    {
        if(_destinationOffsetPerUnit < 0f)
        {
            _destinationOffsetPerUnit = 0f;
        }
    }

    // Public functions

    RaycastHit GetRaycastHitFromMainCamera()
    {
        RaycastHit hitInfo;

        Ray selectRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(selectRay, out hitInfo);
        
        return hitInfo;
    }

    private void CommandUnitsToAdjacentPositions(List<Unit> unitList, Vector3 targetPosition)
    {
        foreach(var unit in unitList)
        {
            var walkingPath = new NavMeshPath();
            NavMesh.CalculatePath(unit.transform.position, targetPosition, NavMesh.AllAreas, walkingPath);
            PositionSteering.Instance.MoveUnitAlongPath(unit, walkingPath, _destinationOffsetPerUnit * unitList.Count);
        }
    }

    private IEnumerator HighlightPosition(Vector3 position)
    {
        GameObject positionHighlight = Instantiate(_positionMarkerPrefab);

        positionHighlight.transform.position = position;
        yield return new WaitForSecondsRealtime(0.2f);
        Destroy(positionHighlight);
    }
}
