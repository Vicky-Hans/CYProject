using System.Collections.Specialized;
using System.ComponentModel;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class EquipBattleItemViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private EquipSlotsCfg cfg;

        [AutoNotify] private bool isOwn;
        [AutoNotify] private bool isLock;
        [AutoNotify] private bool isReplace;

        [AutoNotify] private EquipItemViewModel itemViewModel;
        private EquipItemData itemData;
        public EquipItemData ItemData
        {
            get => itemData;
            set
            {
                var old = itemData;
                Set(ref itemData, value);
                if (old != null)
                {
                    old.PropertyChanged -= EquipItemDataChanged;
                }
                
                if (itemData != null)
                {
                    itemData.PropertyChanged += EquipItemDataChanged;
                }
            }
        }

        public EquipItemView EquipItemViewObj;
        public int playAnimationPos;
        [Preserve]
        public EquipBattleItemViewModel(EquipSlotsCfg cfg,int pos)
        {
            playAnimationPos = pos;
            Cfg = cfg;
            IsLock = !DataCenter.mainStageData.IsPassChapter(Cfg.Unlock);
            var equipData = EquipManager.Instance.GetEquipDataByPos(Cfg.Id);
            IsOwn = equipData != null;//DataCenter.equipData.IsOwn(cfg.Id);
            ItemViewModel = new EquipItemViewModel(equipData?.Id ?? EquipManager.Instance.BaseEquipId,IsOwn);
            RefreshReplace();
            DataCenter.equipData.Items.CollectionChanged += ChangeEquipItem;
            DataCenter.equipData.PropertyChanged += EquipPropertyChanged;
            EquipManager.Instance.PropertyChanged += EquipManagerChanged;
            DataCenter.equipData.Formations.CollectionChanged += FormationsCollectionChanged;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            DataCenter.equipData.Items.CollectionChanged -= ChangeEquipItem;
            DataCenter.equipData.PropertyChanged -= EquipPropertyChanged;
            EquipManager.Instance.PropertyChanged -= EquipManagerChanged;
            DataCenter.equipData.Formations.CollectionChanged -= FormationsCollectionChanged;
        }

        private void FormationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }

        private void EquipManagerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EquipManager.Instance.ReplaceEquipId))
            {
                RefreshReplace();
            }
        }

        private void RefreshReplace()
        {
            IsReplace = !EquipManager.Instance.IsExistNoneSlots() && EquipManager.Instance.ReplaceEquipId != 0;
        }

        private void EquipPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(DataCenter.equipData.Items))
            {
                Refresh();
            }else if (e.PropertyName is nameof(DataCenter.equipData.CurrWearFormation))
            {
                Refresh();
                PlaySwitchAnimation();
            }
        }

        private void ChangeEquipItem(object sender, NotifyCollectionChangedEventArgs e)
        {
            Refresh();
        }
        
        private void EquipItemDataChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            var equipData = EquipManager.Instance.GetEquipDataByPos(Cfg.Id);
            IsOwn = equipData != null;
            ItemViewModel.MergeInfo(equipData?.Id ?? EquipManager.Instance.BaseEquipId,IsOwn);
        }

        private void PlaySwitchAnimation()
        {
            if (ItemViewModel.IsShow && EquipItemViewObj!=null)
            {
                EquipItemViewObj.transform.localScale = Vector3.one;
                DOVirtual.DelayedCall(playAnimationPos*0.05f, () =>
                {
                    if (EquipItemViewObj != null)
                    {
                        EquipItemViewObj.transform.DOScale(0.95f, 0.1f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
                        {
                            EquipItemViewObj.transform.localScale = Vector3.one;
                        });
                    }

                });
               
            }
        }
    }
}