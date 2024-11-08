using System;
using System.Collections.Generic;
using DH.Data;
using DHFramework;
namespace DH.Game
{
    public enum TimerState {
        Pending = 0,
        Running = 1,
        Removed = 2
    }

    public class Timer {
        public long Id;
        public Action action;
        public float interval; // s
        public int repeat;
        public TimerState state;
        public float passTime;
        public int times;
        public string Tag { get; set; }
    }
    public class TimerManager : Singleton<TimerManager>
    {
        private static long tuuid = 1;
        private Dictionary<long, Timer> runningTimer = new();
        private Dictionary<long, Timer> pendingTimer = new();
        private List<Timer> tmpTimerList = new();
        
        public void Update(float deltaTime)
        {
            if (GameTime.Instance.Pause) return;
            Run(deltaTime);
        }
        public Timer FindTimer(long id)
        {
            return runningTimer.GetValueOrDefault(id);
        }

        public long AddTimer(Timer timer)
        {
            if (FindTimer(timer.Id) != null)
            {
                DHLog.Warning($"repeated timer id: {timer.Id}");
                return -1;
            }
            pendingTimer.Add(timer.Id, timer);
            return timer.Id;
        }

        public long AddTimer(Action action, float interval, float delay = 0f, int repeat = 1, string tag = "")
        {
            var timer = new Timer
            {
                Id = tuuid++,
                action = action,
                interval = interval,
                repeat = repeat,
                passTime = interval - delay,
                state = TimerState.Pending,
                times = 0,Tag = tag
            };
            return AddTimer(timer);
        }
        public void RemoveTimerByTag(string tagName)
        {
            if (String.Compare(tagName, "", StringComparison.Ordinal) == 0) return;
            foreach (var item in pendingTimer)
            {
                var timer = item.Value;
                if (String.Compare(timer.Tag, tagName, StringComparison.Ordinal) == 0)
                {
                    timer.state = TimerState.Removed;
                }
            }
            foreach (var item in runningTimer)
            {
                var timer = item.Value;
                if (String.Compare(timer.Tag, tagName, StringComparison.Ordinal) == 0)
                {
                    timer.state = TimerState.Removed;
                }
            }
        }
        private void Run(float dt)
        {
            foreach (var item in runningTimer)
            {
                var timer = item.Value;
                if (timer.state == TimerState.Removed) continue;
                timer.passTime += dt;
                if (timer.passTime >= timer.interval)
                {
                    timer.times++;
                    timer.passTime = 0f;
                    timer.action?.Invoke();
                }
                if (timer.repeat == -1)  continue;
                if (timer.repeat <= timer.times) timer.state = TimerState.Removed;
            }
            tmpTimerList.Clear();
            foreach (var item in runningTimer)
            {
                var timer = item.Value;
                if (timer.state == TimerState.Removed)
                {
                    tmpTimerList.Add(timer);
                }
            }

            foreach (var timer in tmpTimerList)
            {
                runningTimer.Remove(timer.Id);
            }

            tmpTimerList.Clear();
            foreach (var item in pendingTimer)
            {
                var timer = item.Value;
                tmpTimerList.Add(timer);
                timer.state = TimerState.Running;
                runningTimer.Add(timer.Id, timer);
            }
            foreach (var timer in tmpTimerList)
            {
                pendingTimer.Remove(timer.Id);
            }
        }
    }
}