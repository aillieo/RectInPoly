// -----------------------------------------------------------------------
// <copyright file="LargestRectInPolygon.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public static class LargestRectInPolygon
    {
        private const byte intersectFlag = 0b0010;
        private const byte interiorFlag = 0b0001;
        private const byte exteriorFlag = 0b0100;

        [Flags]
        public enum SubdivideMode
        {
            None = 0,
            MidPoint = 1,
            Intersection = 1 << 1,
        }

        public static bool IsClockwise(IList<Vector2> points)
        {
            float sum = 0;
            var n = points.Count;

            for (var i = 0; i < n; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % n];
                sum += (next.x - current.x) * (next.y + current.y);
            }

            return sum < 0;
        }

        public static bool IsValidPolygon(IList<Vector2> points)
        {
            var n = points.Count;

            if (n < 3)
            {
                return false;
            }

            // 检查是否存在自交
            for (var i = 0; i < n; i++)
            {
                Vector2 p0 = points[i];
                Vector2 p1 = points[(i + 1) % n];

                for (var j = i + 1; j < n; j++)
                {
                    Vector2 q0 = points[j];
                    Vector2 q1 = points[(j + 1) % n];

                    // 确保不检查相邻或相同的边
                    if (Mathf.Abs(i - j) == 1 || Mathf.Abs(i - j) == n - 1)
                    {
                        continue;
                    }

                    if (LinesIntersect(p0, p1, q0, q1))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static Rect Find(IList<Vector2> polygon)
        {
            return Find(polygon, SubdivideMode.MidPoint | SubdivideMode.Intersection, 2);
        }

        public static Rect Find(IList<Vector2> polygon, SubdivideMode subdivideMode, int subdivisions)
        {
            CalculateGrids(polygon, subdivideMode, subdivisions, out var xCoords, out var yCoords);
            PolygonGridsIntersection(polygon, xCoords, yCoords, out var graph);
            CalculateLargestInteriorRectangle(xCoords, yCoords, graph, out Rect rect);
            return rect;
        }

        public static Rect Find(IList<Vector2> polygon, out float[] xCoords, out float[] yCoords, out byte[,] graph)
        {
            return Find(polygon, SubdivideMode.MidPoint | SubdivideMode.Intersection, 2, out xCoords, out yCoords, out graph);
        }

        public static Rect Find(IList<Vector2> polygon, SubdivideMode subdivideMode, int subdivisions, out float[] xCoords, out float[] yCoords, out byte[,] graph)
        {
            CalculateGrids(polygon, subdivideMode, subdivisions, out xCoords, out yCoords);
            PolygonGridsIntersection(polygon, xCoords, yCoords, out graph);
            CalculateLargestInteriorRectangle(xCoords, yCoords, graph, out Rect rect);

            var width = graph.GetLength(0);
            var height = graph.GetLength(1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (graph[x, y] != interiorFlag)
                    {
                        graph[x, y] = 0;
                    }
                }
            }

            return rect;
        }

        private static void CalculateGrids(IList<Vector2> polygon, SubdivideMode subdivideMode, int subdivisions, out float[] xCoords, out float[] yCoords)
        {
            var pointCount = polygon.Count;

            var xCoordsList = new List<float>();
            var yCoordsList = new List<float>();
            foreach (var point in polygon)
            {
                xCoordsList.Add(point.x);
                yCoordsList.Add(point.y);
            }

            xCoordsList.Sort();
            yCoordsList.Sort();

            RemoveDuplicatedCoordFromSortedList(xCoordsList);
            RemoveDuplicatedCoordFromSortedList(yCoordsList);

            // 根据需要更进一步细分网格
            for (var n = 0; n < subdivisions; n++)
            {
                if ((subdivideMode & SubdivideMode.MidPoint) != 0)
                {
                    // midpoints
                    for (var l = xCoordsList.Count - 1; l > 0; l--)
                    {
                        var mid = (xCoordsList[l] + xCoordsList[l - 1]) * 0.5f;
                        xCoordsList.Insert(l, mid);
                    }

                    for (var l = yCoordsList.Count - 1; l > 0; l--)
                    {
                        var mid = (yCoordsList[l] + yCoordsList[l - 1]) * 0.5f;
                        yCoordsList.Insert(l, mid);
                    }
                }

                if ((subdivideMode & SubdivideMode.Intersection) != 0)
                {
                    // project Points
                    // 遍历顶点流 每个edge 与x 和 y 判断是否交点 如果有 则在列表中加入交点
                    var newXCoords = new List<float>();
                    var newYCoords = new List<float>();
                    for (var i = 0; i < pointCount; i++)
                    {
                        var edgeStart = polygon[i];
                        var edgeEnd = polygon[(i + 1) % pointCount];
                        foreach (var x in xCoordsList)
                        {
                            if ((x - edgeEnd.x) * (x - edgeStart.x) < 0)
                            {
                                var newY = Mathf.Lerp(edgeStart.y, edgeEnd.y, (edgeEnd.x - x) / (edgeEnd.x - edgeStart.x));
                                newYCoords.Add(newY);
                            }
                        }

                        foreach (var y in yCoordsList)
                        {
                            if ((y - edgeEnd.y) * (y - edgeStart.y) < 0)
                            {
                                var newX = Mathf.Lerp(edgeStart.x, edgeEnd.x, (edgeEnd.y - y) / (edgeEnd.y - edgeStart.y));
                                newXCoords.Add(newX);
                            }
                        }
                    }

                    xCoordsList.AddRange(newXCoords);
                    yCoordsList.AddRange(newYCoords);
                    xCoordsList.Sort();
                    yCoordsList.Sort();

                    RemoveDuplicatedCoordFromSortedList(xCoordsList);
                    RemoveDuplicatedCoordFromSortedList(yCoordsList);
                }
            }

            xCoords = xCoordsList.ToArray();
            yCoords = yCoordsList.ToArray();
        }

        private static void RemoveDuplicatedCoordFromSortedList(List<float> list)
        {
            for (var i = list.Count - 1; i >= 1; i--)
            {
                if (Mathf.Abs(list[i] - list[i - 1]) < float.Epsilon)
                {
                    list.RemoveAt(i);
                }
            }
        }

        private static void PolygonGridsIntersection(IList<Vector2> polygon, float[] xGrids, float[] yGrids, out byte[,] graph)
        {
            graph = new byte[xGrids.Length - 1, yGrids.Length - 1];

            // 遍历polygon的每条边
            for (var i = 0; i < polygon.Count; i++)
            {
                Vector2 p0 = polygon[i];
                Vector2 p1 = polygon[(i + 1) % polygon.Count];
                var xStart = p0.x;
                var xEnd = p1.x;
                var yStart = p0.y;
                var yEnd = p1.y;

                var edgeType = 0;
                if (xStart < xEnd && yEnd > yStart)
                {
                    edgeType = 1;
                }
                else if (xStart > xEnd && yEnd > yStart)
                {
                    edgeType = 2;
                }
                else if (xStart > xEnd && yEnd < yStart)
                {
                    edgeType = 3;
                }
                else if (xStart < xEnd && yEnd < yStart)
                {
                    edgeType = 4;
                }
                else if (xStart == xEnd && yEnd > yStart)
                {
                    edgeType = 5;
                }
                else if (xStart > xEnd && yEnd == yStart)
                {
                    edgeType = 6;
                }
                else if (xStart == xEnd && yEnd < yStart)
                {
                    edgeType = 7;
                }
                else if (xStart < xEnd && yEnd == yStart)
                {
                    edgeType = 8;
                }

                Vector2 v1 = -Vector2.Perpendicular(p1 - p0);
                v1.Normalize();

                // 遍历每个网格单元
                for (var x = 0; x < xGrids.Length - 1; x++)
                {
                    for (var y = 0; y < yGrids.Length - 1; y++)
                    {
                        // 获取网格单元的四个顶点
                        var gridBottomLeft = new Vector2(xGrids[x], yGrids[y]);
                        var gridTopLeft = new Vector2(xGrids[x], yGrids[y + 1]);
                        var gridTopRight = new Vector2(xGrids[x + 1], yGrids[y + 1]);
                        var gridBottomRight = new Vector2(xGrids[x + 1], yGrids[y]);

                        var edgeBottomLeft = Vector2.Min(p0, p1);
                        var edgeTopRight = Vector2.Max(p0, p1);

                        var isHorizontal = p0.y == p1.y;
                        var isVertical = p0.x == p1.x;

                        if (edgeBottomLeft.x <= gridBottomLeft.x && edgeBottomLeft.y <= gridBottomLeft.y
                            && edgeTopRight.x >= gridTopRight.x && edgeTopRight.y >= gridTopRight.y)
                        {
                            // 格子的包围盒小于edge的包围盒 必相交
                        }
                        else if (isHorizontal &&
                            edgeBottomLeft.x <= gridBottomLeft.x && edgeTopRight.x >= gridTopRight.x
                            && (edgeBottomLeft.y == gridBottomLeft.y || edgeTopRight.y == gridTopRight.y))
                        {
                            // edge是格子的上下两边之一
                        }
                        else if (isVertical &&
                            (edgeBottomLeft.x == gridBottomLeft.x || edgeTopRight.x == gridTopRight.x)
                            && edgeBottomLeft.y <= gridBottomLeft.y && edgeTopRight.y >= gridTopRight.y)
                        {
                            // edge是格子的左右两边之一
                        }
                        else
                        {
                            continue;
                        }

                        Vector2 criticalVert = default;
                        switch (edgeType)
                        {
                            case 1:
                            case 5:
                                criticalVert = gridBottomRight;
                                break;
                            case 2:
                            case 6:
                                criticalVert = gridTopRight;
                                break;

                            case 3:
                            case 7:
                                criticalVert = gridTopLeft;
                                break;
                            case 4:
                            case 8:
                                criticalVert = gridBottomLeft;
                                break;
                        }

                        Vector2 v2 = GetPerpendicularVector(p0, p1, criticalVert);

                        if (Vector2.Dot(v1, v2) <= 0)
                        {
                            graph[x, y] |= intersectFlag;
                        }
                        else
                        {
                            graph[x, y] |= interiorFlag;
                        }
                    }
                }
            }

            CalculateInteriorGrids(graph);
        }

        private static void CalculateInteriorGrids(byte[,] graph)
        {
            var width = graph.GetLength(0);
            var height = graph.GetLength(1);

            // 1. 标记所有边界的未检测格子为外部区域
            var intersectOrInterior = intersectFlag | interiorFlag;
            var anyFlag = intersectFlag | interiorFlag | exteriorFlag;

            // 1.1 遍历上下边界
            for (var x = 0; x < width; x++)
            {
                if ((graph[x, 0] & intersectOrInterior) == 0)
                {
                    graph[x, 0] |= exteriorFlag;
                }

                if ((graph[x, height - 1] & intersectOrInterior) == 0)
                {
                    graph[x, height - 1] |= exteriorFlag;
                }
            }

            // 1.2 遍历左右边界
            for (var y = 0; y < height; y++)
            {
                if ((graph[0, y] & intersectOrInterior) == 0)
                {
                    graph[0, y] |= exteriorFlag;
                }

                if ((graph[width - 1, y] & intersectOrInterior) == 0)
                {
                    graph[width - 1, y] |= exteriorFlag;
                }
            }

            // 2. 扩展外部区域 需要优化效率
            bool changed;
            do
            {
                changed = false;

                // 2.1 遍历所有格子 检查与外部区域相邻的单元格
                for (var x = 1; x < width - 1; x++)
                {
                    for (var y = 1; y < height - 1; y++)
                    {
                        // 2.2 如果该单元格没有任何标记 未检测
                        if ((graph[x, y] & anyFlag) == 0)
                        {
                            // 如果有相邻的外部区域 则标记为外部
                            if ((graph[x - 1, y] & exteriorFlag) != 0 ||
                                (graph[x + 1, y] & exteriorFlag) != 0 ||
                                (graph[x, y - 1] & exteriorFlag) != 0 ||
                                (graph[x, y + 1] & exteriorFlag) != 0)
                            {
                                graph[x, y] |= exteriorFlag;
                                changed = true;
                            }
                        }
                    }
                }
            }
            while (changed);

            // 3. 标记所有剩余的相交单元格为内部区域
            var intersectOrExterior = intersectFlag | exteriorFlag;
            for (var x = 1; x < width - 1; x++)
            {
                for (var y = 1; y < height - 1; y++)
                {
                    if ((graph[x, y] & intersectOrExterior) == 0)
                    {
                        graph[x, y] |= interiorFlag;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool LinesIntersect(in Vector2 p0, in Vector2 p1, in Vector2 q0, in Vector2 q1)
        {
            var d0 = Cross(q0, q1, p0);
            var d1 = Cross(q0, q1, p1);
            var d2 = Cross(p0, p1, q0);
            var d3 = Cross(p0, p1, q1);

            if ((d0 * d1 < 0) && (d2 * d3 < 0))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Cross(in Vector2 p0, in Vector2 p1, in Vector2 p)
        {
            return ((p.x - p0.x) * (p1.y - p0.y)) - ((p.y - p0.y) * (p1.x - p0.x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 GetPerpendicularVector(in Vector2 p0, in Vector2 p1, in Vector2 p)
        {
            Vector2 d = p1 - p0;
            Vector2 p0p = p - p0;

            // 计算投影系数 t
            var dSqLength = d.sqrMagnitude;
            if (dSqLength == 0)
            {
                return Vector2.zero;
            }

            var t = Vector2.Dot(p0p, d) / dSqLength;

            // 投影点q
            Vector2 q = p0 + (t * d);

            Vector2 pq = q - p;
            Vector2 perpendicular = Vector2.Perpendicular(d).normalized;

            // 判断方向
            var dotProduct = Vector2.Dot(pq, perpendicular);
            return dotProduct > 0 ? perpendicular : -perpendicular;
        }

        private static void CalculateLargestInteriorRectangle(float[] xCoords, float[] yCoords, byte[,] graph, out Rect rect)
        {
            rect = default;

            var width = graph.GetLength(0);
            var height = graph.GetLength(1);

            // 高度数组（直方图法）
            var heights = new float[width];

            float maxArea = 0;

            for (var y = height - 1; y >= 0; y--)
            {
                for (var x = 0; x < width; x++)
                {
                    // 更新高度数组
                    if ((graph[x, y] & interiorFlag) != 0)
                    {
                        heights[x] += yCoords[y + 1] - yCoords[y];
                    }
                    else
                    {
                        heights[x] = 0;
                    }
                }

                // 计算这一行的直方图的最大矩形面积
                var currentArea = MaxAreaInHistogram(xCoords, yCoords[y], heights, out var currentRect);

                if (currentArea > maxArea)
                {
                    maxArea = currentArea;
                    rect = currentRect;
                }
            }
        }

        private static float MaxAreaInHistogram(float[] xs, float y, float[] heights, out Rect rect)
        {
            rect = default;

            var stack = new Stack<int>();
            float maxArea = 0;

            var columns = heights.Length;

            // 为方便加一个哨兵柱子
            for (var k = 0; k <= columns; k++)
            {
                var currentHeight = (k < columns) ? heights[k] : 0;

                while (stack.Count > 0 && currentHeight < heights[stack.Peek()])
                {
                    var h = heights[stack.Pop()];
                    var rightIndex = k;
                    var leftIndex = stack.Count > 0 ? stack.Peek() + 1 : 0;
                    var width = xs[rightIndex] - xs[leftIndex];
                    var area = h * width;
                    if (area > maxArea)
                    {
                        maxArea = area;
                        rect = new Rect(xs[leftIndex], y, width, h);
                    }
                }

                stack.Push(k);
            }

            return maxArea;
        }
    }
}
