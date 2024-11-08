using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Game;
using DH.Game.UIViews.ItemViews;
using Dh.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dh.Game.UIViews.ItemViews
{
    public partial class CollegeTaskView : BaseItemView
    {
        public TextMeshProUGUI timeDesc;
        public UICircularScrollView scrollViewTop;
        [AssetPath] public string topItemPath;
        public CommonTopView topItems;
        public TabBtnGroupTitleView tabBtnGroupTitleView;
        public UICircularScrollView scrollViewTask;
        [AssetPath] public string taskItemPath;
        public TextMeshProUGUI progressValue;
        [NonSerialized] public int IndexPos;
        public Button btnRule;

        public ParticleSystem moveParticle;
        public ParticleSystem endParticle;
        public Transform[] moveGruop;

        [NonSerialized] public int moveState;
        private Vector3 endPos;

        private Tween tweenPos;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            scrollViewTop.PrefabPath = topItemPath;
            scrollViewTask.PrefabPath = taskItemPath;
            var bindSet = this.CreateBindingSet<CollegeTaskView, CollegeTaskModel>();
            bindSet.Bind(timeDesc).For(v => v.text).To(vm => vm.EndTImeValueStr);
            bindSet.Bind(scrollViewTop).For(v => v.Collection).To(vm => vm.TopProgressModels);
            bindSet.Bind(scrollViewTask).For(v => v.Collection).To(vm => vm.TaskItemModels);
            bindSet.Bind(tabBtnGroupTitleView.BindingContext).For(v => v.DataContext).To(vm => vm.TabBtnGroupTitle);
            bindSet.Bind(topItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsModel);
            bindSet.Bind(progressValue).For(v => v.text).To(vm => vm.Score);
            bindSet.Bind(this).For(v => v.IndexPos).ToExpression(vm => RefreshIndex(vm.TopIndex));
            bindSet.Bind(btnRule).For(v => v.onClick).To(vm => vm.OnClickRuleCommand);
            bindSet.Bind(this).For(v => v.moveState).ToExpression(vm => MoveAnimationStart(vm.StartMoveState));
            bindSet.Build();
            endPos = endParticle.transform.position;
            DelayScrollToPos().Forget();
        }

        private int RefreshIndex(int vmTopIndex)
        {
            scrollViewTop.Jump2SpecificItem(vmTopIndex);
            return vmTopIndex;
        }
        
        private async UniTaskVoid DelayScrollToPos()
        {
            await UniTask.Delay(300);
            scrollViewTop.Jump2SpecificItem(IndexPos);
        }

        private int MoveAnimationStart(int state)
        {
            if (moveGruop.Length > state && state>=0)
            {
                MoveAnimation(moveGruop[state].position).Forget();
            }
            return state;
        }

        private async UniTaskVoid MoveAnimation(Vector3 startPos)
        {
            if (tweenPos != null && tweenPos.IsActive())
            {
                tweenPos.Kill();
            }
            moveParticle.transform.position = startPos;
            UIHelper.PlayEffect(moveParticle);
            tweenPos = moveParticle.transform.DOMove(endPos, 0.5f);
            await UniTask.Delay(500);
            UIHelper.StopEffect(moveParticle);
            UIHelper.PlayEffect(endParticle);
        }
    }
}