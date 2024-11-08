using System.Collections;
using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using Game.UI.MainUi;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public enum EPlayerInfoBtnState
    {
        Free,
        Lock,
        Use
    }
    public partial class AvatarViewModel : ViewModelBase, IComparer
    {
        public ObservableDictionary<int,HeadItemViewModel> HeadModels = new();
        public ObservableDictionary<int,HeadItemViewModel> FrameModels = new();
        public PlayerInfoManager manager => PlayerInfoManager.Instance;
        public DigestData digest => DataCenter.charcaterData.Digest;
        public ObservableList<int> unlockHead => DataCenter.charcaterData.UnlockHead;
        public CharacterData CharacterData => DataCenter.charcaterData;

        [AutoNotify] private CommonHeadItemViewModel headVm;
        [AutoNotify] private int headId = DataCenter.charcaterData.Digest.HeadId;
        [AutoNotify] private int frameId = DataCenter.charcaterData.Digest.HeadFrame;
        private ICollectionView headCollectionView;
        private ICollectionView headFrameCollectionView;
        private HeadItemViewModel currentHead = new(DataCenter.charcaterData.Digest.HeadId);
        private HeadItemViewModel currentFrame = new(DataCenter.charcaterData.Digest.HeadFrame);
        private HeadItemViewModel oldHead;
        private HeadItemViewModel oldHeadFrame;
        private ICollectionView headListView;
        public ICollectionView HeadListView
        {
            get => null;
            set
            {
                headListView = value;
                if (headListView == null) return;
                headListView.Comparer = MainUiManager.HeadCompare.Default;
                headListView.Refresh();
            }
        }
        private ICollectionView headFrameListView;
        public ICollectionView HeadFrameListView
        {
            get => null;
            set
            {
                headFrameListView = value;
                if (headFrameListView == null) return;
                headFrameListView.Comparer = MainUiManager.HeadCompare.Default;
                headFrameListView.Refresh();
            }
        }
        private int curTab;
        private int curId;
        public ICommand OnClickUsetBtn { get; set; }

        [AutoNotify] private ObservableList<PlayerInfoCostItemViewModel> costItemsList = new();

        [Preserve]
        public AvatarViewModel()
        {
            CommonHeadData tempData = new(digest.HeadId, digest.HeadFrame);
            HeadVm = new CommonHeadItemViewModel(tempData, true);
            OnClickUsetBtn = new AsyncCommand(HeadOp);
            
            var unlockHeads = DataCenter.charcaterData.UnlockHead;
            
            var dataItems = ConfigCenter.ProPictureCfgColl.DataItems;

            HeadModels.Clear();
            FrameModels.Clear();
            foreach (var cfg in dataItems)
            {
                if(cfg.SeasonOr != 0) continue;
                AddHeadToList(cfg, cfg.Id);
            }
            foreach (var id in unlockHeads)
            {
                var cfgId = DataCenter.charcaterData.GetHeadCfgId(id);
                var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
                AddHeadToList(cfg, id); ;
            }

            if (headListView != null)
            {
                headListView.Refresh();
            }
            if (headFrameListView != null)
            {
                headFrameListView.Refresh();
            }

            CharacterData.NewHead.CollectionChanged += NewHeadChanged;
        }

        private void AddHeadToList(ProPictureCfg cfg,int id)
        {
            if (cfg.Type == (int)GameConst.EHeadCfgType.Head)
            {
                if (HeadModels.ContainsKey(id))
                {
                    return;
                }
                var itemData = new HeadItemViewModel(id,!unlockHead.Contains(id));
                itemData.NameText = ConfigCenter.ProPictureLanguageCfgColl.GetDataById(id).Description;
                itemData.IsNew = DataCenter.charcaterData.NewHead.Contains(id);
                itemData.SelectCmd = new SimpleCommand(() => { OnSelectHead(itemData); });
                itemData.Use = id== digest.HeadId;
                if (id == HeadId)
                {
                    CurrentHead = itemData;
                    CurId = id;
                }
                HeadModels.Add(id,itemData);
            }
            else
            {
                if (FrameModels.ContainsKey(id))
                {
                    return;
                }
                var itemData = new HeadItemViewModel(id,!unlockHead.Contains(id));
                itemData.NameText = ConfigCenter.ProPictureLanguageCfgColl.GetDataById(id).Description;
                itemData.IsNew = DataCenter.charcaterData.NewHead.Contains(id);
                itemData.SelectCmd = new SimpleCommand(() => { OnSelectHead(itemData); });
                itemData.Use = id == digest.HeadId;
                itemData.SelectCmd = new SimpleCommand(() => { OnSelectFrame(itemData); });
                if (id == FrameId)
                {
                    CurrentHeadFrame = itemData;
                }
                itemData.Use = id == digest.HeadFrame;
                FrameModels.Add(id,itemData);
            }
        }

        protected override void OnDispose()
        {
            CharacterData.NewHead.CollectionChanged -= NewHeadChanged;
            base.OnDispose();
        }

        private void NewHeadChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ShowHeadRedDot));
            RaisePropertyChanged(nameof(ShowHeadFrameRedDot));
        }

        public ICollectionView HeadCollectionView
        {
            get => null;
            set
            {
                headCollectionView = value;
                if (headCollectionView == null) return;
                headCollectionView.Comparer = this;
            }
        }

        public ICollectionView HeadFrameCollectionView
        {
            get => null;
            set
            {
                headFrameCollectionView = value;
                if (headFrameCollectionView == null) return;
                headFrameCollectionView.Comparer = this;
            }
        }

        public int Compare(object x, object y)
        {
            if (x is HeadItemViewModel cell1 && y is HeadItemViewModel cell2)
            {
                if (cell1.IsLock && !cell2.IsLock)
                {
                    return 1;
                }
            }

            return 0;
        }

        public HeadItemViewModel CurrentHead
        {
            get => currentHead;
            set
            {
                oldHead = currentHead;
                if (!Set(ref currentHead, value)) return;
                if (oldHead != null)
                {
                    oldHead.Selected = false;
                }

                if (currentHead != null)
                {
                    currentHead.Selected = true;
                }

                CostItemsList.Clear();
                if (CurTab == 0)
                {
                    if (currentHead.IsLock)
                    {
                        BtnState = EPlayerInfoBtnState.Lock;
                        // var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(CurrentHead.ItemData.cfg.Id);
                        // foreach (var reward in cfg.Reward)
                        // {
                        //     PlayerInfoCostItemViewModel vm = new PlayerInfoCostItemViewModel(reward);
                        //     CostItemsList.Add(vm);
                        // }
                    }
                    else
                    {
                        if (currentHead.Use)
                        {
                            BtnState = EPlayerInfoBtnState.Use;
                        }
                        else
                        {
                            BtnState = EPlayerInfoBtnState.Free;
                        }
                    }
                }
            }
        }

        public HeadItemViewModel CurrentHeadFrame
        {
            get => currentFrame;
            set
            {
                oldHeadFrame = currentFrame;
                if (!Set(ref currentFrame, value)) return;
                if (oldHeadFrame != null)
                {
                    oldHeadFrame.Selected = false;
                }

                if (currentFrame != null)
                {
                    currentFrame.Selected = true;
                }

                CostItemsList.Clear();
                if (CurTab == 1)
                {
                    if (currentFrame.IsLock)
                    {
                        BtnState = EPlayerInfoBtnState.Lock;
                    }
                    else
                    {
                        if (currentFrame.Use)
                        {
                            BtnState = EPlayerInfoBtnState.Use;
                        }
                        else
                        {
                            BtnState = EPlayerInfoBtnState.Free;
                        }
                    }
                }
            }
        }
        // -.-!!
        void RefreshButState()
        {
            if (CurTab == 1)
            {
                if (currentFrame.IsLock)
                {
                    BtnState = EPlayerInfoBtnState.Lock;
                }
                else
                {
                    if (currentFrame.Use)
                    {
                        BtnState = EPlayerInfoBtnState.Use;
                    }
                    else
                    {
                        BtnState = EPlayerInfoBtnState.Free;
                    }
                }
            }
            else
            {
                if (currentHead.IsLock)
                {
                    BtnState = EPlayerInfoBtnState.Lock;
                    // var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(CurrentHead.ItemData.cfg.Id);
                    // foreach (var reward in cfg.Reward)
                    // {
                    //     PlayerInfoCostItemViewModel vm = new PlayerInfoCostItemViewModel(reward);
                    //     CostItemsList.Add(vm);
                    // }
                }
                else
                {
                    if (currentHead.Use)
                    {
                        BtnState = EPlayerInfoBtnState.Use;
                    }
                    else
                    {
                        BtnState = EPlayerInfoBtnState.Free;
                    }
                }
            }
        }

        public async UniTask HeadOp()
        {
            var req = new ReqHeadOp();
            if (this.CurTab == 0)
            {
                // req.Op = CurrentHead.IsLock ? 4: 1;
                req.Op = 1;
                req.Id = CurrentHead.Id;
            }
            else if (CurTab == 1)
            {
                // req.Op = CurrentHeadFrame.IsLock ? 4: 2;
                req.Op = 2;
                req.Id = CurrentHeadFrame.Id;
            }


            var result = await GameNetworkManager.Instance.SendAsync<RspHeadOp>(req);
            if (result.rsp != null && result.rsp.Status == 0)
            {
                // DataCenter.charcaterData.UnlockHead.Add(req.Id);
                if (CurTab == 0)
                {
                    CurrentHead.Use = true;
                    BtnState = EPlayerInfoBtnState.Use;
                    DataCenter.charcaterData.Digest.HeadId = CurrentHead.Id;
                    foreach (var item in HeadModels)
                    {
                        item.Value.Use = false;
                    }

                    CurrentHead.Use = true;
                    if (CurrentHead.IsLock)
                    {
                        CurrentHead.IsLock = false;
                        DataCenter.charcaterData.UnlockHead.Add(req.Id);
                    }

                    // if (req.Op == 4)
                    // {
                    //     var cfgHead = ConfigCenter.ProPictureCfgColl.GetDataById(CurrentHead.ItemData.cfg.Id);
                    //     Lodash.DealRewards(cfgHead.Reward, false);
                    // }
                }
                else
                {
                    CurrentHeadFrame.Use = true;
                    BtnState = EPlayerInfoBtnState.Use;
                    DataCenter.charcaterData.Digest.HeadFrame = CurrentHeadFrame.Id;
                    foreach (var item in FrameModels)
                    {
                        item.Value.Use = false;
                    }

                    CurrentHeadFrame.Use = true;
                    if (CurrentHeadFrame.IsLock)
                    {
                        CurrentHeadFrame.IsLock = false;
                        DataCenter.charcaterData.UnlockHead.Add(req.Id);
                    }

                    // if (req.Op == 4)
                    // {
                    //     var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(CurrentHeadFrame.ItemData.cfg.Id);
                    //     Lodash.DealRewards(cfg.Reward, false);
                    // }
                }
            }
        }

        public async UniTaskVoid OnClickNew(HeadItemViewModel headItemModel)
        {
            if (headItemModel.IsNew)
            {
                var req = new ReqHeadOp();
                req.Op = 3;
                req.Id = headItemModel.Id;
                var result = await GameNetworkManager.Instance.SendAsync<RspHeadOp>(req);
                if (result.rsp != null && result.rsp.Status == 0)
                {
                    int index = DataCenter.charcaterData.NewHead.IndexOf(req.Id);
                    if (index > -1)
                    {
                        DataCenter.charcaterData.NewHead.RemoveAt(index);
                    }

                    if (CurTab == 0)
                    {
                        CurrentHead.IsNew = false;
                    }
                    else
                    {
                        CurrentHeadFrame.IsNew = false;
                    }
                }
            }
        }

        private void OnSelectHead(HeadItemViewModel headItemModel)
        {
            CurrentHead = headItemModel;
            HeadId = headItemModel.Id;
            CurId = HeadId;
            UpdateHeadVmInfo();
            OnClickNew(headItemModel).Forget();
        }

        private void OnSelectFrame(HeadItemViewModel headItemModel)
        {
            CurrentHeadFrame = headItemModel;
            FrameId = headItemModel.Id;
            CurId = FrameId;
            OnClickNew(headItemModel).Forget();
            UpdateHeadVmInfo();
        }

        public int CurTab
        {
            get => curTab;
            set
            {
                Set(ref curTab, value);
                if (curTab == 0)
                {
                    if (CurrentHead == null)
                    {
                        CurId = 0;
                    }
                    else
                    {
                        CurId = CurrentHead.Cfg.Id;
                    }
                }
                else
                {
                    if (CurrentHeadFrame == null)
                    {
                        CurId = 0;
                    }
                    else
                    {
                        CurId = CurrentHeadFrame.Cfg.Id;
                    }
                }
            }
        }

        public int CurId
        {
            get => curId;
            set { Set(ref curId, value); }
        }

        public EPlayerInfoBtnState BtnState
        {
            get => btnState;
            set { Set(ref btnState, value); }
        }

        private EPlayerInfoBtnState btnState;

        public void OnClickHeadTab()
        {
            CurTab = 0;
            if (headListView != null)
            {
                headListView.Refresh();
            }

            RefreshButState();
        }

        public void OnClickFrameTab()
        {
            CurTab = 1;
            if (headFrameListView != null)
            {
                headFrameListView.Refresh();
            }
            RefreshButState();
        }

        public void OnClose()
        {
            UIManager.Instance.CloseDialog<AvatarView>();
        }

        public bool ShowHeadRedDot => DataCenter.charcaterData.Digest.IsNewHead();


        public bool ShowHeadFrameRedDot => DataCenter.charcaterData.Digest.IsNewFrame();


        private void UpdateHeadVmInfo()
        {       
            CommonHeadData tempData = new(HeadId, FrameId);
            HeadVm.UpdatePanel(tempData);
        }
    }
}