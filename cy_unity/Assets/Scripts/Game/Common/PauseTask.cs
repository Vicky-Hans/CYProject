using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Data;

namespace DH.Game
{
    public class PauseTask
    {
        public class PauseTaskItem
        {
            public AutoResetUniTaskCompletionSource<bool> cts;

            public void Complete()
            {
                cts?.TrySetResult(true);
                RemoveItem(this);    
            }
        }

        private static List<PauseTaskItem> ItemList = new();

        public static void RemoveItem(PauseTaskItem item)
        {
            ItemList.Remove(item);
        }

        public static void RemoveAllItem()
        {
            ItemList.ForEach(item =>
            {
                if (!item.cts.Task.Status.IsCompleted())
                {
                    item.cts.TrySetResult(false);
                }
            });
            ItemList.Clear();
        }

        /// <summary>
        /// 等待seconds秒
        /// </summary>
        /// <param name="seconds"></param>
        public static async UniTask<bool> Delay(float seconds)
        {
            var item = new PauseTaskItem()
            {
                cts = AutoResetUniTaskCompletionSource<bool>.Create(),
            };
            TimerManager.Instance.AddTimer(item.Complete, 65535f, seconds, 1, GameConst.TimerTagName.PauseTask);
            ItemList.Add(item);
            return await item.cts.Task;
        }
    }
}