using UnityEngine;

namespace DH.Data
{
    /// <summary>
    /// 这里存玩家的本地数据，
    /// </summary>
    public class Userinfo : BaseData
    {

        /// <summary>
        /// 音效 开关 true 表示开 false 表示关
        /// </summary>
        public bool EffectState
        {
            get
            {
               var on=  DHUnityUtil.PlayerPrefs.GetInt(GameConst.UserInfoCode.EffectState, 1) ;
               return on == 1;
            }
            set
            {
                DHUnityUtil.PlayerPrefs.SetInt(GameConst.UserInfoCode.EffectState, value ? 1 : 0);
                DHUnityUtil.PlayerPrefs.Save();
            }
           
        }

        /// <summary>
        /// 音乐 开关 true 表示开 false 表示关
        /// </summary>
        public bool MusicState
        {
            get
            {
                int on = DHUnityUtil.PlayerPrefs.GetInt(GameConst.UserInfoCode.MusicState, 1);
                return on == 1;
            }
            set
            {
                DHUnityUtil.PlayerPrefs.SetInt(GameConst.UserInfoCode.MusicState, value ? 1 : 0);
                DHUnityUtil.PlayerPrefs.Save();
            }
        
        }
    }
}