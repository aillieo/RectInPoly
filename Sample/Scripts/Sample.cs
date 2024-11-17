// -----------------------------------------------------------------------
// <copyright file="Sample.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Game
{
    using System.Linq;
    using AillieoUtils;
    using UnityEngine;

    [ExecuteAlways]
    public class Sample : MonoBehaviour
    {
        [Range(0, 2)]
        public int subDivisions = 2;

        public bool drawGraph;

        private Point[] points;

        private LineRenderer lineRenderer;

        private Camera mainCamera;

        private GUIStyle labelStyle;

        private void OnEnable()
        {
            this.mainCamera = Camera.main;

            this.points = this.GetComponentsInChildren<Point>();
            this.lineRenderer = this.GetComponent<LineRenderer>();
        }

        private void Update()
        {
            var positionChanged = false;
            foreach (var point in this.points)
            {
                if (point.positionDirty)
                {
                    point.positionDirty = false;
                    positionChanged = true;
                }
            }

            if (positionChanged)
            {
                this.lineRenderer.positionCount = this.points.Length;
                for (var i = 0; i < this.points.Length; i++)
                {
                    this.lineRenderer.SetPosition(i, this.points[i].transform.position);
                }
            }
        }

        private static void DrawCells(float[] xs, float[] ys, int[,] cells)
        {
            Color backup = Gizmos.color;
            Gizmos.color = new Color(1, 1, 1, 0.1f);

            for (var x = 0; x + 1 < xs.Length; x++)
            {
                for (var y = 0; y + 1 < ys.Length; y++)
                {
                    var index = cells[x, y];

                    var fourPoints = new Vector3[4]
                    {
                        new Vector2(xs[x], ys[y]),
                        new Vector2(xs[x + 1], ys[y]),
                        new Vector2(xs[x + 1], ys[y + 1]),
                        new Vector2(xs[x], ys[y + 1]),
                    };

                    if ((index & LargestRectInPolygon.interiorFlag) != 0)
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.1f);
                        Gizmos.DrawLineStrip(fourPoints, true);
                        Gizmos.DrawCube((fourPoints[0] + fourPoints[2]) / 2, fourPoints[2] - fourPoints[0]);
                    }

                    if ((index & LargestRectInPolygon.exteriorFlag) != 0)
                    {
                        Gizmos.color = new Color(0, 1, 1, 0.1f);
                        Gizmos.DrawLineStrip(fourPoints, true);
                        Gizmos.DrawCube((fourPoints[0] + fourPoints[2]) / 2, fourPoints[2] - fourPoints[0]);
                    }

                    if ((index & LargestRectInPolygon.intersectFlag) != 0)
                    {
                        Gizmos.color = new Color(1, 1, 1, 0.2f);
                        Gizmos.DrawLineStrip(fourPoints, true);
                        Gizmos.DrawCube((fourPoints[0] + fourPoints[2]) / 2, fourPoints[2] - fourPoints[0]);
                    }
                }
            }

            Gizmos.color = backup;
        }

        private void OnDrawGizmos()
        {
            var polygon = this.points.Select(p => (Vector2)p.transform.position).ToArray();

            var valid = LargestRectInPolygon.IsValidPolygon(polygon);

            Color backup = Gizmos.color;
            if (!valid)
            {
                Gizmos.color = Color.red;
            }

            var clockwise = LargestRectInPolygon.IsClockwise(polygon);
            if (!clockwise)
            {
                Gizmos.color = Color.blue;
            }

            Gizmos.color = backup;

            if (valid)
            {
                if (!clockwise)
                {
                    polygon = polygon.Reverse().ToArray();
                }

                Rect rect = default;
                if (this.drawGraph)
                {
                    rect = LargestRectInPolygon.Find(polygon, this.subDivisions, out var x, out var y, out var graph);
                    DrawCells(x, y, graph);
                }
                else
                {
                    rect = LargestRectInPolygon.Find(polygon, this.subDivisions);
                }

                Gizmos.color = new Color(1, 1, 0, 0.5f);
                //Gizmos.DrawCube(rect.center, rect.size);
            }
        }

        private void OnGUI()
        {
            if (this.labelStyle == null)
            {
                this.labelStyle = new GUIStyle();
                this.labelStyle.fontSize = 36;
                this.labelStyle.normal.textColor = Color.white;
            }

            // points
            foreach (var point in this.points)
            {
                Vector3 guiPosition = this.WorldToGUIPosition(point.transform.position);
                var rect = new Rect(guiPosition.x, guiPosition.y, 200, 50);
                var labelText = $"{point.name}";
                GUI.Label(rect, labelText, this.labelStyle);
            }

            // ui
            var labelRect = new Rect(0, 0, 200, 50);
            GUI.Label(labelRect, $"Sub Divisions:{this.subDivisions}", this.labelStyle);
            var sliderRect = new Rect(0, 50, 200, 50);
            var sliderValue = GUI.HorizontalSlider(sliderRect, this.subDivisions, 0, 2);
            this.subDivisions = Mathf.Clamp((int)sliderValue, 0, 2);

            var polygon = this.points.Select(p => (Vector2)p.transform.position).ToArray();

            var valid = LargestRectInPolygon.IsValidPolygon(polygon);

            if (!valid)
            {
            }

            var clockwise = LargestRectInPolygon.IsClockwise(polygon);
            if (!clockwise)
            {
            }

            if (valid)
            {
                if (!clockwise)
                {
                    polygon = polygon.Reverse().ToArray();
                }

                Rect rect = default;
                if (this.drawGraph)
                {
                    rect = LargestRectInPolygon.Find(polygon, this.subDivisions, out var x, out var y, out var graph);
                }
                else
                {
                    rect = LargestRectInPolygon.Find(polygon, this.subDivisions);
                }

                var lb = rect.position;
                var rt = rect.position + rect.size;
                var lbScreen = this.WorldToGUIPosition(lb);
                var rtScreen = this.WorldToGUIPosition(rt);

                var xMin = Mathf.Min(lbScreen.x, rtScreen.x);
                var xMax = Mathf.Max(lbScreen.x, rtScreen.x);
                var yMin = Mathf.Min(lbScreen.y, rtScreen.y);
                var yMax = Mathf.Max(lbScreen.y, rtScreen.y);

                var guiRect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
                GUI.DrawTexture(guiRect, Texture2D.grayTexture);
            }
        }

        private Vector3 WorldToGUIPosition(Vector3 worldPosition)
        {
            Vector3 screenPosition = this.mainCamera.WorldToScreenPoint(worldPosition);
            var guiPosition = new Vector3(
                screenPosition.x,
                Screen.height - screenPosition.y,
                0);
            return guiPosition;
        }
    }
}
