using System.Collections.Generic;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public class TimeManager : Singleton<TimeManager>
    {
        private readonly Dictionary<string, float> timescaleContainer = new Dictionary<string, float>();

        private void Refresh()
        {
            float value = 1.0f;
            foreach (var item in timescaleContainer)
            {
                value *= item.Value;
            }

            Time.timeScale = value;
        }
        
        public void AddTimeScale(string key, float value)
        {
            timescaleContainer[key] = value;
            Refresh();
        }

        public void RemoveTimeScale(string key)
        {
            timescaleContainer.Remove(key);
            Refresh();
        }

        public void RemoveAll()
        {
            timescaleContainer.Clear();
            Time.timeScale = 1.0f;
        }
    }
}