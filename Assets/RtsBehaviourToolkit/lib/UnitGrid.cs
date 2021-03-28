using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RtsBehaviourToolkit
{
    [System.Serializable]
    public class UnitGrid
    {
        // Public

        // TODO: Find a way of having custom constructors. With written constructors, private values reset to default value 
        //       for their respective types even though they're initialized

        // public UnitGrid(Vector3 bounds, Vector3Int dimensions) : this(bounds, dimensions, Vector3.zero)
        // {
        //     Debug.Log("UnitGrid2");
        // }

        // public UnitGrid(Vector3 bounds, Vector3Int dimensions, Vector3 center)
        // {
        //     Debug.Log("UnitGrid3");
        //     Bounds = bounds;
        //     Dimensions = dimensions;
        //     Center = center;
        // }

        [field: SerializeField]
        [field: Min(1)]
        public Vector3 Bounds { get; private set; }

        [field: SerializeField]
        [field: Min(1)]
        public Vector3Int Dimensions { get; private set; }

        [field: SerializeField]
        public Vector3 Center { get; private set; }

        public void DrawGizmos()
        {
            // var cellSize = new Vector3(Bounds.x / Dimensions.x, Bounds.y / Dimensions.y, Bounds.z / Dimensions.z);
            // for (int x = 0; x < Dimensions.x; x++)
            // {
            //     for (int y = 0; y < Dimensions.y; y++)
            //     {
            //         for (int z = 0; z < Dimensions.z; z++)
            //         {
            //             var cellCenter = new Vector3(x * cellSize.x, y * cellSize.y, z * cellSize.z) + cellSize / 2;
            //             cellCenter = cellCenter - Bounds / 2 + Center;
            //             Gizmos.DrawWireCube(cellCenter, cellSize);
            //         }
            //     }
            // }

            foreach (var cell in _cells.Values)
            {
                var bounds = new Vector3(1, 1, 1);
                Gizmos.DrawWireCube(cell.cellIndex + bounds / 2, bounds);
            }

            Debug.Log(_cells.Count);
        }

        public void Add(RBTUnit unit)
        {
            var cellIndices = GetOccupiedCells(unit);
            Debug.Log("CHECK THIS OUT " + cellIndices.Length);

            int index = 0;
            foreach (var cellIndex in cellIndices)
            {
                var newHead = new Node() { unit = unit, cellIndex = cellIndex };
                if (_cells.ContainsKey(cellIndex))
                {
                    Debug.Log("duplicate " + index);
                    var head = _cells[cellIndex];
                    head.previous = newHead;
                    newHead.next = head;
                }
                _cells[cellIndex] = newHead;

                if (!_unitToCells.ContainsKey(unit))
                    _unitToCells.Add(unit, new List<Node>());
                _unitToCells[unit].Add(newHead);

                index++;
            }
        }

        Vector3Int[] GetOccupiedCells(RBTUnit unit)
        {
            var boundsCorners = unit.Bounds.Corners;
            var cellIndices = new HashSet<Vector3Int>();
            for (int i = 0; i < 4; i++) // change back to 'i < 4' later
            {
                var startCorner = boundsCorners[i % 4];
                var endCorner = boundsCorners[(i + 1) % 4];

                var startCell = GetCellIndex(startCorner);
                var endCell = GetCellIndex(endCorner);

                cellIndices.Add(startCell);
                if (startCell == endCell) break;
                cellIndices.Add(endCell);

                var rayDir = (endCorner - startCorner).normalized;

                var step_dzdx = rayDir.z / (rayDir.x + 0.0001f);
                var step_dxdz = rayDir.x / (rayDir.z + 0.0001f);

                var rayStepX = Mathf.Sqrt(1 + Mathf.Pow(step_dzdx, 2));
                var rayStepZ = Mathf.Sqrt(1 + Mathf.Pow(step_dxdz, 2));

                int stepX, stepZ;
                float intialStepX, intialStepZ;

                if (rayDir.x < 0)
                {
                    stepX = -1;
                    intialStepX = startCorner.x - startCell.x;
                }
                else
                {
                    stepX = 1;
                    intialStepX = (startCell.x + 1) - startCorner.x;
                }

                if (rayDir.z < 0)
                {
                    stepZ = -1;
                    intialStepZ = startCorner.z - startCell.z;
                }
                else
                {
                    stepZ = 1;
                    intialStepZ = (startCell.z + 1) - startCorner.z;
                }

                var maxRayLength = (endCorner - startCorner).magnitude;
                var rayLengthX = intialStepX * rayStepX;
                var rayLengthZ = intialStepZ * rayStepZ;

                float zAlongX = startCorner.z + intialStepX * step_dzdx;
                float xAlongZ = startCorner.x + intialStepZ * step_dxdz;

                var currentCellX = startCell;
                var currentCellZ = startCell;

                bool endCellReached = false;
                while (!endCellReached)
                {
                    if (rayLengthX < rayLengthZ)
                    {
                        rayLengthX += rayStepX;
                        if (rayLengthX < maxRayLength)
                        {
                            zAlongX += step_dzdx;
                            currentCellX.x += stepX;
                            currentCellX.z = Mathf.FloorToInt(zAlongX);
                            cellIndices.Add(currentCellX);
                        }
                    }
                    else
                    {
                        rayLengthZ += rayStepZ;
                        if (rayLengthZ < maxRayLength)
                        {
                            xAlongZ += step_dxdz;
                            currentCellZ.z += stepZ;
                            currentCellZ.x = Mathf.FloorToInt(xAlongZ);
                            cellIndices.Add(currentCellZ);
                        }

                    }

                    endCellReached = rayLengthX >= maxRayLength && rayLengthZ >= maxRayLength;
                }
            }

            var topCount = cellIndices.Count;
            var cellsY = GetCellIndex(boundsCorners[0]).y - GetCellIndex(boundsCorners[4]).y;
            var allCellIndices = new Vector3Int[topCount * (cellsY + 1)];
            Array.Copy(cellIndices.ToArray(), allCellIndices, topCount);

            for (int y = 1; y <= cellsY; y++)
            {
                for (int i1 = 0, i2 = topCount * y; i1 < topCount; i1++, i2++)
                    allCellIndices[i2] = new Vector3Int(allCellIndices[i1].x, allCellIndices[i1].y - y, allCellIndices[i1].z);
            }

            return allCellIndices;
        }

        public void Remove(Unit unit)
        {

        }

        public void Update()
        {

        }

        public void FindNear(Vector3 positon, Vector3 bounds)
        {

        }

        // Private
        Dictionary<Vector3Int, Node> _cells = new Dictionary<Vector3Int, Node>();
        Dictionary<RBTUnit, List<Node>> _unitToCells = new Dictionary<RBTUnit, List<Node>>();

        class Node
        {
            public RBTUnit unit;
            public Node previous;
            public Node next;
            public Vector3Int cellIndex;
        }

        Vector3Int GetCellIndex(Vector3 position)
        {
            return Vector3Int.FloorToInt(position);
        }

        string GetCellKey(Vector3Int cellIndex)
        {
            return cellIndex.ToString();
        }
    }
}