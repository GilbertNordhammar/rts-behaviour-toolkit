using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBTUnitSelector : MonoBehaviour
{
    [SerializeField]
    Material _material;

    Vector2 _selectBoxStartCorner;
    struct SelectBox
    {
        public Vector2 lowerLeft, upperRight;
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
            var selectBox = CalcSelectBox(_selectBoxStartCorner, Input.mousePosition);
            DrawSelectonBox(selectBox);

        }
    }

    SelectBox CalcSelectBox(Vector2 startCorner, Vector2 endCorner)
    {
        var selectBox = new SelectBox();
        if (endCorner.x > startCorner.x)
        {
            if (endCorner.y > startCorner.y)
            {
                selectBox.lowerLeft = startCorner;
                selectBox.upperRight = endCorner;
            }
            else
            {
                float deltaY = Mathf.Abs(endCorner.y - startCorner.y);
                selectBox.lowerLeft = startCorner - new Vector2(0, deltaY);
                selectBox.upperRight = endCorner + new Vector2(0, deltaY);
            }
        }
        else
        {
            if (endCorner.y > startCorner.y)
            {
                float deltaY = Mathf.Abs(endCorner.y - startCorner.y);
                selectBox.lowerLeft = endCorner - new Vector2(0, deltaY);
                selectBox.upperRight = startCorner + new Vector2(0, deltaY);
            }
            else
            {
                selectBox.lowerLeft = endCorner;
                selectBox.upperRight = startCorner;
            }
        }

        return selectBox;
    }

    void DrawSelectonBox(SelectBox selectBox)
    {
        if (!_material)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }

        float x = selectBox.lowerLeft.x / Screen.width;
        float y = selectBox.lowerLeft.y / Screen.height;
        var size = selectBox.upperRight - selectBox.lowerLeft;
        size.x /= Screen.width;
        size.y /= Screen.height;

        GL.PushMatrix();
        _material.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);

        GL.Vertex3(x, y, 0);
        GL.Vertex3(x, y + size.y, 0);
        GL.Vertex3(x + size.x, y + size.y, 0);
        GL.Vertex3(x + size.x, y, 0);

        GL.End();
        GL.PopMatrix();
    }
}

