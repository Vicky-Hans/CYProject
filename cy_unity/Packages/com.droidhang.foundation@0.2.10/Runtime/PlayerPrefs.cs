namespace DHUnityUtil
{
    public static class PlayerPrefs
    {
        public static IPlayerPrefs Instance { get; set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        public static void SetInt(string key, int value)
        {
            Instance.SetInt(key, value);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            return Instance.GetInt(key, defaultValue);
        }

        public static void SetString(string key, string value)
        {
            Instance.SetString(key, value);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return Instance.GetString(key, defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            Instance.SetFloat(key, value);
        }

        public static float GetFloat(string key, float defaultValue = 0)
        {
            return Instance.GetFloat(key, defaultValue);
        }

        public static void DeleteAll()
        {
            Instance.DeleteAll();
        }

        public static void DeleteKey(string key)
        {
            Instance.DeleteKey(key);
        }

        public static bool HasKey(string key)
        {
            return Instance.HasKey(key);
        }

        public static void Save()
        {
            Instance.Save();
        }
#else
        public static void SetInt(string key, int value)
        {
            UnityEngine.PlayerPrefs.SetInt(key, value);
        }
        public static int GetInt(string key, int defaultValue = 0)
        {
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
        }
        public static void SetString(string key, string value)
        {
            UnityEngine.PlayerPrefs.SetString(key, value);
        }
        public static string GetString(string key, string defaultValue = "")
        {
            return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
        }
        public static void SetFloat(string key, float value)
        {
            UnityEngine.PlayerPrefs.SetFloat(key, value);
        }
        public static float GetFloat(string key, float defaultValue = 0)
        {
            return UnityEngine.PlayerPrefs.GetFloat(key, defaultValue);
        }
        public static void DeleteAll()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }
        public static void DeleteKey(string key)
        {
            UnityEngine.PlayerPrefs.DeleteKey(key);
        }
        public static bool HasKey(string key)
        {
            return UnityEngine.PlayerPrefs.HasKey(key);
        }
        public static void Save()
        {
            UnityEngine.PlayerPrefs.Save();
        }
#endif
        
    }

    public interface IPlayerPrefs
    {
        void SetInt(string key, int value);

        int GetInt(string key, int defaultValue = 0);

        void SetString(string key, string value);

        string GetString(string key, string defaultValue = "");

        void SetFloat(string key, float value);

        float GetFloat(string key, float defaultValue = 0);

        void DeleteAll();

        void DeleteKey(string key);

        bool HasKey(string key);

        void Save();
    }
}