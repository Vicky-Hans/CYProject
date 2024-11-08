using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DHFramework;
using Game.UI.MainUi;

public class JumpManager : ObservableSingleton<JumpManager>
{

    public void Jump(int jumpId)
    {
        try
        {
            Jump((FunctionJumpCfgId)jumpId);
        }
        catch (Exception e)
        {
            DHLog.Error($"JumpManager jumpId 转为为枚举值异常   jumpId = {jumpId}");
        }
    }

    public void Jump(FunctionJumpCfgId jumpId)
    {
        if(!CheckFunctionUnLock(jumpId)) return;
        switch (jumpId)
        {
            case FunctionJumpCfgId.go_Login : JumpChapter();break;
            case FunctionJumpCfgId.go_EnergyPurchas : JumpBuyPower();break;
            case FunctionJumpCfgId.go_Offline : JumpChapter(isPatrol:true);break;
            case FunctionJumpCfgId.go_MainStage : JumpChapter();break;
            case FunctionJumpCfgId.go_Mainshop : JumpShop();break;
            case FunctionJumpCfgId.go_ChestShop : JumpShop(ShopTitle.Draw);break;
            case FunctionJumpCfgId.go_SpecialShop : JumpShop(ShopTitle.DisCount);break;
            case FunctionJumpCfgId.go_CurrencyShop : JumpShop(ShopTitle.Currency);break;
            case FunctionJumpCfgId.go_ClothingShop : JumpShop(ShopTitle.Draw);break;
            case FunctionJumpCfgId.go_MainEquip : JumpEquip();break;
            case FunctionJumpCfgId.go_MainHero : JumpHero();break;
            case FunctionJumpCfgId.go_HeroEquip : JumpHeroEquip();break;
            case FunctionJumpCfgId.go_DailyStage : JumpDaily();break;
            case FunctionJumpCfgId.go_InfiniteAdventure : JumpAdventure();break;
            case FunctionJumpCfgId.go_ForestAdventure : JumpForestAdventure();break;
            case FunctionJumpCfgId.go_MainBingo : JumpMagicBingo();break;
            default:
            {
                DHLog.Error($"JumpManager FunctionJumpCfgId 暂未处理   FunctionJumpCfgId = {jumpId}");
                break;
            }
        }
    }

    private bool CheckFunctionUnLock(FunctionJumpCfgId jumpId)
    {
        var cfg = ConfigCenter.FunctionJumpCfgColl.GetDataById((int)jumpId);
        if (cfg != null)
        {
            var isUnlock = MainUiManager.Instance.CheckFunctionIsUnlock(cfg.FunctionId);
            if (!isUnlock)
            {
                ToastManager.Show(MainUiManager.Instance.GetFunctionUnLockTips(cfg.FunctionId));
            }
            return isUnlock;
        }
        return false;
    }

    /// <summary>
    /// 跳转关卡
    /// </summary>
    private void JumpChapter(bool isLastChapter=false,bool isPatrol = false )
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            if (isLastChapter)
            {
                MainUiManager.Instance.OnJumpCanFrameChapter();
            }
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeMainStage;
            if (isPatrol)
                UIManager.Instance.OpenDialog<PatrolRewardView, PatrolRewardViewModel>().Forget();
            return default;
        }, true, true).Forget();
    }
    
    /// <summary>
    /// 能量购买
    /// </summary>
    private void JumpBuyPower()
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeMainStage;
            UIManager.Instance.OpenDialog<BuyPowerView>(new BuyPowerViewModel()).Forget();
            return default;
        }, true, true).Forget();
    }
    
    /// <summary>
    /// 默认商店 
    /// </summary>
    /// <param name="title">根据商店类型打开，为空的时候表示默认</param>
    private void JumpShop(ShopTitle title = ShopTitle.None)
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeShop;
            ShopManager.Instance.JumpSelectShopTile = title;
            return default;
        }, true, true).Forget();
    }
    /// <summary>
    /// 跳转装备
    /// </summary>
    private void JumpEquip()
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeEquip;
            return default;
        }, true, true).Forget();
    }
    
    /// <summary>
    /// 英雄界面
    /// </summary>
    private void JumpHero()
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.Role;
            ClothesManager.Instance.CurTabType = ClothesUI.Hero;
            return default;
        }, true, true).Forget();
    }
    /// <summary>
    /// 服饰界面
    /// </summary>
    private void JumpHeroEquip()
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.Role;
            ClothesManager.Instance.CurTabType = ClothesUI.HeroEquip;
            return default;
        }, true, true).Forget();
    }
    /// <summary>
    /// 每日调整
    /// </summary>
    private void JumpDaily()
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeActivity;
            UIManager.Instance.OpenDialog<ChallengeActivityView,ChallengeActivityViewModel>().Forget();
            return default;
        }, true, true).Forget();
    }
    
    /// <summary>
    /// 无尽冒险
    /// </summary>
    private void JumpAdventure()
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeActivity;
            UIManager.Instance.OpenDialog<EndlessActivityView,EndlessActivityViewModel>().Forget();
            return default;
        }, true, true).Forget();
    }
    /// <summary>
    /// 密林探险
    /// </summary>
    private void JumpForestAdventure()
    {
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeMainStage;
            UIManager.Instance.OpenDialog<SecretView,SecretViewModel>().Forget();
            return default;
        }, true, true).Forget();
    }
    /// <summary>
    /// 宾果界面
    /// </summary>
    private void JumpMagicBingo()
    {
        if (DataCenter.mgicBingoData.IsTimeOver())
        {
            ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.niudan17).Name);
            return ;
        }
        
        if (UIManager.Instance.IsOpen<MagicBingoView>())
        {
            if ( ActivityUIManager.Instance.BagicBingo != MagicBingoShowType.Main)
            {
                ActivityUIManager.Instance.BagicBingo = MagicBingoShowType.Main;
            }
            return;
        }
       
        UIManager.Instance.SafeGotoMenu(() =>
        {
            UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeMainStage;
            bool isOpenMagicBingo= MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.MagicBingo);
            if (!isOpenMagicBingo)
            {
                return default;
            }
            if (DataCenter.mgicBingoData.IsTimeOver())
            {
                ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.niudan17).Name);
                return default;
            }
            UIManager.Instance.OpenDialog<MagicBingoView,MagicBingoViewModel>().Forget();
            return default;
        }, true, true).Forget();
    }
    
}
