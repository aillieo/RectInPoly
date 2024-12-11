// -----------------------------------------------------------------------
// <copyright file="LargestRectInPolygon.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    /// <summary>
    /// Provides methods to find the largest rectangle that can fit inside a given polygon.
    /// </summary>
    public static class LargestRectInPolygon
    {
        private const byte intersectFlag = 0b0010;
        private const byte interiorFlag = 0b0001;
        private const byte exteriorFlag = 0b0100;

        private static readonly Stack<int> stackReusable = new Stack<int>();
        private static readonly Stack<List<float>> listsReusable = new Stack<List<float>>();

        /// <summary>
        /// Specifies the subdivision modes used for grid calculations in polygons.
        /// Each value is a combination of 'M' (MidPoint) and 'C' (CrossingPoint) indicating the subdivision strategy.
        /// </summary>
        public enum SubdivideMode
        {
            /// <summary>
            /// No subdivision will be performed on the grid.
            /// </summary>
            None = 0,

            /// <summary>
            /// M - Subdivide using midpoints.
            /// </summary>
            MidPoint = 0b01,

            /// <summary>
            /// C - Subdivide using crossing points.
            /// </summary>
            CrossingPoint = 0b10,

            /// <summary>
            /// MM - Twice MidPoint subdivision.
            /// </summary>
            MM = MidPoint | MidPoint << 2,

            /// <summary>
            /// MC - MidPoint then CrossingPoint.
            /// </summary>
            MC = MidPoint | CrossingPoint << 2,

            /// <summary>
            /// CM - CrossingPoint then MidPoint.
            /// </summary>
            CM = CrossingPoint | MidPoint << 2,

            /// <summary>
            /// CC - Twice CrossingPoint subdivision.
            /// </summary>
            CC = CrossingPoint | CrossingPoint << 2,

            /// <summary>
            /// MMM - MidPoint then MidPoint then MidPoint.
            /// </summary>
            MMM = MidPoint | MidPoint << 2 | MidPoint << 4,

            /// <summary>
            /// MCM - MidPoint then CrossingPoint then MidPoint.
            /// </summary>
            MCM = MidPoint | CrossingPoint << 2 | MidPoint << 4,

            /// <summary>
            /// CMM - CrossingPoint then MidPoint then MidPoint.
            /// </summary>
            CMM = CrossingPoint | MidPoint << 2 | MidPoint << 4,

            /// <summary>
            /// CCM - CrossingPoint then CrossingPoint then MidPoint.
            /// </summary>
            CCM = CrossingPoint | CrossingPoint << 2 | MidPoint << 4,

            /// <summary>
            /// MMC - MidPoint then MidPoint then CrossingPoint.
            /// </summary>
            MMC = MidPoint | MidPoint << 2 | CrossingPoint << 4,

            /// <summary>
            /// MCC - MidPoint then CrossingPoint then CrossingPoint.
            /// </summary>
            MCC = MidPoint | CrossingPoint << 2 | CrossingPoint << 4,

            /// <summary>
            /// CMC - CrossingPoint then MidPoint then CrossingPoint.
            /// </summary>
            CMC = CrossingPoint | MidPoint << 2 | CrossingPoint << 4,

            /// <summary>
            /// CCC - CrossingPoint then CrossingPoint then CrossingPoint.
            /// </summary>
            CCC = CrossingPoint | CrossingPoint << 2 | CrossingPoint << 4,

            /// <summary>
            /// MMMM - MidPoint then MidPoint then MidPoint then MidPoint.
            /// </summary>
            MMMM = MidPoint | MidPoint << 2 | MidPoint << 4 | MidPoint << 6,

            /// <summary>
            /// MCMM - MidPoint then CrossingPoint then MidPoint then MidPoint.
            /// </summary>
            MCMM = MidPoint | CrossingPoint << 2 | MidPoint << 4 | MidPoint << 6,

            /// <summary>
            /// CMMM - CrossingPoint then MidPoint then MidPoint then MidPoint.
            /// </summary>
            CMMM = CrossingPoint | MidPoint << 2 | MidPoint << 4 | MidPoint << 6,

            /// <summary>
            /// CCMM - CrossingPoint then CrossingPoint then MidPoint then MidPoint.
            /// </summary>
            CCMM = CrossingPoint | CrossingPoint << 2 | MidPoint << 4 | MidPoint << 6,

            /// <summary>
            /// MMCM - MidPoint then MidPoint then CrossingPoint then MidPoint.
            /// </summary>
            MMCM = MidPoint | MidPoint << 2 | CrossingPoint << 4 | MidPoint << 6,

            /// <summary>
            /// MCCM - MidPoint then CrossingPoint then CrossingPoint then MidPoint.
            /// </summary>
            MCCM = MidPoint | CrossingPoint << 2 | CrossingPoint << 4 | MidPoint << 6,

            /// <summary>
            /// CMCM - CrossingPoint then MidPoint then CrossingPoint then MidPoint.
            /// </summary>
            CMCM = CrossingPoint | MidPoint << 2 | CrossingPoint << 4 | MidPoint << 6,

            /// <summary>
            /// CCCM - CrossingPoint then CrossingPoint then CrossingPoint then MidPoint.
            /// </summary>
            CCCM = CrossingPoint | CrossingPoint << 2 | CrossingPoint << 4 | MidPoint << 6,

            /// <summary>
            /// MMMC - MidPoint then MidPoint then MidPoint then CrossingPoint.
            /// </summary>
            MMMC = MidPoint | MidPoint << 2 | MidPoint << 4 | CrossingPoint << 6,

            /// <summary>
            /// MCMC - MidPoint then CrossingPoint then MidPoint then CrossingPoint.
            /// </summary>
            MCMC = MidPoint | CrossingPoint << 2 | MidPoint << 4 | CrossingPoint << 6,

            /// <summary>
            /// CMMC - CrossingPoint then MidPoint then MidPoint then CrossingPoint.
            /// </summary>
            CMMC = CrossingPoint | MidPoint << 2 | MidPoint << 4 | CrossingPoint << 6,

            /// <summary>
            /// CCMC - CrossingPoint then CrossingPoint then MidPoint then CrossingPoint.
            /// </summary>
            CCMC = CrossingPoint | CrossingPoint << 2 | MidPoint << 4 | CrossingPoint << 6,

            /// <summary>
            /// MMCC - MidPoint then MidPoint then CrossingPoint then CrossingPoint.
            /// </summary>
            MMCC = MidPoint | MidPoint << 2 | CrossingPoint << 4 | CrossingPoint << 6,

            /// <summary>
            /// MCCC - MidPoint then CrossingPoint then CrossingPoint then CrossingPoint.
            /// </summary>
            MCCC = MidPoint | CrossingPoint << 2 | CrossingPoint << 4 | CrossingPoint << 6,

            /// <summary>
            /// CMCC - CrossingPoint then MidPoint then CrossingPoint then CrossingPoint.
            /// </summary>
            CMCC = CrossingPoint | MidPoint << 2 | CrossingPoint << 4 | CrossingPoint << 6,

            /// <summary>
            /// CCCC - CrossingPoint then CrossingPoint then CrossingPoint then CrossingPoint.
            /// </summary>
            CCCC = CrossingPoint | CrossingPoint << 2 | CrossingPoint << 4 | CrossingPoint << 6,
        }

        /// <summary>
        /// Determines if the points of the polygon are ordered in a clockwise direction.
        /// </summary>
        /// <param name="points">A list of points representing the vertices of the polygon.</param>
        /// <returns>True if the polygon's points are in clockwise order; otherwise, false.</returns>
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

        /// <summary>
        /// Validates whether the given points form a valid polygon.
        /// A valid polygon must have at least three vertices and must not self-intersect.
        /// </summary>
        /// <param name="points">A list of points representing the vertices of the polygon.</param>
        /// <returns>True if the polygon is valid; otherwise, false.</returns>
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

        /// <summary>
        /// Finds the largest rectangle that can fit inside the specified polygon.
        /// Defaults to using the Twice CrossingPoint subdivision mode.
        /// </summary>
        /// <param name="polygon">A list of points representing the vertices of the polygon.</param>
        /// <returns>The largest rectangle that fits within the polygon.</returns>
        public static Rect Find(IList<Vector2> polygon)
        {
            return Find(polygon, SubdivideMode.CC);
        }

        /// <summary>
        /// Finds the largest rectangle that can fit inside the specified polygon using the specified subdivision mode.
        /// </summary>
        /// <param name="polygon">A list of points representing the vertices of the polygon.</param>
        /// <param name="subdivideMode">The mode of subdivision to use for calculations.</param>
        /// <returns>The largest rectangle that fits within the polygon.</returns>
        public static Rect Find(IList<Vector2> polygon, SubdivideMode subdivideMode)
        {
            CalculateGrids(polygon, subdivideMode, out var xCoords, out var yCoords);
            PolygonGridsIntersection(polygon, xCoords, yCoords, out var graph);
            CalculateLargestInteriorRectangle(xCoords, yCoords, graph, out Rect rect);
            return rect;
        }

        /// <summary>
        /// Finds the largest rectangle that can fit inside the specified polygon.
        /// Outputs the x-coordinates, y-coordinates, and graph data for visualization.
        /// Defaults to using the Twice CrossingPoint subdivision mode.
        /// </summary>
        /// <param name="polygon">A list of points representing the vertices of the polygon.</param>
        /// <param name="xCoords">Output array of x-coordinates of graph used in calculations.</param>
        /// <param name="yCoords">Output array of y-coordinates of graph used in calculations.</param>
        /// <param name="graph">Output graph data used for visualization.</param>
        /// <returns>The largest rectangle that fits within the polygon.</returns>
        public static Rect Find(IList<Vector2> polygon, out float[] xCoords, out float[] yCoords, out byte[,] graph)
        {
            return Find(polygon, SubdivideMode.CC, out xCoords, out yCoords, out graph);
        }

        /// <summary>
        /// Finds the largest rectangle that can fit inside the specified polygon using the specified subdivision mode.
        /// Outputs the x-coordinates, y-coordinates, and graph data for visualization.
        /// </summary>
        /// <param name="polygon">A list of points representing the vertices of the polygon.</param>
        /// <param name="subdivideMode">The mode of subdivision to use for calculations.</param>
        /// <param name="xCoords">Output array of x-coordinates of graph used in calculations.</param>
        /// <param name="yCoords">Output array of y-coordinates of graph used in calculations.</param>
        /// <param name="graph">Output graph data used for visualization.</param>
        /// <returns>The largest rectangle that fits within the polygon.</returns>
        public static Rect Find(IList<Vector2> polygon, SubdivideMode subdivideMode, out float[] xCoords, out float[] yCoords, out byte[,] graph)
        {
            CalculateGrids(polygon, subdivideMode, out xCoords, out yCoords);
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

        private static void CalculateGrids(IList<Vector2> polygon, SubdivideMode subdivideMode, out float[] xCoords, out float[] yCoords)
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
            for (var n = 0; n < 5; n++)
            {
                var mode = (SubdivideMode)(((int)subdivideMode >> (n * 2)) & 0b11);
                if (mode == SubdivideMode.MidPoint)
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
                else if (mode == SubdivideMode.CrossingPoint)
                {
                    // project Points
                    // 遍历顶点流 每个edge 与x 和 y 判断是否交点 如果有 则在列表中加入交点
                    if (!listsReusable.TryPop(out var newXCoords))
                    {
                        newXCoords = new List<float>();
                    }

                    if (!listsReusable.TryPop(out var newYCoords))
                    {
                        newYCoords = new List<float>();
                    }

                    try
                    {
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
                    }
                    finally
                    {
                        newXCoords.Clear();
                        newYCoords.Clear();
                        listsReusable.Push(newXCoords);
                        listsReusable.Push(newYCoords);
                    }

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

                int edgeType;
                if (xStart < xEnd)
                {
                    if (yEnd > yStart)
                    {
                        edgeType = 1;
                    }
                    else if (yEnd < yStart)
                    {
                        edgeType = 4;
                    }
                    else // yEnd == yStart
                    {
                        edgeType = 8;
                    }
                }
                else if (xStart > xEnd)
                {
                    if (yEnd > yStart)
                    {
                        edgeType = 2;
                    }
                    else if (yEnd < yStart)
                    {
                        edgeType = 3;
                    }
                    else // yEnd == yStart
                    {
                        edgeType = 6;
                    }
                }
                else // xStart == xEnd
                {
                    if (yEnd > yStart)
                    {
                        edgeType = 5;
                    }
                    else // yEnd < yStart
                    {
                        edgeType = 7;
                    }
                }

                Vector2 v1 = -Vector2.Perpendicular(p1 - p0);
                v1.Normalize();

                var edgeBottomLeftX = Mathf.Min(p0.x, p1.x);
                var edgeBottomLeftY = Mathf.Min(p0.y, p1.y);
                var edgeTopRightX = Mathf.Max(p0.x, p1.x);
                var edgeTopRightY = Mathf.Max(p0.y, p1.y);

                var isHorizontal = p0.y == p1.y;
                var isVertical = p0.x == p1.x;

                // 遍历每个网格单元
                for (int x = 0, xlen = xGrids.Length - 1; x < xlen; x++)
                {
                    for (int y = 0, ylen = yGrids.Length - 1; y < ylen; y++)
                    {
                        // 获取网格单元的四个顶点
                        var gridBottomLeftX = xGrids[x];
                        var gridTopRightX = xGrids[x + 1];
                        var gridBottomLeftY = yGrids[y];
                        var gridTopRightY = yGrids[y + 1];

                        if (edgeBottomLeftX <= gridBottomLeftX && edgeBottomLeftY <= gridBottomLeftY
                            && edgeTopRightX >= gridTopRightX && edgeTopRightY >= gridTopRightY)
                        {
                            // 格子的包围盒小于edge的包围盒 必相交
                        }
                        else if (isHorizontal &&
                            edgeBottomLeftX <= gridBottomLeftX && edgeTopRightX >= gridTopRightX
                            && (edgeBottomLeftY == gridBottomLeftY || edgeTopRightY == gridTopRightY))
                        {
                            // edge是格子的上下两边之一
                        }
                        else if (isVertical &&
                            (edgeBottomLeftX == gridBottomLeftX || edgeTopRightX == gridTopRightX)
                            && edgeBottomLeftY <= gridBottomLeftY && edgeTopRightY >= gridTopRightY)
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
                                criticalVert = new Vector2(gridTopRightX, gridBottomLeftY);
                                break;
                            case 2:
                            case 6:
                                criticalVert = new Vector2(gridTopRightX, gridTopRightY);
                                break;

                            case 3:
                            case 7:
                                criticalVert = new Vector2(gridBottomLeftX, gridTopRightY);
                                break;
                            case 4:
                            case 8:
                                criticalVert = new Vector2(gridBottomLeftX, gridBottomLeftY);
                                break;
                        }

                        Vector2 v2 = GetPerpendicularVector(p0, p1, criticalVert);

                        if (Vector2.Dot(v1, v2) <= 0)
                        {
                            graph[x, y] |= intersectFlag;
                            graph[x, y] &= unchecked((byte)(~interiorFlag));
                        }
                        else
                        {
                            if ((graph[x, y] & intersectFlag) == 0)
                            {
                                graph[x, y] |= interiorFlag;
                            }
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
            try
            {
                rect = default;

                float maxArea = 0;

                var columns = heights.Length;

                // 为方便加一个哨兵柱子
                for (var k = 0; k <= columns; k++)
                {
                    var currentHeight = (k < columns) ? heights[k] : 0;

                    while (stackReusable.Count > 0 && currentHeight < heights[stackReusable.Peek()])
                    {
                        var h = heights[stackReusable.Pop()];
                        var rightIndex = k;
                        var leftIndex = stackReusable.Count > 0 ? stackReusable.Peek() + 1 : 0;
                        var width = xs[rightIndex] - xs[leftIndex];
                        var area = h * width;
                        if (area > maxArea)
                        {
                            maxArea = area;
                            rect = new Rect(xs[leftIndex], y, width, h);
                        }
                    }

                    stackReusable.Push(k);
                }

                return maxArea;
            }
            finally
            {
                stackReusable.Clear();
            }
        }
    }
}
