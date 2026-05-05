using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DH.UIFramework.Observables;
using System.ComponentModel;
using System.Reflection;
using DH.UIFramework.Messaging;
using DHFramework;

namespace DH.UIFramework.ViewModels
{
    public abstract class ViewModelBase : ObservableObject, IViewModel
    {
        /// <summary>
        /// 释放当前脚本内继承ViewModelBase的子类（注意事项：目前支持的 数据结构:ObservableList,ObservableDictionary,Array，List，Dictionary，HashSet，Stack，Queue 如果需要其他数据结构,请自行添加）
        /// </summary>
        public virtual bool AutoDispose => false;
        private IMessenger messenger;

        /// <summary>
        /// 绑定数据层Collection到VM层，代码由编译器自动生成，此处只用于代码生成所需要的参数
        /// </summary>
        /// <param name="source">原始数据，通常为数据中心的容器，不能使用局部变量</param>
        /// <param name="target">ViewModel上的容器</param>
        /// <param name="func">构造VM上对象的工厂，可以是Lambda表达式或者函数，建议使用成员函数</param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        protected static void BindCollection<TSource, TTarget>(ObservableList<TSource> source,
            ObservableList<TTarget> target, Func<TSource, TTarget> func)
        {
        }

        protected static void BindCollection<TSource, TTarget>(ObservableList<TSource> source,
            ObservableList<TTarget> target,
            Action<TSource> addFunc,
            Action<TSource> removeFunc,
            Action clearFunc)
        {
        }

