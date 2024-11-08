using DHFramework;
using DHFramework.Localization;

namespace DH.Launch
{
    /// <summary>
    /// 负责管理启动配置相关
    /// </summary>
    public partial class StartupEntry
    {
        public StartupLauncherConfig StartupConfig { get; set; }
    
        /// <summary>
        /// 获取当前的语言
        /// </summary>
        /// <returns></returns>
        public string GetCurLanguageCode()
        {
            return Localization.GetCurrentLanguage();
        }
    
        /// <summary>
        /// 获取协议使用的语言当前的语言
        /// </summary>
        /// <returns></returns>
        public string GetAgreementCurLanguageCode()
        {
            var language = GetCurLanguageCode();

            if (StartupConfig.AgreementSupportLanguages.Contains(language))
            {
                return language;
            }

            return StartupConfig.AgreementSupportLanguages[0];
        }
        
        /// <summary>
        /// 获取隐私协议
        /// </summary>
        /// <returns></returns>
        public string GetPrivacyAgreement()
        {
            return DHUtility.Format(StartupConfig.PrivacyAgreement, GetAgreementCurLanguageCode());
        }
        
        /// <summary>
        /// 获取用户协议
        /// </summary>
        /// <returns></returns>
        public string GetUserAgreementUrl()
        {
            return DHUtility.Format(StartupConfig.UserAgreement, GetAgreementCurLanguageCode());
        }
    }
}
