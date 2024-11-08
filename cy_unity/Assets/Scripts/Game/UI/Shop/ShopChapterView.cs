using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;

namespace DH.Game.UIViews
{
    public partial class ShopChapterView : BaseItemView
    {
        public override bool FullScreen => false;
        public FlipPageCircularScrollView scrollViewChapter;
        [AssetPath]public string scrollViewChapterCell;
        
        public DhButton btnLeft;
        public DhButton btnRight;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollViewChapter.m_FuncPageMoveEndCallBack = PageEndCallback;   
            scrollViewChapter.PrefabPath = scrollViewChapterCell;
            await base.Create();
            var bindSet = this.CreateBindingSet<ShopChapterView, ShopChapterViewModel>();
            bindSet.Bind(scrollViewChapter.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowChapter);
            bindSet.Bind(scrollViewChapter).For(v => v.Collection).To(vm => vm.ScrollViewChapterList);
            bindSet.Bind(scrollViewChapter).For(v => v.CurPageIndex).To(vm => vm.ChapterDiftPosPage);
            bindSet.Bind(btnLeft).For(v => v.onClick).To(vm => vm.OnClickChapterLeftCommand);
            bindSet.Bind(btnRight).For(v => v.onClick).To(vm => vm.OnClickChapterRightCommand);
            
            bindSet.Bind(btnLeft.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ChapterDiftPosPage>0);
            bindSet.Bind(btnRight.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ChapterDiftPosPage<vm.ScrollViewChapterList.Count-1);
            bindSet.Build();
        }
        
        private void PageEndCallback(int pageIdnex)
        {
            ShopManager.Instance.CurSelectChapterPos = pageIdnex;
        }
    }
}