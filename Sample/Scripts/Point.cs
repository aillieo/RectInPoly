// -----------------------------------------------------------------------
// <copyright file="Point.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Game
{
    using System;
    using UnityEngine;

    [ExecuteAlways]
    public class Point : MonoBehaviour
    {
        [NonSerialized]
        public bool positionDirty = true;

        private Camera mainCamera;

        private float zScreen;
        private Vector3 draggingStartOffset;

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
            this.positionDirty = true;
        }

        private Vector3 GetMouseWorldPos()
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = this.zScreen;
            return this.mainCamera.ScreenToWorldPoint(mouseScreenPos);
        }
    }
}
