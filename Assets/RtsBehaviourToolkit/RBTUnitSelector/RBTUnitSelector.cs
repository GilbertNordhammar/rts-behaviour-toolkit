using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class RBTUnitSelector : MonoBehaviour
{
    [SerializeField]
    Material _material;

    Vector2 _selectBoxStartCorner;

    void Awake()
    {
        if (!_material)
            Debug.LogError("Please Assign a material on the inspector");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _selectBoxStartCorner = Input.mousePosition;
        }
    }

    void OnPostRender()
    {
        if (Input.GetMouseButton(0))
        {
            var selectBox = new SelectBox(_selectBoxStartCorner, Input.mousePosition);
            if (_material)
                selectBox.Draw(_material);
        }
    }
}

