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

        public enum GizmosDrawMode
        {
            Solid, Wire
        }

        public void DrawGizmos(GizmosDrawMode drawMode, Color color)
        {
            var originalColor = Gizmos.color;
            Gizmos.color = color;

            foreach (var cellIndex in _indexToCell.Keys)
            {
                var bounds = new Vector3(1, 1, 1);
                if (drawMode == GizmosDrawMode.Solid)
                    Gizmos.DrawCube(cellIndex + bounds / 2, bounds);
                else
                    Gizmos.DrawWireCube(cellIndex + bounds / 2, bounds);
            }
            Gizmos.color = originalColor;
        }

        public void Add(RBTUnit unit)
        {
            if (_unitToCells.ContainsKey(unit)) return;
            else _unitToCells.Add(unit, new List<CellNode>());

            var cellIndices = CalcOccupiedCells(unit);
            foreach (var cellIndex in cellIndices)
            {
                var newHead = new CellNode() { unit = unit, cellIndex = cellIndex };
                if (_indexToCell.ContainsKey(cellIndex))
                {
                    var prevHead = _indexToCell[cellIndex];
                    prevHead.previous = newHead;
                    newHead.next = prevHead;
                }

                _indexToCell[cellIndex] = newHead;
                _unitToCells[unit].Add(newHead);
            }
        }

        public void Remove(RBTUnit unit)
        {
            var cellNodes = _unitToCells[unit];

            foreach (var node in cellNodes)
            {
                var previous = node.previous;
                var next = node.next;

                if (previous == null && next == null)
                {
                    _indexToCell.Remove(node.cellIndex);

                }
                else
                {
                    if (previous != null)
                        previous.next = next;
                    if (next != null)
                        next.previous = previous;
                }
            }

            _unitToCells.Remove(unit);
        }

        public void Update(RBTUnit unit)
        {
            Remove(unit);
            Add(unit);
        }

        public List<RBTUnit> FindNear(Vector3 position, Vector3 bounds)
        {
            var minCell = GetCellIndex(position - bounds);
            var maxCell = GetCellIndex(position + bounds);
            var nearbyUnits = new List<RBTUnit>();
            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    for (int z = minCell.z; z <= maxCell.z; z++)
                    {
                        CellNode cellNode;
                        _indexToCell.TryGetValue(new Vector3Int(x, y, z), out cellNode);
                        while (cellNode != null)
                        {
                            nearbyUnits.Add(cellNode.unit);
                            cellNode = cellNode.next;
                        }
                    }
                }
            }

            return nearbyUnits;
        }

        public Vector3Int[] GetOccupiedCells(RBTUnit unit)
        {
            return _unitToCells[unit].Select(node => node.cellIndex).ToArray();
        }

        // Private
        Dictionary<Vector3Int, CellNode> _indexToCell = new Dictionary<Vector3Int, CellNode>();
        Dictionary<RBTUnit, List<CellNode>> _unitToCells = new Dictionary<RBTUnit, List<CellNode>>();

        class CellNode
        {
            public RBTUnit unit;
            public CellNode previous;
            public CellNode next;
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

        class OccupiedCells
        {
            public void Add(Vector3Int cell)
            {
                if (!rowToCells.ContainsKey(cell.z))
                    rowToCells.Add(cell.z, new HashSet<Vector3Int>());

                var prevCount = rowToCells[cell.z].Count;
                rowToCells[cell.z].Add(cell);
                if (rowToCells[cell.z].Count > prevCount)
                    Count++;

                if (rowEdgeValues.ContainsKey(cell.z))
                {
                    var edgeValues = rowEdgeValues[cell.z];
                    if (cell.x < edgeValues.Item1)
                        rowEdgeValues[cell.z] = new Tuple<int, int>(cell.x, edgeValues.Item2);
                    else if (cell.x > edgeValues.Item2)
                        rowEdgeValues[cell.z] = new Tuple<int, int>(edgeValues.Item1, cell.x);
                }
                else
                {
                    rowEdgeValues.Add(cell.z, new Tuple<int, int>(cell.x, cell.x));
                }
            }

            public int Count { get; private set; } = 0;
            public Dictionary<int, HashSet<Vector3Int>> rowToCells = new Dictionary<int, HashSet<Vector3Int>>();
            public Dictionary<int, Tuple<int, int>> rowEdgeValues = new Dictionary<int, Tuple<int, int>>();
        }

        List<Vector3Int> CalcOccupiedCells(RBTUnit unit)
        {
            var boundsCorners = unit.Bounds.Corners;
            // var cellIndices = new HashSet<Vector3Int>();
            var occupiedCells = new OccupiedCells();

            for (int i = 0; i < 4; i++)
            {
                var startCorner = boundsCorners[i % 4];
                var endCorner = boundsCorners[(i + 1) % 4];

                var startCell = GetCellIndex(startCorner);
                var endCell = GetCellIndex(endCorner);

                occupiedCells.Add(startCell);
                if (startCell == endCell) continue;
                occupiedCells.Add(endCell);

                var rayDir = (endCorner - startCorner).normalized;

                var step_dzdx = Mathf.Abs(rayDir.z / (rayDir.x + 0.0001f));
                var step_dxdz = Mathf.Abs(rayDir.x / (rayDir.z + 0.0001f));

                var rayStepX = Mathf.Sqrt(1 + Mathf.Pow(step_dzdx, 2));
                var rayStepZ = Mathf.Sqrt(1 + Mathf.Pow(step_dxdz, 2));

                int stepX, stepZ;
                float initialAdjustmentX, initialAdjustmentZ;

                if (rayDir.x < 0)
                {
                    stepX = -1;
                    initialAdjustmentX = startCorner.x - startCell.x;
                }
                else
                {
                    stepX = 1;
                    initialAdjustmentX = (startCell.x + 1) - startCorner.x;
                }

                if (rayDir.z < 0)
                {
                    stepZ = -1;
                    initialAdjustmentZ = startCorner.z - startCell.z;
                }
                else
                {
                    stepZ = 1;
                    initialAdjustmentZ = (startCell.z + 1) - startCorner.z;
                }

                step_dxdz *= stepX;
                step_dzdx *= stepZ;

                var rayLengthX = initialAdjustmentX * rayStepX;
                var rayLengthZ = initialAdjustmentZ * rayStepZ;

                float zAlongX = startCorner.z + step_dzdx * initialAdjustmentX;
                float xAlongZ = startCorner.x + step_dxdz * initialAdjustmentZ;

                var maxRayLength = (endCorner - startCorner).magnitude;

                var currentCellX = GetCellIndex(new Vector3(startCorner.x + stepX * initialAdjustmentX, startCorner.y, zAlongX));
                if (rayLengthX < maxRayLength)
                    occupiedCells.Add(currentCellX);

                var currentCellZ = GetCellIndex(new Vector3(xAlongZ, startCorner.y, startCorner.z + stepZ * initialAdjustmentZ));
                if (rayLengthZ < maxRayLength)
                    occupiedCells.Add(currentCellZ);

                bool endCellReached = false;
                while (!endCellReached)
                {
                    bool commonAdjacentCell = currentCellZ.z - currentCellX.z == 1 && currentCellX.x - currentCellZ.x == 1;
                    if (commonAdjacentCell)
                        occupiedCells.Add(new Vector3Int(currentCellZ.x, startCell.y, currentCellX.z));

                    if (rayLengthX < rayLengthZ)
                    {
                        rayLengthX += rayStepX;
                        if (rayLengthX < maxRayLength)
                        {
                            zAlongX += step_dzdx;
                            currentCellX.x += stepX;
                            currentCellX.z = Mathf.FloorToInt(zAlongX);
                            occupiedCells.Add(currentCellX);
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
                            occupiedCells.Add(currentCellZ);
                        }
                    }

                    endCellReached = rayLengthX >= maxRayLength && rayLengthZ >= maxRayLength;
                }
            }

            foreach (var entry in occupiedCells.rowEdgeValues)
            {
                var valuePair = entry.Value;
                var smallestX = valuePair.Item1;
                var biggestX = valuePair.Item2;
                var z = entry.Key;
                var y = Mathf.FloorToInt(boundsCorners[0].y);
                for (int x = smallestX + 1; x < biggestX; x++)
                {
                    occupiedCells.Add(new Vector3Int(x, y, z));
                }
            }

            int topCount = occupiedCells.Count;
            var allCells = new List<Vector3Int>();
            allCells.Capacity = topCount;
            foreach (var row in occupiedCells.rowToCells.Values)
            {
                foreach (var cell in row)
                {
                    allCells.Add(cell);
                }
            }
            var diffY = GetCellIndex(boundsCorners[0]).y - GetCellIndex(boundsCorners[4]).y;

            for (int y = 1; y <= diffY; y++)
            {
                for (int i = 0; i < topCount; i++)
                    allCells.Add(new Vector3Int(allCells[i].x, allCells[i].y - y, allCells[i].z));
            }

            return allCells;
        }
    }
}