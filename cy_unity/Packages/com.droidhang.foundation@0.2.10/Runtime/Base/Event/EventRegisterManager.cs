using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DHFramework;
using Debug = UnityEngine.Debug;

namespace DH.Foundations.Event
{
    public class EventRegisterManager : Singleton<EventRegisterManager>
    {
        /// <summary>
        /// 注册字典容器
        /// </summary>
        private Dictionary<string, Delegate> _CallbackDelegate;

        private Dictionary<string, Delegate> _Delegate;

        public Dictionary<string, Delegate> EventDic
        {
            get => _CallbackDelegate;
            private set => _CallbackDelegate = value;
        }

        public void Initialize()
        {
            _CallbackDelegate = new Dictionary<string, Delegate>();
            _Delegate = new Dictionary<string, Delegate>();
            _registedClass = new List<Type>();

            //注册自动化
            InitSerachAssembly();
            //开始注册，且限定优先级
            RegEventSystem();
        }

        /// <summary>
        /// 重写的初始化函数
        /// </summary>
        protected override void Initialization()
        {
        }

        /// <summary>
        /// 初始化注册系统
        /// </summary>
        private void RegEventSystem()
        {
            foreach (var dic in _CallbackDelegate)
            {
                AddListener(dic.Key, dic.Value, _CallbackDelegate);
            }

            foreach (var dic in _Delegate)
            {
                AddListener(dic.Key, dic.Value, _Delegate);
            }
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="eventValue"></param>
        /// <param name="callbackDelegate"></param>
        internal void AddListener(string eventKey, Delegate eventValue, Dictionary<string, Delegate> callbackDelegate)
        {
            //Debug.Log($"AddListener eventKey->{eventKey}");
            //查看Delegate的参数类型和个数，针对注册
            //判定有无参数
            var delegateType = eventValue.GetType();
            if (delegateType != typeof(Action))
            {
                EventDispatcher.RegEventListener(eventKey, eventValue);
            }
            else
            {
                var handle = GetAction(eventKey, eventValue, callbackDelegate);
                EventDispatcher.RegEventListener(eventKey, handle);
            }
        }

        /// <summary>
        /// 回收监听
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="eventValue"></param>
        /// <param name="callbackDelegate"></param>
        internal void RemoveListener(string eventKey, Delegate eventValue,
            Dictionary<string, Delegate> callbackDelegate)
        {
            if (eventValue.GetType() != typeof(Action))
            {
                var handle = GetAction<string>(eventKey, eventValue, callbackDelegate);
                EventDispatcher.UnRegEventListener<string>(eventKey, handle);
            }
            else
            {
                var handle = GetAction(eventKey, eventValue, callbackDelegate);
                EventDispatcher.UnRegEventListener(eventKey, handle);
            }
        }


        /// <summary>
        /// 所有打过注册标签的类列表
        /// </summary>
        private List<Type> _registedClass;
        private void InitSerachAssembly()
        {
            //开始注册(需要获取到所有程序集)
            var dhAssemblies = DHUtility.Assembly.GetAssemblies();
            var list = dhAssemblies.ToList();
            Type[] allTypes = null;
            list.ForEach((dhassembly) =>
            {
                allTypes = dhassembly.GetTypes();
                foreach (var type in allTypes)
                {
                    //过滤获取到添加了自定义标签的类
                    if (type.IsDefined(typeof(ClassRegistAttribute), true))
                    {
                        //Debug.Log($"IsDefined->{type}");
                        _registedClass.Add(type);
                    }
                }
            });

            RegisterDelegate();
        }

        private void RegisterDelegate()
        {
            _registedClass.ForEach((type) =>
            {
#if UNITY_EDITOR
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Static);
#else
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Static);
#endif
                for (int i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    if (methods[i].IsDefined(typeof(DHServiceProviderAttribute)))
                    {
                        if (!method.IsStatic)
                        {
                            throw new Exception($@"DHServiceProviderAttribute 只支持静态函数注册 {method.Name} 不是静态函数");
                        }

                        try
                        {
                            var attr = methods[i].GetCustomAttribute<DHServiceProviderAttribute>();
                            //要注册的调用函数
                            var actionT = GetActionType(methods[i].GetParameters());
                            DisplayGenericMethodInfo(methods[i]);
                            Delegate deleGate = Delegate.CreateDelegate(actionT, methods[i]);
                            _Delegate.Add(attr.ServiceName, deleGate);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"DHServiceProviderAttribute CreateDelegate Error->{methods[i]},ex->{ex}");
                        }
                    }

                    if (methods[i].IsDefined(typeof(DHServiceCallbackAttribute), true))
                    {
                        if (!method.IsStatic)
                        {
                            throw new Exception($@"DHServiceProviderAttribute 只支持静态函数注册 {method.Name} 不是静态函数");
                        }

                        try
                        {
                            var attr = methods[i].GetCustomAttribute<DHServiceCallbackAttribute>();
                            var name = attr.ServiceName + "Callback";
                            //要注册的回调函数
                            var paras = methods[i].GetParameters();
                            var actionT = GetActionType(methods[i].GetParameters());
                            //var actionT = typeof(Action<>).MakeGenericType(paras[0].ParameterType);
                            DisplayGenericMethodInfo(methods[i]);
                            Delegate deleGate = Delegate.CreateDelegate(actionT, methods[i]);
                            //Debug.Log($"CallbackName->{name}");
                            _CallbackDelegate.Add(name, deleGate);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"DHServiceCallbackAttribute CreateDelegate Error->{methods[i].Name},ex->{ex}");
                        }
                    }
                }
            });
        }

        private Action<T> GetAction<T>(string eventKey, Delegate eventValue,
            Dictionary<string, Delegate> callbackDic)
        {
            Action<T> handle = null;
            handle = eventValue as Action<T>;

            // Action<T> handle = (Action<T>) Delegate.Combine(
            //     (Action<T>) callbackDic[eventKey], eventValue);

            return handle;
        }

        private Action GetAction(string eventKey, Delegate eventValue,
            Dictionary<string, Delegate> callbackDic)
        {
            Action handle = null;
            handle = eventValue as Action;
            // (Action) Delegate.Combine(
            // (Action) callbackDic[eventKey], eventValue);

            return handle;
        }

        private object CreateParamInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }

        private object CreateParamInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }


        /// <summary>
        /// 根据委托类型生成对应的Action Type
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        private Type GetActionType(ParameterInfo[] paras)
        {
            Type type = null;
            //无参数的委托创建
            if (paras == null || paras.Length == 0)
            {
                type = typeof(Action);
            }

            switch (paras.Length)
            {
                case 1:
                    type = typeof(Action<>).MakeGenericType(paras[0].ParameterType);
                    break;
                case 2:
                    type = typeof(Action<,>).MakeGenericType(paras[0].ParameterType, paras[1].ParameterType);
                    break;
                case 3:
                    type = typeof(Action<,,>).MakeGenericType(paras[0].ParameterType, paras[1].ParameterType,
                        paras[2].ParameterType);
                    break;
                case 4:
                    type = typeof(Action<,,,>).MakeGenericType(paras[0].ParameterType, paras[1].ParameterType,
                        paras[2].ParameterType, paras[3].ParameterType);
                    break;
            }

            return type;
        }

        [Conditional("EVENT_DEBUG")]
        private static void DisplayGenericMethodInfo(MethodInfo mi)
        {
            Debug.Log(DHUtility.Format("\r\n{0}", mi));

            Debug.Log(DHUtility.Format("\tIs this a generic method definition? {0}",
                mi.IsGenericMethodDefinition));

            Debug.Log(DHUtility.Format("\tIs it a generic method? {0}",
                mi.IsGenericMethod));

            Debug.Log(DHUtility.Format("\tDoes it have unassigned generic parameters? {0}",
                mi.ContainsGenericParameters));

            // If this is a generic method, display its type arguments.
            //
            if (mi.IsGenericMethod)
            {
                Type[] typeArguments = mi.GetGenericArguments();

                Debug.Log(DHUtility.Format("\tList type arguments ({0}):",
                    typeArguments.Length));

                foreach (Type tParam in typeArguments)
                {
                    // IsGenericParameter is true only for generic type
                    // parameters.
                    //
                    if (tParam.IsGenericParameter)
                    {
                        Debug.Log(DHUtility.Format("\t\t{0}  parameter position {1}" +
                                                   "\n\t\t   declaring method: {2}",
                            tParam,
                            tParam.GenericParameterPosition,
                            tParam.DeclaringMethod));
                    }
                    else
                    {
                        Debug.Log(DHUtility.Format("\t\t{0}", tParam));
                    }
                }
            }
        }

        protected override void Release()
        {
            foreach (var dic in _CallbackDelegate)
            {
                RemoveListener(dic.Key, dic.Value, _CallbackDelegate);
            }

            foreach (var dic in _Delegate)
            {
                RemoveListener(dic.Key, dic.Value, _Delegate);
            }

            //Debug.Log("Release is begin~~~~~~~");
            //EventDispatcher.TriggerEvent<string>(EventCallback.OnNativeCloseCustomer,"wqekwjqyeashjdqwejkjhwqdbhkashd");
        }
    }
}