using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DH
{
    /// <summary>
    /// 质量设置的回调接口
    /// </summary>
    public interface IQualitySettings
    {
        /// <summary>
        /// 低质量回调
        /// </summary>
        void LowQualitySetting();
            
        /// <summary>
        /// 中等质量回调
        /// </summary>
        void MiddleQualitySetting();
            
        /// <summary>
        /// 高等质量回调
        /// </summary>
        void HighQualitySetting();
            
        /// <summary>
        /// 极致质量回调
        /// </summary>
        void SuperQualitySetting();

    }
    
    public class QualityManager
    {
        /// <summary>
        /// 硬件的性能级别
        /// </summary>
        public enum DevicePerformanceLevel
        {
            /// <summary>
            /// 低端机型
            /// </summary>
            Low = 0,

            /// <summary>
            /// 中端机型
            /// </summary>
            Middle,

            /// <summary>
            /// 高端机型
            /// </summary>
            High,

            /// <summary>
            /// 极致机型
            /// </summary>
            Super,
        }

        private const string DeviceQualityLevelInitSettingsKey = "DH.RenderQuality.Init";
        private const string DeviceQualityLevelSettingsKey = "DH.RenderQuality.Level";

        private IQualitySettings qualitySettings;

        #region Instance

        private sealed class Nest
        {
            internal static readonly QualityManager instance = new QualityManager();

            static Nest()
            {

            }
        }

        public static QualityManager Instance => Nest.instance;

        private QualityManager()
        {

        }

        #endregion

        /// <summary>
        /// 初始化质量设置，第一次设置自动根据设备性能进行默认质量设置；
        /// 后续设置会根据ChangeQualityLevel的设置进行调整；
        /// </summary>
        /// <param name="lowLevel">可以支持的最低等级</param>
        /// <param name="middleLevel">可以支持的中等等级</param>
        /// <param name="highLevel">可以支持的高级等级</param>
        /// <param name="superLevel">可以支持的极致等级</param>
        public void InitQualityLevel(IQualitySettings qualitySettings)
        {
            this.qualitySettings = qualitySettings;
            
            int isInit = PlayerPrefs.GetInt(DeviceQualityLevelInitSettingsKey, 0);
            if (isInit != 1)
            {
                ChangeQualityLevelBaseDevicePerformance();
                
                PlayerPrefs.SetInt(DeviceQualityLevelInitSettingsKey, 1);
                PlayerPrefs.Save();
                return;
            }

            int newLevel = PlayerPrefs.GetInt(DeviceQualityLevelSettingsKey, (int)DevicePerformanceLevel.Middle);
            ApplyQualitySettings((DevicePerformanceLevel)newLevel);
        }
        
        /// <summary>
        /// 改变画面质量设置，立刻回调IQualitySettings；同时会保存当前设置，下次启动再次调用
        /// IQualitySettings保持设置；
        /// </summary>
        /// <param name="performanceLevel">DevicePerformanceLevel</param>
        public void ChangeQualityLevel(DevicePerformanceLevel performanceLevel)
        {
            PlayerPrefs.SetInt(DeviceQualityLevelSettingsKey, (int)performanceLevel);
            PlayerPrefs.Save();
            
            ApplyQualitySettings(performanceLevel);
        }
        
        /// <summary>
        /// 设置目标fps，[30, 60]
        /// </summary>
        /// <param name="fps"></param>
        public void SetFrameRate(int fps)
        {
            Application.targetFrameRate = Mathf.Clamp(fps, 30, 60);
        }
        
        /// <summary>
        /// 调整渲染分辨率
        /// </summary>
        /// <param name="minRenderScale">渲染分辨率最小值</param>
        /// <param name="maxRenderScale">渲染分辨率最大值</param>
        /// <param name="auto">是否根据目标分辨率，自动计算render scale</param>
        /// <param name="target"> 目标分辨率 </param>
        public void ChangeRenderScale(float minRenderScale = 0.7f, float maxRenderScale = 0.91f, bool auto = false, int target = 1280)
        {
            float renderScale = target * 1.0f / Screen.width;
            renderScale = auto ? Mathf.Clamp(renderScale, minRenderScale, maxRenderScale) : minRenderScale;

            int test = (int)(renderScale * 100);
            var renderPipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            renderPipeline.renderScale = test * 0.01f;
            Debug.Log($"====> renderScale : {renderPipeline.renderScale}, Screen.width = {Screen.width}, Screen.height = {Screen.height}");
        }

        private void ChangeQualityLevelBaseDevicePerformance()
        {
            var level = GetDevicePerformanceLevel();
            ApplyQualitySettings(level);
        }

        private void ApplyQualitySettings(DevicePerformanceLevel performanceLevel)
        {
            if (qualitySettings == null)
            {
                return;
            }

            switch (performanceLevel)
            {
                case DevicePerformanceLevel.Low:
                    qualitySettings.LowQualitySetting();
                    break;
                
                case DevicePerformanceLevel.Middle:
                    qualitySettings.MiddleQualitySetting();
                    break;
                
                case DevicePerformanceLevel.High:
                    qualitySettings.HighQualitySetting();
                    break;
                
                case DevicePerformanceLevel.Super:
                    qualitySettings.SuperQualitySetting();
                    break;
                
                default:
                    qualitySettings.MiddleQualitySetting();
                    break;
            }
        }

        /// <summary>
        /// 获取本机硬件性能级别
        /// </summary>
        /// <returns></returns>
        private DevicePerformanceLevel GetDevicePerformanceLevel()
        {
            int graphicsMemory = SystemInfo.graphicsMemorySize;
            int systemMemorySize = SystemInfo.systemMemorySize;
            int graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID;
            
            Debug.Log($"{SystemInfo.systemMemorySize}, {SystemInfo.processorCount}, {SystemInfo.processorType}, {SystemInfo.processorFrequency}");
            Debug.Log($"{SystemInfo.graphicsDeviceVendorID}, {SystemInfo.graphicsMemorySize}, {SystemInfo.graphicsDeviceName}, {SystemInfo.graphicsDeviceType}");
            Debug.Log($"{SystemInfo.graphicsDeviceVendor}, {SystemInfo.graphicsDeviceVersion}, {SystemInfo.graphicsMultiThreaded}, {SystemInfo.graphicsShaderLevel}");
            Debug.Log($"{SystemInfo.graphicsDeviceID}, {SystemInfo.graphicsUVStartsAtTop}, {SystemInfo.supportsGraphicsFence}");

            //集显
            if (graphicsDeviceVendorID == 32902)
            {
                return DevicePerformanceLevel.Low;
            }

#if UNITY_IPHONE
            if (SystemInfo.processorCount < 2)
#elif UNITY_ANDROID
            if (SystemInfo.processorCount < 4)
#endif
            {
                return DevicePerformanceLevel.Low;
            }

#if UNITY_IPHONE
            if (graphicsMemory >= 2000 && systemMemorySize >= 4000)
            {
                return DevicePerformanceLevel.High;
            }
            else if (graphicsMemory >= 500 && systemMemorySize >= 1000)
            {
                return DevicePerformanceLevel.Middle;
            }
#endif
            
#if UNITY_ANDROID
            if (graphicsMemory >= 4000 && systemMemorySize >= 6000)
            {
                return DevicePerformanceLevel.High;
            }
            else if (graphicsMemory >= 1000 && systemMemorySize >= 2000)
            {
                return DevicePerformanceLevel.Middle;
            }
#endif
            
            return DevicePerformanceLevel.Low;
        }

    }
}