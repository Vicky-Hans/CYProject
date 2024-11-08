using System;
using System.Collections.Generic;
using DHFramework;

namespace DH.Game
{
    public class TimelineActionPlayer
    {
        public class ActionItem : IComparable<ActionItem>,IComparable
        {
            public float time;
            public Action action;
            
            public int CompareTo(ActionItem other)
            {
                return time.CompareTo(other.time);
            }

            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                action = null;
                time = 0;
            }
        }

        private readonly List<ActionItem> actionItems = new List<ActionItem>();
        private int currentIndex;
        private float timer;

        public void AddAction(Action action, float time)
        {
            var item = new ActionItem
            {
                action = action,
                time = time
            };
            actionItems.Add(item);
        }

        public void PrepareExecute()
        {
            timer = 0;
            currentIndex = 0;
            actionItems.Sort();
        }

        public bool OnUpdate(float deltaTime)
        {
            if (currentIndex >= actionItems.Count)
            {
                return false;
            }
            
            timer += deltaTime;
            var item = actionItems[currentIndex];
            if (item.time < timer)
            {
                item.action?.Invoke();
                currentIndex++;
            }

            return true;
        }

        public void Reset()
        {
            actionItems.Clear();
        }
    }
}