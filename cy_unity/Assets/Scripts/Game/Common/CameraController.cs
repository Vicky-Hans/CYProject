using System;
using System.Runtime.CompilerServices;
using DH.Data;
using Game;
using UnityEngine;
using UnityEngine.Rendering;

namespace DH.Game
{
    /// <summary>
    /// Camera Size = x / ((( x / y ) * 2 ) * s )
    /// Where:
    /// x = Screen Width (px)
    /// y = Screen Height (px)
    /// s = Desired Height of Photoshop Square (px)
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public float smoothTime = 0.3f;
        public Vector3 targetOffset;
        public Bounds cameraRange;
        [SerializeField] 
        private Transform target;
        [SerializeField]
        private Transform cameraTarget;
        
        /// <summary>
        /// 摄像机跟随方向永远和角色相同移动方向相同，防止击退等效果影响摄像机效果
        /// </summary>
        internal Vector3 currentMoveDirection;
        
        /// <summary>
        /// 地图区域
        /// </summary>
        private Camera mainCamera;
        private Vector3[] corners = new Vector3[4];
        private Plane plane = new Plane(new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0));
        private bool ignoreFollow;
        private Vector3 velocity = Vector3.zero;
        private Vector3 previousTargetPos;
        public Vector2 MapRange { get; set; }
        public Vector2 ScreenSize { get; set; }
        
        public Camera MainCamera => mainCamera;

        private void Awake()
        {
            if (!cameraTarget)
            {
                cameraTarget = AppGlobal.Instance.BaseCamera.transform;
            }
            mainCamera = cameraTarget.GetComponent<Camera>();
            CalculateCameraProperties(Screen.width, Screen.height);
        }

        private void LateUpdate()
        {
            if(!target || ignoreFollow)
            {
                return;
            }

            var screenSize = BattleManager.Instance.fightingManagerIns.ScreenSize;
            var srcPosition = target.position;
            var targetPosition = srcPosition + targetOffset;
            var delta = targetPosition - previousTargetPos;
            var current = cameraTarget.position;
            // Vector3 smoothedPosition = Vector3.SmoothDamp(current, targetPosition, ref velocity, smoothTime);
            var smoothedPosition = targetPosition;
            smoothedPosition.z = -2f;
            smoothedPosition.x = ClampMinMax(smoothedPosition.x, -MapRange.x/2+screenSize.x/2, MapRange.x/2-screenSize.x/2);
            smoothedPosition.y = ClampMinMax(smoothedPosition.y, -MapRange.y/2+screenSize.y/2, MapRange.y/2-screenSize.y/2);
            cameraTarget.position = smoothedPosition;
            // PixelSnap(smoothedPosition);
            // CalculateCorner();
            // UpdateCameraRange();
        }

        internal void CalculateCameraProperties(int screenWidth, int screenHeight)
        {
            int assetsPpu = (int)GameConst.PixelPerUnit;
            int refResolutionX = 1080;
            int refResolutionY = 1920;
            // zoom level (PPU scale)
            float verticalZoom = screenHeight * 1f / refResolutionY;
            float horizontalZoom = screenWidth * 1f / refResolutionX;
            float zoom = Math.Max(1, Math.Min(verticalZoom, horizontalZoom));
            if (verticalZoom < 1f && horizontalZoom < 1f)
            {
                zoom = Math.Min(verticalZoom, horizontalZoom);
            }
            float pixelHeight = screenHeight ;
            mainCamera.orthographicSize = (pixelHeight * 0.5f) / (zoom * assetsPpu);
        }
        
        private void PixelSnap(Vector3 cameraPosition)
        {
            Vector3 roundedCameraPosition = RoundToPixel(cameraPosition);
            Vector3 offset = roundedCameraPosition - cameraPosition;
            offset.z = -offset.z;
            Matrix4x4 offsetMatrix = Matrix4x4.TRS(-offset, Quaternion.identity, new Vector3(1.0f, 1.0f, -1.0f));

            mainCamera.worldToCameraMatrix = offsetMatrix * mainCamera.transform.worldToLocalMatrix;

            CalculateCameraProperties(Screen.width, Screen.height);
        }
        
        private Vector4 RoundToPixel(Vector4 position)
        {
            float unitsPerPixel = 1 / GameConst.PixelPerUnit;
            if (unitsPerPixel == 0.0f)
                return position;

            Vector4 result;
            result.x = Mathf.Round(position.x / unitsPerPixel) * unitsPerPixel;
            result.y = Mathf.Round(position.y / unitsPerPixel) * unitsPerPixel;
            result.z = Mathf.Round(position.z / unitsPerPixel) * unitsPerPixel;
            result.w = Mathf.Round(position.w / unitsPerPixel) * unitsPerPixel;

            return result;
        }
        
        private Vector3 RoundToPixel(Vector3 position)
        {
            float unitsPerPixel = 1 / GameConst.PixelPerUnit;
            if (unitsPerPixel == 0.0f)
                return position;

            Vector3 result;
            result.x = Mathf.Round(position.x / unitsPerPixel) * unitsPerPixel;
            result.y = Mathf.Round(position.y / unitsPerPixel) * unitsPerPixel;
            result.z = Mathf.Round(position.z / unitsPerPixel) * unitsPerPixel;

            return result;
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            if (mainCamera)
            {
                mainCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                mainCamera.ResetAspect();
                mainCamera.ResetWorldToCameraMatrix();
            }
        }
        
        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == mainCamera)
            {
                UnityEngine.U2D.PixelPerfectRendering.pixelSnapSpacing = 0.01f;
            }
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == mainCamera)
                UnityEngine.U2D.PixelPerfectRendering.pixelSnapSpacing = 0.0f;
        }

        private void CalculateCorner()
        {
            corners[0] = MainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
            corners[1] = MainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
            corners[2] = MainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
            corners[3] = MainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0));
        }
        
        private void UpdateCameraRange()
        {
            cameraRange = new Bounds(corners[0],new Vector3(1,1,01));
            cameraRange.Encapsulate(corners[1]);
            cameraRange.Encapsulate(corners[2]);
            cameraRange.Encapsulate(corners[3]);
        }
        
        //返回射线与平面相交的点
        public Vector3 ViewportPointToWorldPointOnPlane(Vector3 viewportPoint)
        {
            Ray ray = MainCamera.ViewportPointToRay(viewportPoint);
            plane.Raycast(ray, out var dist);
            Vector3 vect = ray.GetPoint(dist);
            return vect;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float ClampMinMax(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(cameraRange.center,cameraRange.size);
        }
#endif
        
        public void SetTarget(Transform targetTransform)
        {
            target = targetTransform;
            previousTargetPos = target.position + targetOffset;
            // transform.position = previousTargetPos;
        }
    }
}