using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;

namespace DH.Data
{
    public enum SchedulerState {
        Pending = 0,
        Running = 1,
        Removed = 2
    }
    
    public class Scheduler {
        public long Id;
        // public GameObject Go;
        public Action action;
        public float interval; // s
        public float delay; // s
        public long repeat;
        public SchedulerState state;
        public float passTime;
        public long times;
        public string Tag { get; set; }
    }
    
    public class GlobalSchedule : Singleton<GlobalSchedule>
    {
        private static long suuid = 1;
        private Dictionary<long, Scheduler> runningScheduler = new();
        private Dictionary<long, Scheduler> pendingScheduler = new();
        private List<Scheduler> tmpSchedulerList = new();
        
        public Scheduler FindScheduler(long id)
        {
            if (runningScheduler.TryGetValue(id, out Scheduler scheduler))
            {
                return scheduler;
            }
            return null;
        }
        public Scheduler FindScheduler(string tag)
        {
            foreach (var item in runningScheduler)
            {
                if (item.Value.Tag == tag)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public void Clear()
        {
            pendingScheduler.Clear();
            runningScheduler.Clear();
            tmpSchedulerList.Clear();
        }

        public long AddScheduler(Scheduler scheduler)
        {
            if (FindScheduler(scheduler.Id) != null)
            {
                DHLog.Warning($"repeated scheduler id: {scheduler.Id}");
                return -1;
            }

            if (scheduler.repeat == -1)
            {
                scheduler.repeat = long.MaxValue;
            }
            pendingScheduler.Add(scheduler.Id, scheduler);
            return scheduler.Id;
        }

        public long AddScheduler(Action action, float interval, float delay = 0f,
            int repeat = 1, string tag = "")
        {
            var scheduler = new Scheduler();
            scheduler.Id = suuid++;
            scheduler.action = action;
            scheduler.interval = interval;
            scheduler.repeat = repeat;
            scheduler.Tag = tag;
            scheduler.passTime = scheduler.passTime - delay + interval;
            scheduler.delay = 0f;
            scheduler.state = SchedulerState.Pending;
            return AddScheduler(scheduler);
        }

        public void RemoveScheduler(long id)
        {
            var scheduler = FindScheduler(id);
            if (scheduler != null)
            {
                scheduler.state = SchedulerState.Removed;
            }
        }

        public void RemoveSchedulerByTag(string tagName)
        {
            if (String.Compare(tagName, "", StringComparison.Ordinal) == 0)
            {
                return;
            }

            foreach (var item in pendingScheduler)
            {
                var scheduler = item.Value;
                if (String.Compare(scheduler.Tag, tagName, StringComparison.Ordinal) == 0)
                {
                    scheduler.state = SchedulerState.Removed;
                }
            }
            foreach (var item in runningScheduler)
            {
                var scheduler = item.Value;
                if (String.Compare(scheduler.Tag, tagName, StringComparison.Ordinal) == 0)
                {
                    scheduler.state = SchedulerState.Removed;
                }
            }
        }

        private void Run(float dt)
        {
            foreach (var item in runningScheduler)
            {
                var scheduler = item.Value;
                if (scheduler.state == SchedulerState.Removed) continue;
                
                scheduler.passTime += dt;
                if (scheduler.passTime >= scheduler.interval)
                {
                    scheduler.times++;
                    scheduler.passTime = 0f;
                    scheduler.action?.Invoke();
                }

                if (scheduler.repeat <= scheduler.times)
                {
                    scheduler.state = SchedulerState.Removed;
                    continue;
                }
            }
            tmpSchedulerList.Clear();
            foreach (var item in runningScheduler)
            {
                var scheduler = item.Value;
                if (scheduler.state == SchedulerState.Removed)
                {
                    // runningScheduler.Remove(scheduler.Id);
                    tmpSchedulerList.Add(scheduler);
                }
            }

            foreach (var scheduler in tmpSchedulerList)
            {
                runningScheduler.Remove(scheduler.Id);
            }

            tmpSchedulerList.Clear();
            foreach (var item in pendingScheduler)
            {
                var scheduler = item.Value;
                tmpSchedulerList.Add(scheduler);
                scheduler.state = SchedulerState.Running;
                runningScheduler.Add(scheduler.Id, scheduler);
            }

            foreach (var scheduler in tmpSchedulerList)
            {
                pendingScheduler.Remove(scheduler.Id);
            }
        }
        
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            elapseSeconds /= Time.timeScale;
            Run(elapseSeconds);
        }

        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
        }

        public void Destroy()
        {
            Clear();
        }
    }
}