using System;
using System.ComponentModel;
using DG.Tweening;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class BottomItemViewModel : ViewModelBase
    {
		// [AutoNotify] private Vector3 effectImgRectPos = new Vector3(0,10);
        [AutoNotify] private ObservableList<TabBtnItemViewModel> opBtnScrollviewList = new ();
        [AutoNotify] private RectTransform effectImgRect;
        private Action<ETabType> onClickTabCallback;
        private  Tweener effeectTweener;
        private float moveDuration = 0.1f;
        [Preserve]
        public BottomItemViewModel(Action<ETabType> clickTabCallback)
        {
            
            onClickTabCallback = clickTabCallback;
            // 这里确定有几个按钮
            foreach (ETabType tabType in Enum.GetValues(typeof(ETabType)))
            {
                var tempTab = new TabBtnItemViewModel(tabType, OnClickTabBtn);
                OpBtnScrollviewList.Add(tempTab);
            }
            MainUiManager.Instance.OnTabTypeChangeByJumpCallback = OnTabTypeChangeByJump;
        }

        protected override void OnDispose()
        {
            foreach (var item in OpBtnScrollviewList)
            {
                item.Dispose();
            }

            base.OnDispose();
        }
        
        private void OnClickTabBtn(ETabType tabType, Tuple<Vector3, Vector3> info)
        {
            // 播动效
            if(MainUiManager.Instance.CurTabType == tabType) return;
            onClickTabCallback.Invoke(tabType);
        }

        private void PlayEffectAnimation(Tuple<Vector3, Vector3> info)
        {
            // EffectImgRectPos = info.Item1;
            if(EffectImgRect == null) return;
            
            // 将世界坐标转换为节点坐标
            Vector3 nodePosition = effectImgRect.transform.parent.InverseTransformPoint(info.Item1);
            // 判断是否需要移动
            if( Math.Abs(nodePosition.x -effectImgRect.anchoredPosition.x) < 5.0f ) return;
            // 停止之前的动画
            if(effeectTweener != null)
            {
                effeectTweener.Kill();
            }
            // 开始移动
            effeectTweener = DOVirtual.Float(effectImgRect.anchoredPosition.x, nodePosition.x, moveDuration, v =>
            {
                // 设置目标节点的anchoredPosition
                EffectImgRect.anchoredPosition = new Vector3(v, effectImgRect.anchoredPosition.y);
            });
            // 添加完成回调
            effeectTweener.onComplete += OnEffectTweenComplete;

        }
        
        
        private void OnEffectTweenComplete()
        {
            // 停止动画
            effeectTweener.onComplete -= OnEffectTweenComplete;
            effeectTweener = null;
        }


        private void OnTabTypeChangeByJump(ETabType type)
        {
            foreach (var item in OpBtnScrollviewList)
            {
                if(item.TabType == type)
                {
                    if(item.CellIcon == null) break;
                    Tuple<Vector3, Vector3> info = new Tuple<Vector3, Vector3>(item.CellIcon.transform.position, Vector3.zero);
                    // 播放动画
                    PlayEffectAnimation(info);
                    break;
                }
            }
        }

        /// <summary>
        /// 获取tab 是否解锁
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool GetTabBtnIsUnLock(ETabType type)
        {
            bool ret = true;
            foreach (var item in OpBtnScrollviewList)
            {
                if (item.TabType != type) continue;
                ret = !item.IsLocked;
            }

            return ret;
        }

        /// <summary>
        /// 获取没解锁的提示
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetLockTabBtnTips(ETabType type)
        {
            var ret = "";
            foreach (var item in OpBtnScrollviewList)
            {
                if(item.TabType != type) continue;
                ret = item.GetLockTipsStr();
                break;
            }
            return ret;
        }
    }
}