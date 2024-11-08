using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class MagicDrawViewModel : ViewModelBase
    {
	    [AutoNotify] private ObservableList<LuckProgressAwardCellViewModel> rewardScrollViewList = new();
		[AutoNotify] private string progressStr;
		[AutoNotify] private bool isShowNumEffectParent;
		[AutoNotify] private CommonTopViewModel commonTopItemsVm;
		[AutoNotify] private BottomComponentViewModel bottomComponentVm;
		[AutoNotify] private bool isShowSkipIcon;
		[AutoNotify] private int rewardScrollViewJumpIndex;
		[AutoNotify] private Transform numberEffectNode;
		[AutoNotify] private CommonAdvIconViewModel advIconVm=new();
		[AutoNotify] private string opBtnTextStr;
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeVm;
		[AutoNotify] private string leftTextStr;
		[AutoNotify] private ObservableDictionary<int, MagicDrawRewardCellViewModel> outSideRewardDictionary = new();
		[AutoNotify] private ObservableDictionary<int, MagicDrawInsideRewardCellViewModel> inSideRewardDictionary = new();
		[AutoNotify] private string countPointerStr;
		[AutoNotify] private string timeTextStr;
		[AutoNotify] private bool isCanOpAdBtn;
		[AutoNotify] private bool isCanOpLeftBtn;
		[AutoNotify] private bool isCanOpRightBtn;
		[AutoNotify] private bool isCanOpBtn;
		[AutoNotify] private string adBtnImgPath;
		[AutoNotify] private string opBtnImgPath;
		[AutoNotify] private GameObject turnPointer0;
		[AutoNotify] private GameObject turnPointer1;
		[AutoNotify] private GameObject turnPointer2;
		[AutoNotify] private bool isShowFundRedDot;
		[AutoNotify] private GameObject turnNode;
		
 		
		private Dictionary<int, int> idToAngleDictionary = new();
		private Dictionary<int, int> idToInsideIndexDictionary = new();
		private Dictionary<int, int> idToOutsideIndexDictionary = new();
		private List<int> limitRewardsIds = new();
		
		public int rotateDuration = 3000;
		public float rotateSpeed = 360f; // 指针旋转速度（每秒度数）
		public int numRotations = 4; // 需要转动的圈数
		/// <summary>
		/// 当前指针数量
		/// </summary>
		private int curPointerCount = 1;
		private TitleShowType rewardShowType;
		public Func<System.Object, object> GetOutSideRewardByIndex => GetOutSideRewardVm;
		public Func<System.Object, object> GetInSideRewardByIndex => GetInSideRewardVm;
		
		private float totalProgress = 0;
		private Reward costReward;
        [Preserve]
        public MagicDrawViewModel()
        {
	        List<GameConst.ItemIdCode> list = new(){GameConst.ItemIdCode.Stone};
	        CommonTopItemsVm = new(list);
	        BottomComponentVm = new(OnClickCloseBtn);
	        var cfgs = ConfigCenter.StageRewardCfgColl.DataItems;
	        List<StageRewardCfg> cfgList = new();
	        foreach (var cfg in cfgs)
	        {
		        if(cfg.Type != 2) continue;
		        cfgList.Add(cfg);
	        }
	        var costCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.MagicPray_03);
	        if (costCfg != null && costCfg.Reward != null && costCfg.Reward.Count > 0)
	        {
		        costReward = costCfg.Reward[0];
	        }
	        cfgList.Sort((a, b) => a.Level - b.Level);
	        // 进度奖励
	        for (int i = 0; i < cfgList.Count; ++i)
	        {
		        LuckProgressAwardCellViewModel tempVm= new(cfgList[i], OnClickRecordItem,i == cfgList.Count - 1,EDrawState.DrawMagic);
		        RewardScrollViewList.Add(tempVm);
		        totalProgress = cfgList[i].Level;
	        }
	        
	        InitTurnRewards();
	        UpdateSkipState();
	        UpdatePointerState();
	        UpdateTimeStr();
	        UpdatePanel();
	        UpdateIsShowFundRedDot();
	        DataCenter.magicDrawData.FundClaimed.CollectionChanged += OnFundClaimedChanged;
	        DataCenter.magicDrawData.FundPlusClaimed.CollectionChanged += OnFundClaimedChanged;
        }

        protected override void OnDispose()
        {
	        foreach (var item in OutSideRewardDictionary)
	        {
		        item.Value.Dispose();
	        }
	        foreach (var item in InSideRewardDictionary)
	        {
		        item.Value.Dispose();
	        }
	        advIconVm.Dispose();
	        itemPriceNodeVm.Dispose();
	        DataCenter.magicDrawData.FundClaimed.CollectionChanged -= OnFundClaimedChanged;
	        DataCenter.magicDrawData.FundPlusClaimed.CollectionChanged -= OnFundClaimedChanged;
	        base.OnDispose();
        }

        private void OnFundClaimedChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
	        UpdateIsShowFundRedDot();
        }

        private void InitTurnRewards()
        {
	        foreach (var item in OutSideRewardDictionary)
	        {
		        item.Value.Dispose();
	        }
	        foreach (var item in InSideRewardDictionary)
	        {
		        item.Value.Dispose();
	        }
	        OutSideRewardDictionary.Clear();
	        InSideRewardDictionary.Clear();
	        limitRewardsIds.Clear();
	        idToAngleDictionary.Clear();
	        idToInsideIndexDictionary.Clear();
	        var cfgs = ConfigCenter.PrayJackpotCfgColl.DataItems;
	        List<int> outAngle = new List<int>(){67, 292,157};
	        foreach (var cfg in cfgs)
	        {
		        if (cfg.Type == 1)
		        {
			        var count = OutSideRewardDictionary.Count;
			        OutSideRewardDictionary.Add(count, new MagicDrawRewardCellViewModel(cfg));
			        idToAngleDictionary.Add(cfg.Id, outAngle[count]);
			        limitRewardsIds.Add(cfg.Id);
			        idToOutsideIndexDictionary.Add(cfg.Id, count);
		        }
		        else
		        {
			        var count = InSideRewardDictionary.Count;
			        InSideRewardDictionary.Add(count, new MagicDrawInsideRewardCellViewModel(cfg,count));
			        idToAngleDictionary.Add(cfg.Id, 45*count);
			        idToInsideIndexDictionary.Add(cfg.Id, count);
		        }
	        }
	        
        }

        private float timeCount = 0;
        public override void Update()
        {
	        if(UIHelper.CalculateTime(ref timeCount)) return;
	        UpdateTimeStr();
	        base.Update();
        }

        private void UpdateTimeStr()
        {
	        
	        var endTime = ServerTime.Instance.RemainTime(DataCenter.magicDrawData.EndStamp);
	        if (endTime <= 0)
	        {
		        UIManager.Instance.CloseDialog<MagicDrawView>();
	        }
	        if (endTime >= 86400)
	        {
		        TimeTextStr = UIHelper.ConvertTimeSecondToString(endTime, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
	        }
	        else
	        {
		        TimeTextStr =  ServerTime.Instance.Seconds2Hhmmss(endTime); 
	        }
	        
        }
        

        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<MagicDrawView>();
        }

        [Command]
        private void OnClickInfoBtn()
        {
	        var ruleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.MakeWish_08);
	        List<LimitCellRatioData> limitList = new();
	        List<LimitCellRatioData> normalList = new();
	        var cfgs = ConfigCenter.PrayJackpotCfgColl.DataItems;
	        foreach (var item in cfgs)
	        {
		        var ratioStr = Math.Round(item.Weight / 100f, 2, MidpointRounding.AwayFromZero).ToString("0.##");
		        if (item.Type == 1)
		        {
			        LimitCellRatioData tempData = new(item);
			        tempData.CurRatioStr = ratioStr;
			        limitList.Add(tempData);
		        }
		        else
		        {
			        LimitCellRatioData tempData = new(item.Reward[0], ratioStr, normalList.Count);
			       	normalList.Add(tempData);
		        }
	        }
	        
	        ActivityRuleAndRatioData curData = new(ruleStr, limitList,normalList);
	        ActivityRuleAndRatioViewModel tempVm = new(curData);
	        UIManager.Instance.OpenDialog<ActivityRuleAndRatioView>(tempVm).Forget();
        }

        [Command]
        private void OnClickAdBtn()
        {
	        UIHelper.ShowRewardAds(() =>
	        {
		        curPointerCount = 1;
		        UpdatePointerState();
		        RequestMagicDraw(ELuckDrawOpType.LuckDrawOpAd, 1).Forget();
	        });
        }

        [Command]
        private void OnClickFundBtn()
        {
	        UIManager.Instance.OpenDialog<MagicFundView, MagicFundViewModel>().Forget();
        }

        [Command]
        private void OnClickSkipBtn()
        {
	        DataCenter.magicDrawData.IsSkip = !DataCenter.magicDrawData.IsSkip;
	        UpdateSkipState();
        }
        

        [Command]
        private void OnClickOpBtn()
        {
	        if(CostRes ==null) return;
	        if(!UIHelper.CheckRewardIsEnough(CostRes,true)) return; 
	        RequestMagicDraw(ELuckDrawOpType.LuckDrawOpOneTimes, curPointerCount).Forget();
        }

        [Command]
        private void OnClickLeftBtn()
        {
	        if (curPointerCount > 1)
	        {
		        curPointerCount--;
		        UpdatePointerState();
		        UpdateBtnState();
	        }
        }

        [Command]
        private void OnClickRightBtn()
        {
	        var maxCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.MagicPray_01);
	        if (maxCfg == null || maxCfg.Content ==null || maxCfg.Content.Count == 0)
	        {
		        DHLog.Error("没有配置 请检查 配置  DefinesCfg  ");
		        return;
	        }
	        if (curPointerCount < maxCfg.Content[0])
	        {
		        curPointerCount++;
		        UpdatePointerState();
		        UpdateBtnState();
	        }
        }

        private async void OnClickRecordItem(int id)
        {
	        // var list = DataCenter.magicDrawData.GetCanClaimIdList();
	        var chooseIndex = DataCenter.magicDrawData.GetSelectIndex(id);
	        ReqMagicClaim req = new();
	        req.Id = id;
	        req.OptionalIndex = chooseIndex;
		       
	        var result = await GameNetworkManager.Instance.SendAsync<RspMagicClaim>(req);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        // 直接给奖励
	        UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
	        // 1 更新数据
	        Lodash.DealRewards(result.rsp.Reward.ToList());
	        DataCenter.magicDrawData.UpdateRecordList(id);
	        // 2 刷新状态
	        UpdateRecordListState();
        }
        private async UniTaskVoid RequestMagicDraw(ELuckDrawOpType opType, int count)
        {
	        ResetAllChooseState();
	        ChangeAllOpBtnClick(false);
	        ReqMagicDraw req = new();
	        req.Op = (int)opType;
	        req.Count = count;
	        var result = await GameNetworkManager.Instance.SendAsync<RspMagicDraw>(req);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        ChangeAllOpBtnClick(true);
		        UpdateBtnState();
		        return;
	        }
	        
	        if (opType == ELuckDrawOpType.LuckDrawOpAd)
	        {
		        DataCenter.magicDrawData.AdCount += 1;
	        }

	        // 给表现 给奖励 扣钱
	        // 1 先扣钱
	        Lodash.DealRewards(result.rsp.Cost.ToList(), false);
	        // 更新进度
	        DataCenter.magicDrawData.Progress = result.rsp.Progress;
	        rewardShowType = TitleShowType.Base;
	        var ids = result.rsp.Ids;
	        if (result.rsp.Extra.Count != 0)
	        {
		        rewardShowType = TitleShowType.Residue;
		        DataCenter.magicDrawData.UpdateDrawRecordList(ids.ToList(), true);
	        }
	        else
	        {
		        bool isHave = false;
		        // 检查里面是否有限定
		        foreach (var id in result.rsp.Ids)
		        {
			        if (limitRewardsIds.Contains(id))
			        {
				        isHave = true;
				        break;
			        }
		        }
	        
		        rewardShowType = isHave ? TitleShowType.Limit : TitleShowType.Base;
		        DataCenter.magicDrawData.UpdateDrawRecordList(ids.ToList(), false);
	        }

	      
	    
	        if(DataCenter.magicDrawData.IsSkip)
	        {
		        // 2 更新数据
		        Lodash.DealRewards(result.rsp.Reward.ToList());
		        Lodash.DealRewards(result.rsp.Extra.ToList());
		        var tempList = UIHelper.MergeLists(result.rsp.Reward.ToList(), result.rsp.Extra.ToList());
		        // 直接给奖励动画
		        UpdatePointAngle(ids.ToList(),false);
		        UIHelper.OpenCommonRewardView(tempList, ResetAllChooseState, rewardShowType);
		        await UniTask.Delay(1000);
		        ChangeAllOpBtnClick(true);
		        UpdateBtnState();
		        UpdatePanel();
				     
	        }
	        else
	        {
		        Lodash.DealRewards(result.rsp.Reward.ToList());
		        Lodash.DealRewards(result.rsp.Extra.ToList());
		        var tempList = UIHelper.MergeLists(result.rsp.Reward.ToList(), result.rsp.Extra.ToList());
		        // 播动画
		        PlayDrawAction(tempList, result.rsp.Ids.ToList());
	        }
        }

        private void UpdateBtnState()
        {
	        if (CostRes == null)
	        {
		        OpBtnImgPath = "common[common_button_grey]";
		        IsCanOpBtn = false;
	        }
	        else
	        {
		        if (!UIHelper.CheckRewardIsEnough(CostRes) || !DataCenter.magicDrawData.CheckRewardIsEnough(curPointerCount))
		        {
			        OpBtnImgPath = "common[common_button_grey]";
			        IsCanOpBtn = false;
		        }
		        else
		        {
			        
			        OpBtnImgPath = "common[commom_button_blue]";
			        IsCanOpBtn = true;
		        }   
	        }
	        
	        if (!DataCenter.magicDrawData.IsClickAdBtn())
	        {
		        AdBtnImgPath = "common[common_button_grey3]";
		        IsCanOpAdBtn = false;
	        }
	        else
	        {
		        AdBtnImgPath = "turntable[turntable_btn_1]";
		        IsCanOpAdBtn = true;
	        }
	        
        }

        private void UpdateRewardState()
        {
	        foreach (var item in InSideRewardDictionary)
	        {
		        item.Value.UpdatePanel();
	        }
	        foreach (var item in OutSideRewardDictionary)
	        {
		        item.Value.UpdatePanel();
	        }
        }

        private void UpdateSkipState()
        {
	        IsShowSkipIcon = DataCenter.magicDrawData.IsSkip;
        }
        public void UpdateRecordListState()
        {
	        foreach (var item in RewardScrollViewList)
	        {
		        item.UpdateState();
	        }
	        UpdateRecordJumpIndex();
        }
        private void UpdateRecordJumpIndex()
        {
	        RewardScrollViewJumpIndex = 0;
	        for (int i = 0; i < RewardScrollViewList.Count; i++)
	        {
		        var item = RewardScrollViewList[i];
		        if (item.CurState == ECellItemState.GetIng || item.CurState == ECellItemState.None)
		        {
			        RewardScrollViewJumpIndex = i;
			        break;
		        }
	        }
        }
        private void ChangeAllOpBtnClick(bool isCanOp)
        {
	        IsCanOpAdBtn = isCanOp;
	        IsCanOpLeftBtn = isCanOp;
	        IsCanOpRightBtn = isCanOp;
	        IsCanOpBtn = isCanOp;
        }
        
        private object GetOutSideRewardVm(object index)
        {
	        if (OutSideRewardDictionary.TryGetValue((int)index, out MagicDrawRewardCellViewModel ret))
	        {
		        return ret;
	        }
	        return null;
        }
        private object GetInSideRewardVm(object index)
        {
	        if (InSideRewardDictionary.TryGetValue((int)index, out MagicDrawInsideRewardCellViewModel ret))
	        {
		        return ret;
	        }
	        return null;
        }

        private void UpdatePointerState()
        {
	        if(TurnPointer0 != null) TurnPointer0.SetActive(false);
	        if(TurnPointer1 != null) TurnPointer1.SetActive(false);
	        if(TurnPointer2 != null) TurnPointer2.SetActive(false);
	        if (curPointerCount == 1)
	        {
		        if (TurnPointer0 != null && !TurnPointer0.activeInHierarchy)
		        {
			        TurnPointer0.SetActive(true);
			        var localAngle = TurnPointer0.transform.localEulerAngles;
			        localAngle.z = Lodash.RandRange(0, 360);
			        TurnPointer0.transform.localEulerAngles = localAngle;
		        }
	        }
	        else if (curPointerCount == 2)
	        {
		        if (TurnPointer0 != null )
		        {
			        TurnPointer0.SetActive(true);
			        var localAngle = TurnPointer0.transform.localEulerAngles;
			        localAngle.z = Lodash.RandRange(0, 360);
			        TurnPointer0.transform.localEulerAngles = localAngle;
		        }
		        if (TurnPointer1 != null )
		        {
			        TurnPointer1.SetActive(true);
			        var localAngle = TurnPointer0.transform.localEulerAngles;
			        localAngle.z = Lodash.RandRange(0, 360);
			        TurnPointer1.transform.localEulerAngles = localAngle;
		        }
	        } 
	        else if (curPointerCount == 3)
	        {
		        if (TurnPointer0 != null )
		        {
			        TurnPointer0.SetActive(true);
			        var localAngle = TurnPointer0.transform.localEulerAngles;
			        localAngle.z = Lodash.RandRange(0, 360);
			        TurnPointer0.transform.localEulerAngles = localAngle;
		        }
		        if (TurnPointer1 != null)
		        {
			        TurnPointer1.SetActive(true);
			        var localAngle = TurnPointer0.transform.localEulerAngles;
			        localAngle.z = Lodash.RandRange(0, 360);
			        TurnPointer1.transform.localEulerAngles = localAngle;
		        }
		        if (TurnPointer2 != null)
		        {
			        TurnPointer2.SetActive(true);
			        var localAngle = TurnPointer0.transform.localEulerAngles;
			        localAngle.z = Lodash.RandRange(0, 360);
			        TurnPointer2.transform.localEulerAngles = localAngle;
		        }
	        }
	        CountPointerStr = curPointerCount.ToString();
	        var tipsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_05,curPointerCount);
	        OpBtnTextStr = tipsStr;
	        ItemPriceNodeVm?.Dispose();
	        ItemPriceNodeVm = null;
	        if (CostRes != null)
	        {
		        ItemPriceNodeVm = new(CostRes,true);
	        }
        }

        private Reward CostRes
        {
	        get
	        {
		        if (costReward != null)
		        {
			       return new Reward(costReward.Type, costReward.Id, costReward.Count * curPointerCount);
		        }

		        return null;
	        }
        }

        private void UpdateProgressStr()
        {
	        var tempPro = DataCenter.magicDrawData.Progress;
	        ProgressStr = $"<color=#FFED29><size=50>{tempPro}/</size></color><color=#F5F2CC><size=24>{totalProgress}</size></color>";
        }

        private void UpdatePanel()
        {
	        UpdateProgressStr();
	        UpdateRecordListState();
	        UpdateBtnState();
	        UpdateRewardState();
	        UpdateLeftCount();
        }

        private void UpdateLeftCount()
        {
	        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.MakeWish_05);
	        var count = DataCenter.magicDrawData.GetLeftRewardCount();
	       LeftTextStr = $"{str}{count}";
        }

        private async void PlayDrawAction(List<Resource> tempList, List<int> toList)
        {
	        UpdatePointAngle(toList,true);
	      
	        await UniTask.Delay(rotateDuration + 500);
	        // 直接给奖励动画
	        UIHelper.OpenCommonRewardView(tempList, ResetAllChooseState, rewardShowType);
	        await UniTask.Delay(1000);
	        ChangeAllOpBtnClick(true);
	        UpdateBtnState();
	        UpdatePanel();
	        
        }

        private void UpdateIsShowFundRedDot()
        {
	        var list = DataCenter.magicDrawData.GetCanFundClaimIdList();
	        IsShowFundRedDot = list.Count > 0;
        }

        private void UpdatePointAngle(List<int> idList, bool isPaly)
        {
	        for (int i = 0; i < idList.Count; i++)
	        {
			   var id = idList[i];
		      
		        if (i == 0)
		        {
			        SetPointerAngle(turnPointer0, id, isPaly);
		        } else if (i == 1)
		        {
			        SetPointerAngle(turnPointer1, id, isPaly);
		        } else if (i == 2)
		        {
			        SetPointerAngle(turnPointer2, id, isPaly);
		        }
	        }

	        if (isPaly)
	        {
		        // var angle = Lodash.RandRange(0, 360) - 360;
		        // TurnNode.transform.DOLocalRotate(new Vector3(0, 0, -360), (float)rotateDuration / 1000,
				      //   RotateMode.FastBeyond360)
			       //  .SetEase(Ease.OutQuint);
	        }
        }

        private void SetPointerAngle(GameObject target, int id, bool isPaly)
        {
	        
	        var angleOffset = idToAngleDictionary[id];
	        var cfg = ConfigCenter.PrayJackpotCfgColl.GetDataById(id);
	        if (cfg.Type != 1)
	        {
		        angleOffset += Lodash.RandRange(-10,10);
	        }

	        var curAngle = target.transform.localEulerAngles;
	        curAngle.z = angleOffset;
	        if (isPaly)
	        {
		        RotatePointer(target, curAngle, () =>
		        {
			        TurnEndCallback(id);
		        });
	        }
	        else
	        {
		        target.transform.localEulerAngles = curAngle;
	        }
        }
        void RotatePointer(GameObject pointer, Vector3  targetAngle, Action callabck)
        {
	        var tempAngle = targetAngle;
	        tempAngle.z = targetAngle.z - 360;
	        tempAngle.z += -360f * numRotations; // 计算目标角度
	        pointer.transform.DOLocalRotate(tempAngle, (float)rotateDuration/1000, RotateMode.FastBeyond360)
		        .SetEase(Ease.OutQuint)
		        .OnComplete(() =>
		        {
			        callabck?.Invoke();
		        });
        }

        private async void TurnEndCallback(int id)
        {
	        var cfg = ConfigCenter.PrayJackpotCfgColl.GetDataById(id);
	        if (cfg == null || cfg.Type == 1)
	        { 
		        var tempIndex = idToOutsideIndexDictionary[id];
		        outSideRewardDictionary[tempIndex].PlayEffect();
		        return;
	        }

	        var index = idToInsideIndexDictionary[id];
	        InSideRewardDictionary[index].PlayEffect();
	        InSideRewardDictionary[index].IsShowChooseImg = true;
	        await UniTask.Delay(100);
	        InSideRewardDictionary[index].IsShowChooseImg = false;
	        await UniTask.Delay(100);
	        InSideRewardDictionary[index].IsShowChooseImg = true;
	        await UniTask.Delay(100);
	        InSideRewardDictionary[index].IsShowChooseImg = false;
	        await UniTask.Delay(100);
	        InSideRewardDictionary[index].IsShowChooseImg = true;
	        
        }

        private void ResetAllChooseState()
        {
	        foreach (var item in InSideRewardDictionary)
	        {
		        item.Value.IsShowChooseImg = false;
	        }
        }
        
        
    }
}