        /// <summary>
        /// 绑定数据层Collection到VM层，代码由编译器自动生成，此处只用于代码生成所需要的参数
        /// </summary>
        /// <param name="source">原始数据，通常为数据中心的容器，不能使用局部变量</param>
        /// <param name="target">ViewModel上的容器</param>
        /// <param name="func">构造VM上对象的工厂，可以是Lambda表达式或者函数，建议使用成员函数</param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        protected static void BindCollection<TKey, TSource, TTarget>(ObservableDictionary<TKey, TSource> source,
            ObservableDictionary<TKey, TTarget> target, Func<TKey, TSource, TTarget> func)
        {
        }

        /// <summary>
        /// 绑定数据层Collection到VM层，代码由编译器自动生成，此处只用于代码生成所需要的参数
        /// 适用于全部自定义的数据管理
        /// </summary>
        /// <param name="source">原始数据，通常为数据中心的容器，不能使用局部变量</param>
        /// <param name="target">ViewModel上的容器</param>
        /// <param name="addFunc">添加行为</param>
        /// <param name="removeFunc">移除行为</param>
        /// <param name="clearFunc">清理全部对象行为</param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        protected static void BindCollection<TKey, TSource, TTarget>(ObservableDictionary<TKey, TSource> source,
            ObservableDictionary<TKey, TTarget> target,
            Action<TKey, TSource> addFunc,
            Action<TKey, TSource> removeFunc,
            Action clearFunc)
        {
        }


        protected static void BindCollection<TKey, TSource, TTarget>(ObservableDictionary<TKey, TSource> source,
            ObservableList<TTarget> target,
            Action<TKey, TSource> addFunc,
            Action<TKey, TSource> removeFunc,
            Action clearFunc)
        {
        }

        public ViewModelBase() : this(null)
        {
            ViewModelTracker.TrackActiveViewModel(this);
        }

        public ViewModelBase(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public virtual IMessenger Messenger
        {
            get => messenger;
            set => messenger = value;
        }

        protected void Broadcast<T>(T oldValue, T newValue, string propertyName)
        {
            try
            {
                var messenger = Messenger;
                if (messenger != null)
                    messenger.Publish(new PropertyChangedMessage<T>(this, oldValue, newValue, propertyName));
            }
            catch (Exception e)
            {
                DHLog.Error("Set property '{0}', broadcast messages failure.Exception:{1}", propertyName, e);
            }
        }

        /// <summary>
        /// Set the specified propertyName, field, newValue and broadcast.
        /// </summary>
        /// <param name="field">Field.</param>
        /// <param name="newValue">New value.</param>
        /// <param name="propertyExpression">Expression of property name.</param>
        /// <param name="broadcast">If set to <c>true</c> broadcast.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        protected bool Set<T>(ref T field, T newValue, Expression<Func<T>> propertyExpression, bool broadcast)
        {
            if (Equals(field, newValue))
                return false;

            var oldValue = field;
            field = newValue;
            var propertyName = ParserPropertyName(propertyExpression);
            RaisePropertyChanged(propertyName);

            if (broadcast)
                Broadcast(oldValue, newValue, propertyName);
            return true;
        }

        /// <summary>
        ///  Set the specified propertyName, field, newValue and broadcast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        /// <param name="broadcast"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T field, T newValue, string propertyName, bool broadcast)
        {
            if (Equals(field, newValue))
                return false;

            var oldValue = field;
            field = newValue;
            RaisePropertyChanged(propertyName);

            if (broadcast)
                Broadcast(oldValue, newValue, propertyName);
            return true;
        }

        /// <summary>
        ///  Set the specified propertyName, field, newValue and broadcast.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="eventArgs"></param>
        /// <param name="broadcast"></param>
        /// <returns></returns>
        protected bool Set<T>(ref T field, T newValue, PropertyChangedEventArgs eventArgs, bool broadcast)
        {
            if (Equals(field, newValue))
                return false;

            var oldValue = field;
            field = newValue;
            RaisePropertyChanged(eventArgs);

            if (broadcast)
                Broadcast(oldValue, newValue, eventArgs.PropertyName);
            return true;
        }

        #region Unity LifeCycle

        public virtual void Update()
        {
        }

        public virtual void LateUpdate()
        {
        }

        #endregion

        #region IDisposable Support

        protected virtual void OnDispose()
        {
            if (AutoDispose) OnDisposeAll();

        }

        public void Dispose()
        {
            ViewModelTracker.RemoveTracking(this);
            OnDispose();
            GC.SuppressFinalize(this);
        }

        public void OnDisposeAll()
        {
            var classType = GetType();
            FieldInfo[] properties = classType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (FieldInfo  property in properties)
            {
                var propertyType = property.FieldType;
                if (propertyType.IsSubclassOf(typeof(ViewModelBase)))
                {
                    var value = property.GetValue(this) as ViewModelBase;
                    value?.Dispose();
                }
                
                
                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(List<>)  
                                                   || propertyType.GetGenericTypeDefinition() == typeof(ObservableList<>)))
                {
                    IList list = (IList)property.GetValue(this);

                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if(item is ViewModelBase value)
                                value?.Dispose();
                        }
                    }
                }
                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)  
                                                   || propertyType.GetGenericTypeDefinition() == typeof(ObservableDictionary<,>)))
                {
                    IDictionary list = (IDictionary)property.GetValue(this);
                    if (list != null)
                    {
                        foreach (DictionaryEntry  item in list)
                        {
                            if (item.Key is ViewModelBase key)
                            {
                                key?.Dispose();
                            }
                            
                            if (item.Value is ViewModelBase value)
                            {
                                value?.Dispose();
                            }
                        }
                    }
                }
                
                if (propertyType.IsArray && propertyType.GetElementType()!.IsSubclassOf(typeof(ViewModelBase)))
                {
                    Array array = (Array)property.GetValue(this);
                    if (array != null)
                    {
                        foreach (var item in array)
                        {
                            if (item is ViewModelBase value)
                                value?.Dispose();
                        }
                    }
                }

                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(HashSet<>)
                                                || propertyType.GetGenericTypeDefinition() == typeof(Queue<>)
                                                || propertyType.GetGenericTypeDefinition() == typeof(Stack<>)))
                {
                    IEnumerable set = (IEnumerable)property.GetValue(this);
                    if (set != null)
                    {
                        foreach (var item in set)
                        {
                            if (item is ViewModelBase value)
                                value?.Dispose();
                        }
                    }
                }
            }
        }
        #endregion
    }
}