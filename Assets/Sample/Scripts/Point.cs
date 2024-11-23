// -----------------------------------------------------------------------
// <copyright file="Point.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Game
{
    using UnityEngine;

    [ExecuteAlways]
    internal class Point : MonoBehaviour
    {
        private Camera mainCamera;

        private float zScreen;
        private Vector3 draggingStartOffset;

        public static implicit operator Vector3(Point point)
        {
            return point.transform.position;
        }

        private void OnEnable()
        {
            this.mainCamera = Camera.main;
        }

        private void OnMouseDown()
        {
            var screenPoint = this.mainCamera.WorldToScreenPoint(this.transform.position);
            this.zScreen = screenPoint.z;
            this.draggingStartOffset = this.transform.position - this.GetMouseWorldPos();
        }

        private void OnMouseDrag()
        {
            Vector3 newPosition = this.GetMouseWorldPos() + this.draggingStartOffset;
            newPosition.z = 0;
            this.transform.position = newPosition;
        }

        private Vector3 GetMouseWorldPos()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = this.zScreen;
            return this.mainCamera.ScreenToWorldPoint(mouseScreenPos);
        }
    }
}
