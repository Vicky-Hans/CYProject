using System.Collections.Specialized;
using System.ComponentModel;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;


namespace DH.Game.ViewModels
{
    public partial class ShopChapterViewModel : ViewModelBase
    {
        [AutoNotify] private ObservableList<ShopChapterItemViewModel> scrollViewChapterList = new();
        [AutoNotify] private string endTImeValueStr;
		
        [AutoNotify] private int chapterDiftPosPage;
        [AutoNotify] private bool isShowChapter;

        [Preserve]
        public ShopChapterViewModel()
        {
            RefreshChapterGift();
            DataCenter.shopData.ChapterGift.CollectionChanged += ChapterGiftChanged;
            ShopManager.Instance.PropertyChanged += ShopManagerChanged;
            MainUiManager.Instance.PropertyChanged += OnMainUiChanged;
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.shopData.ChapterGift.CollectionChanged -= ChapterGiftChanged;
            ShopManager.Instance.PropertyChanged -= ShopManagerChanged;
            MainUiManager.Instance.PropertyChanged -= OnMainUiChanged;
            UIHelper.ViewModelBaseOnDisposes(scrollViewChapterList);
        }
        
        private void OnMainUiChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MainUiManager.Instance.CurTabType))
            {
                ShopManager.Instance.CurSelectChapterPos = GetChapterPagePos();
            }
        }

        private void ShopManagerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShopManager.CurSelectChapterPos))
            {
                ChapterDiftPosPage = ShopManager.Instance.CurSelectChapterPos;
            }
        }

        private void ChapterGiftChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshChapterGift();
        }
        private void RefreshChapterGift()
        {
            scrollViewChapterList.ClearAndDispose();
            int selelctIndex = 0;
            var chapterList = ShopManager.Instance.GetChapterGiftList();
            for (int i = 0; i < chapterList.Count; i++) 
            {
                if (!DataCenter.shopData.CheckChapterGift(chapterList[i].Id))
                {
             
                    scrollViewChapterList.Add(new ShopChapterItemViewModel(chapterList[i]));
                }
            }

            ShopManager.Instance.CurSelectChapterPos = GetChapterPagePos();
            ChapterDiftPosPage = ShopManager.Instance.CurSelectChapterPos;
            IsShowChapter = scrollViewChapterList.Count > 0;
	        
        }
        
        private int GetChapterPagePos()
        {
	        
            for (int i = scrollViewChapterList.Count-1; i >=0 ; i--)
            {
                if (DataCenter.mainStageData.IsPassChapter(scrollViewChapterList[i].Cfg.Condition))
                {
                    return i;
                }
            }

            return 0;
        }
        
        [Command]
        public void OnClickChapterRight()
        {
            if (ShopManager.Instance.CurSelectChapterPos < scrollViewChapterList.Count-1)
            {
                ShopManager.Instance.CurSelectChapterPos += 1;
            }
        }
        
        [Command]
        public void OnClickChapterLeft()
        {
            if (ShopManager.Instance.CurSelectChapterPos > 0)
            {
                ShopManager.Instance.CurSelectChapterPos -= 1;
            }
        }
        
    }
}