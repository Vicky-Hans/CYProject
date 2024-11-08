using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ChapterCellView : BaseItemView
    {
        public override bool FullScreen => true;
        
		public DhImage chapterImg;
		public GameObject chapterEffectNode;
		public DhText chapterName;
		public DhButton infoBtn;
		public DhText recordText;
		public GameObject monsterParentNode;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ChapterCellView, ChapterCellViewModel>();
			bindingSet.Bind(chapterImg).For(v => v.sprite).To(vm => vm.ChapterImgPath).WithConversion(this);
			bindingSet.Bind(chapterImg.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowChapterEffectNode);
			bindingSet.Bind(chapterEffectNode).For(v => v.activeSelf).To(vm => vm.IsShowChapterEffectNode);
			bindingSet.Bind(this).For(v => v.chapterEffectNode).To(vm => vm.EffectParentNode).OneWayToSource();
			bindingSet.Bind(chapterName).For(v => v.text).To(vm => vm.ChapterNameStr);
			bindingSet.Bind(infoBtn).For(v => v.onClick).To(vm => vm.OnClickInfoBtnCommand);
			bindingSet.Bind(recordText).For(v => v.text).To(vm => vm.RecordTextStr);
			bindingSet.Bind(this).For(v=>v.monsterParentNode).To(vm=>vm.MonsterParentNode).OneWayToSource();
            bindingSet.Build();
            // 先隐藏这个详情按钮
            infoBtn.gameObject.SetActive(false);
        }
    }
}