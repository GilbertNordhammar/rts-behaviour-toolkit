using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    public partial class RBTUnitSelector
    {
        struct SelectBox
        {
            public Vector2 LowerLeft { get; private set; }
            public Vector2 UpperRight { get; private set; }

            public SelectBox(Vector2 startCorner, Vector2 endCorner)
            {
                if (endCorner.x > startCorner.x)
                {
                    if (endCorner.y > startCorner.y)
                    {
                        LowerLeft = startCorner;
                        UpperRight = endCorner;
                    }
                    else
                    {
                        float deltaY = Mathf.Abs(endCorner.y - startCorner.y);
                        LowerLeft = startCorner - new Vector2(0, deltaY);
                        UpperRight = endCorner + new Vector2(0, deltaY);
                    }
                }
                else
                {
                    if (endCorner.y > startCorner.y)
                    {
                        float deltaY = Mathf.Abs(endCorner.y - startCorner.y);
                        LowerLeft = endCorner - new Vector2(0, deltaY);
                        UpperRight = startCorner + new Vector2(0, deltaY);
                    }
                    else
                    {
                        LowerLeft = endCorner;
                        UpperRight = startCorner;
                    }
                }
            }

            public void Draw(Material material)
            {
                float x = LowerLeft.x / Screen.width;
                float y = LowerLeft.y / Screen.height;
                var size = UpperRight - LowerLeft;
                size.x /= Screen.width;
                size.y /= Screen.height;

                GL.PushMatrix();
                material.SetPass(0);
                GL.LoadOrtho();
                GL.Begin(GL.QUADS);

                GL.Vertex3(x, y, 0);
                GL.Vertex3(x, y + size.y, 0);
                GL.Vertex3(x + size.x, y + size.y, 0);
                GL.Vertex3(x + size.x, y, 0);

                GL.End();
                GL.PopMatrix();
            }

            public bool IsWithinBox(Vector2 screenPos)
            {
                return screenPos.x >= LowerLeft.x && screenPos.y >= LowerLeft.x
                && screenPos.x <= UpperRight.x && screenPos.y < UpperRight.y;
            }
        }
    }
}

