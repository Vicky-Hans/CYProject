using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.UIFramework.ViewModels;

namespace DH.UIFramework
{
    public static class ViewModelTracker
    {
#if UNITY_EDITOR
        static int trackingId = 0;
        public const string EnableTrackingKey = "ViewModelTrackerWindow_EnableTrackingKey";
        static bool enableTracking;
        public static bool EnableTracking
        {
            get { return enableTracking; }
            set
            {
                enableTracking = value;
                UnityEditor.EditorPrefs.SetBool(EnableTrackingKey, value);
            }
        }
#endif
        static List<KeyValuePair<ViewModelBase, (string formattedType, int trackingId, DateTime addTime, string
            stackTrace)>> listPool = new();

        static readonly WeakDictionary<ViewModelBase, (string formattedType, int trackingId, DateTime addTime, string
                stackTrace)>
            tracking = new();


        [Conditional("UNITY_EDITOR")]
        public static void TrackActiveViewModel(ViewModelBase viewModel, int skipFrame = 2)
        {
#if UNITY_EDITOR
            dirty = true;
            if (!EnableTracking)
            {
                return;
            }
            var stackTrace = new StackTrace(skipFrame, true).CleanupAsyncStackTrace();
            var sb = new StringBuilder();
            TypeBeautify(viewModel.GetType(), sb);
            string typeName = sb.ToString();
            tracking.TryAdd(viewModel, (typeName, Interlocked.Increment(ref trackingId), DateTime.UtcNow, stackTrace));
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void RemoveTracking(ViewModelBase task)
        {
#if UNITY_EDITOR
            dirty = true;
            if (!EnableTracking)
            {
                return;
            }
            var success = tracking.TryRemove(task);
#endif
        }

        static bool dirty;

        public static bool CheckAndResetDirty()
        {
            var current = dirty;
            dirty = false;
            return current;
        }

        /// <summary>(trackingId, awaiterType, awaiterStatus, createdTime, stackTrace)</summary>
        public static void ForEachActiveTask(Action<int, string, DateTime, string> action)
        {
            lock (listPool)
            {
                var count = tracking.ToList(ref listPool, clear: false);
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        action(listPool[i].Value.trackingId, listPool[i].Value.formattedType, listPool[i].Value.addTime, listPool[i].Value.stackTrace);
                        listPool[i] = default;
                    }
                }
                catch
                {
                    listPool.Clear();
                    throw;
                }
            }
        }

        static void TypeBeautify(Type type, StringBuilder sb)
        {
            if (type.IsNested)
            {
                // TypeBeautify(type.DeclaringType, sb);
                sb.Append(type.DeclaringType.Name.ToString());
                sb.Append(".");
            }

            if (type.IsGenericType)
            {
                var genericsStart = type.Name.IndexOf("`");
                if (genericsStart != -1)
                {
                    sb.Append(type.Name.Substring(0, genericsStart));
                }
                else
                {
                    sb.Append(type.Name);
                }

                sb.Append("<");
                var first = true;
                foreach (var item in type.GetGenericArguments())
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }

                    first = false;
                    TypeBeautify(item, sb);
                }

                sb.Append(">");
            }
            else
            {
                sb.Append(type.Name);
            }
        }
    }
}