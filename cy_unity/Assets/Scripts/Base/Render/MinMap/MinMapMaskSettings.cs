using UnityEngine;

namespace TF.Base
{
    public static class MinMapMaskSettings
    {
        public static bool Enable { get; set; }
        //摄像机相对于整张地图的偏移量
        public static Vector4 MapRange { get; set; }
        /// <summary>
        /// 玩家的世界坐标
        /// </summary>
        public static Vector3 PlayerPosition { get; set; }

        /// <summary>
        /// 玩家光圈的内外半径
        /// </summary>
        public static Vector2 PlayerLightRadius { get; set; } = new Vector2(1, 1.5f);

        private static float fogCutRadius = 2;
        /// <summary>
        /// 控制玩家周围迷雾消散半径
        /// </summary>
        public static float FogCutRadius
        {
            set => fogCutRadius = Mathf.Max(value, 1);
            get => fogCutRadius;
        }
    }
}