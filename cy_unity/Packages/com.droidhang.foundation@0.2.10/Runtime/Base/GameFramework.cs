using System;
using System.Collections.Generic;
using UnityEngine;
using DH.NativeCore.MonoSingleton;

namespace DHFramework
{
    public class GameFramework : MonoSingleton<GameFramework>
    {
        private static bool init;
        private Dictionary<Type, IGameFrameworkComponent> updatersContainer = new Dictionary<Type, IGameFrameworkComponent>();
        private HashSet<Type> pendingList = new HashSet<Type>();
        private Dictionary<Type, IGameModuleLateUpdater> lateUpdatersContainer = new Dictionary<Type, IGameModuleLateUpdater>();
        private HashSet<Type> lateUpdatePendingList = new HashSet<Type>();
        private readonly UnityThreadModule threadModule = new UnityThreadModule();

        public UnityThreadModule ThreadModule => threadModule;

        protected override void Init()
        {
            if (init)
            {
                throw new GameFrameworkException("Already created GameFramework Manager,Please confirm use this correct");
            }

            init = true;
        }

        private void Update()
        {
            if (pendingList.Count > 0)
            {
                foreach (var item in pendingList)
                {
                    updatersContainer.Remove(item);
                }
                pendingList.Clear();
            }
            
            float deltaTime = Time.deltaTime;
            float realDeltaTime = Time.unscaledDeltaTime;
            GameFrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
            foreach (var item in updatersContainer)
            {
                item.Value.Update(deltaTime, realDeltaTime);
            }
            
            threadModule.Update(deltaTime,realDeltaTime);
        }

        private void LateUpdate()
        {
            if (lateUpdatePendingList.Count > 0)
            {
                foreach (var item in lateUpdatePendingList)
                {
                    lateUpdatersContainer.Remove(item);
                }
                lateUpdatePendingList.Clear();
            }
            
            foreach (var item in lateUpdatersContainer)
            {
                item.Value.LateUpdate();
            }
        }

        protected override void Release()
        {
            foreach (var item in updatersContainer)
            {
                item.Value.Shutdown();
            }
            updatersContainer.Clear();
            lateUpdatersContainer.Clear();
            GameFrameworkEntry.Shutdown();
        }

        public T GetModule<T>() where T : IGameFrameworkComponent
        {
            var type = typeof(T);
            if (updatersContainer.ContainsKey(type))
            {
                return (T)updatersContainer[type];
            }

            // try get sigleton
            var property = type.BaseType?.GetProperty("Instance", System.Reflection.BindingFlags.Static
                                                        | System.Reflection.BindingFlags.Public
                                                        | System.Reflection.BindingFlags.GetProperty);
            IGameFrameworkComponent module = null;
            if (property != null)
            {
                module = (T)property.GetValue(null);
            }
            else
            {
                module = Activator.CreateInstance<T>();
            }

            updatersContainer.Add(type, module);

            return (T)module;
        }

        public T GetLateUpdateModule<T>() where T : IGameModuleLateUpdater
        {
            var type = typeof(T);
            if (lateUpdatersContainer.ContainsKey(type))
            {
                return (T)lateUpdatersContainer[type];
            }

            // try get sigleton
            var property = type.BaseType?.GetProperty("Instance", System.Reflection.BindingFlags.Static
                                                                  | System.Reflection.BindingFlags.Public
                                                                  | System.Reflection.BindingFlags.GetProperty);
            IGameModuleLateUpdater module = null;
            if (property != null)
            {
                module = (T)property.GetValue(null);
            }
            else
            {
                module = Activator.CreateInstance<T>();
            }
            
            lateUpdatersContainer.Add(type, module);

            return (T)module;
        }

        public void Release<T>()
        {
            var type = typeof(T);
            if (!pendingList.Contains(type))
            {
                if (updatersContainer.TryGetValue(type, out var component))
                {
                    component.Shutdown();
                }
                pendingList.Add(type);
            }
            
            if (!lateUpdatePendingList.Contains(type))
            {
                lateUpdatePendingList.Add(type);
            }
        }
    }
}