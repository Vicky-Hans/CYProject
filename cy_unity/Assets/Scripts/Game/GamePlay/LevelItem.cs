using System;
using UnityEngine;

namespace DH.Game
{
    public class LevelItem : MonoBehaviour
    {
        public Camera sceneCamera;

        private void Start()
        {
            CameraManager.Instance.AddCamera(sceneCamera,0);
        }

        private void OnDestroy()
        {
            CameraManager.Instance.RemoveCamera(sceneCamera);
        }
    }
}