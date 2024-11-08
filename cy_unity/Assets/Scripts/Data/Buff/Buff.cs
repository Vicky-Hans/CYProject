using DH.UIFramework.Observables;

namespace DH.Data
{
    public enum BuffValueType
    {
        Positive = 0,
        Negative = 1
    }
    public class Buff : ObservableObject
    {
        public int id;
        public string attrName;
        public float startTime;
        public float duration;
        public BuffValueType valueType;
        public float value;
        public bool multi;
        public Skill skillData;
        public int equipModelId;
        public int clothesId;//服饰Id
        public float interval;

        public float RemainTime(float currentTime)
        {
            return duration - currentTime + startTime;
        }

        public bool IsValid(float currentTime)
        {
            return !(RemainTime(currentTime) < 0);
        }
    }
}