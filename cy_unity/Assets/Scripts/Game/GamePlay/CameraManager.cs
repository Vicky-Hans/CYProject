using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DH.Game
{
    public class CameraManager : Singleton<CameraManager>
    {
        private class OverlayCameraItemCompare : Comparer<OverlayCameraItem>
        {
            public static readonly OverlayCameraItemCompare DefaultCompare = new OverlayCameraItemCompare();
            public override int Compare(OverlayCameraItem x, OverlayCameraItem y)
            {
                return x.CompareTo(y);
            }
        }
        
        private struct OverlayCameraItem : IComparable<OverlayCameraItem>,IEquatable<OverlayCameraItem>
        {
            public Camera camera;
            public int priority;

            public override int GetHashCode()
            {
                return camera.GetHashCode();
            }

            public int CompareTo(OverlayCameraItem other)
            {
                return priority.CompareTo(other.priority);
            }

            public override bool Equals(object obj)
            {
                if (camera == null && obj == null)
                    return true;
                
                var otherCamera = obj as Camera;

                if (camera == null || otherCamera == null)
                    return false;
                
                return camera == otherCamera;
            }
            
            public bool Equals(OverlayCameraItem other)
            {
                return Equals(camera, other.camera);
            }
        }
        
        private Camera baseCamera;
        private UniversalAdditionalCameraData cameraData;
        private readonly List<OverlayCameraItem> uiCameras = new List<OverlayCameraItem>(2);
        private readonly List<OverlayCameraItem> sceneCameras = new List<OverlayCameraItem>(2);
        private readonly HashSet<Camera> allCameraHashSet = new HashSet<Camera>(2);

        public void Init()
        {
            baseCamera = AppGlobal.Instance.BaseCamera;
            cameraData = baseCamera.GetComponent<UniversalAdditionalCameraData>();
            
            AddCamera(AppGlobal.Instance.UICamera, 10, true);
        }

        public void AddCamera(Camera camera, int priority, bool isUI = false)
        {
            if (camera == null)
            {
                return;
            }

            if (allCameraHashSet.Contains(camera))
            {
                return;
            }

            if (isUI)
            {
                uiCameras.Add(new OverlayCameraItem()
                {
                    camera = camera,
                    priority = priority,
                });
                uiCameras.Sort(OverlayCameraItemCompare.DefaultCompare);
            }
            else
            {
                sceneCameras.Add(new OverlayCameraItem()
                {
                    camera = camera,
                    priority = priority,
                });
            
                sceneCameras.Sort(OverlayCameraItemCompare.DefaultCompare);
            }

            allCameraHashSet.Add(camera);

            UpdateCameraStack();
        }

        public void RemoveCamera(Camera camera, bool isUI = false)
        {
            if (!allCameraHashSet.Contains(camera))
            {
                return;
            }

            if (isUI)
            {
                uiCameras.Remove(new OverlayCameraItem() { camera = camera});
            }
            else
            {
                sceneCameras.Remove(new OverlayCameraItem() { camera = camera});
            }
            
            UpdateCameraStack();
        }

        private void UpdateCameraStack()
        {
            if (cameraData == null)
            {
                return;
            }

            var cameraStack = cameraData.cameraStack;
            cameraStack.Clear();
            foreach (var cameraItem in sceneCameras)
            {
                cameraStack.Add(cameraItem.camera);
            }

            foreach (var cameraItem in uiCameras)
            {
                cameraStack.Add(cameraItem.camera);
            }
        }
        
    }
}