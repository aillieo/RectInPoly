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
    using SubdivideMode = AillieoUtils.LargestRectInPolygon.SubdivideMode;

    [ExecuteAlways]
    internal class Sample : MonoBehaviour
    {
        public SubdivideMode subdivideMode = SubdivideMode.Intersection & SubdivideMode.MidPoint;

        [Range(0, 4)]
        public int subdivisions = 2;

        public bool drawGraph;

        private static readonly Vector3[] fourPoints = new Vector3[4];

        private Point[] points;

        private LineRenderer lineRenderer;

        private Camera mainCamera;

        private GUIStyle labelStyle;

        private static void DrawCells(float[] xs, float[] ys, byte[,] cells)
        {
            Color backup = Gizmos.color;

            for (var x = 0; x + 1 < xs.Length; x++)
            {
                for (var y = 0; y + 1 < ys.Length; y++)
                {
                    var index = cells[x, y];

                    fourPoints[0] = new Vector2(xs[x], ys[y]);
                    fourPoints[1] = new Vector2(xs[x + 1], ys[y]);
                    fourPoints[2] = new Vector2(xs[x + 1], ys[y + 1]);
                    fourPoints[3] = new Vector2(xs[x], ys[y + 1]);

                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                    Gizmos.DrawLineStrip(fourPoints, true);

                    if (index == 1)
                    {
                        Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.1f);
                        Gizmos.DrawCube((fourPoints[0] + fourPoints[2]) / 2, fourPoints[2] - fourPoints[0]);
                    }
                }
            }

            Gizmos.color = backup;
        }

        private void OnEnable()
        {
            this.mainCamera = Camera.main;

            this.points = this.GetComponentsInChildren<Point>();
            this.lineRenderer = this.GetComponent<LineRenderer>();

            this.CheckAndUpdateLineRenderer(true);
        }

        private void Update()
        {
            this.CheckAndUpdateLineRenderer(false);
        }

        private void CheckAndUpdateLineRenderer(bool forceUpdate)
        {
            var positionChanged = false;
            if (forceUpdate)
            {
                positionChanged = true;
            }
            else
            {
                foreach (var point in this.points)
                {
                    if (point.transform.hasChanged)
                    {
                        point.transform.hasChanged = false;
                        positionChanged = true;
                    }
                }
            }

            if (positionChanged)
            {
                this.lineRenderer.positionCount = this.points.Length;
                for (var i = 0; i < this.points.Length; i++)
                {
                    this.lineRenderer.SetPosition(i, this.points[i].transform.position);
                }

                var polygon = this.points.Select(p => (Vector2)(Vector3)p).ToArray();
                var valid = LargestRectInPolygon.IsValidPolygon(polygon);

                Color lineColor = Color.white;
                if (!valid)
                {
                    lineColor = Color.red;
                }
                else
                {
                    var clockwise = LargestRectInPolygon.IsClockwise(polygon);
                    if (!clockwise)
                    {
                        lineColor = Color.blue;
                    }
                }

                // this.lineRenderer.startColor = lineColor;
                // this.lineRenderer.endColor = lineColor;
                if (Application.isEditor && !Application.isPlaying)
                {
                    this.lineRenderer.sharedMaterial.color = lineColor;
                }
                else
                {
                    this.lineRenderer.material.color = lineColor;
                }
            }
        }

        private void OnDrawGizmos()
        {
            var polygonV3 = this.points.Select(p => (Vector3)p).ToArray();
            var polygon = polygonV3.Select(v3 => (Vector2)v3).ToArray();

            var valid = LargestRectInPolygon.IsValidPolygon(polygon);

            Color backup = Gizmos.color;
            if (!valid)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                var clockwise = LargestRectInPolygon.IsClockwise(polygon);
                if (!clockwise)
                {
                    polygon = polygon.Reverse().ToArray();
                    Gizmos.color = Color.blue;
                }
            }

            Gizmos.DrawLineStrip(polygonV3, true);

            Gizmos.color = backup;

            if (valid && this.drawGraph)
            {
                if (this.drawGraph)
                {
                    var rect = LargestRectInPolygon.Find(polygon, this.subdivideMode, this.subdivisions, out var x, out var y, out var graph);
                    DrawCells(x, y, graph);
                }
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
            GUI.Label(labelRect, $"Sub Divisions:{this.subdivisions}", this.labelStyle);
            var sliderRect = new Rect(0, 50, 200, 50);
            var sliderValue = GUI.HorizontalSlider(sliderRect, this.subdivisions, 0, 4);
            this.subdivisions = Mathf.Clamp((int)sliderValue, 0, 4);

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
                    rect = LargestRectInPolygon.Find(polygon, this.subdivideMode, this.subdivisions, out var x, out var y, out var graph);
                }
                else
                {
                    rect = LargestRectInPolygon.Find(polygon, this.subdivideMode, this.subdivisions);
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
                var guiColor = GUI.color;
                GUI.color = new Color(0.2f, 0.75f, 0.2f, 0.9f);
                GUI.DrawTexture(guiRect, Texture2D.whiteTexture);
                GUI.color = guiColor;
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
