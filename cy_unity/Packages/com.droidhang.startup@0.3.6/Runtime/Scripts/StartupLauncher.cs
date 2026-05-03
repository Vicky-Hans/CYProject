using DHFramework;
namespace DH.Launch
{
    public class StartupLauncher
    {
        public static void LaunchEntry()
        {
            StartupEntry.Instance.Startup().Forget();
        }
        public static void Quit()
        {
            DHLog.Debug("[Startup] ShutDown");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

#if UNITY_WEBGL || WECHAT_MINI

        [RuntimeInitializeOnLoadMethod]
        static void InitStartupLauncherInstance()
        {
            GameRoot.StartupLauncher = LaunchEntry;
        }
#endif
        
    }
}
