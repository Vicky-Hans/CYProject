using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;

namespace DH.Game.UIViews
{
    public partial class DebugViewModel : ViewModelBase
    {
        
        private readonly ObservableList<DebugCellViewModel> debugList = new();

        public ObservableList<DebugCellViewModel> DebugList => debugList;


        private Dictionary<string, Action> list = new Dictionary<string, Action>
        {
            {"GM工具", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                UIManager.Instance.OpenDialog<GmView, GmViewModel>().Forget();
            }},
            {"秘林", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                UIManager.Instance.OpenDialog<SecretSurveysView,SecretSurveysViewModel>().Forget();
            }},
            {"加击杀经验", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                PlayerStats.Instance.KillExp += 300;
            }},
            {"排行榜", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                RankViewModel tempVm= new RankViewModel(ERankType.RankItemMainStage);
                UIManager.Instance.OpenDialog<RankView>(tempVm).Forget();
            }},
            {"邀请好友", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                UIManager.Instance.OpenDialog<InvitedView,InvitedViewModel>().Forget();
            }},
            {"一键胜利", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                PlayerStats.Instance.KillCount = 10000000;
                GameManager.Instance.OnGameEnd(true, true, true);
            }},
            {"一键失败", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                GameManager.Instance.OnGameEnd(false, true,true);
            }},
            {"复活", () =>
            {
                UIManager.Instance.CloseDialog<DebugView>();
                UIManager.Instance.OpenDialog<ReviveView, ReviveViewModel>().Forget();
            }},
            {
                "发武器", () =>
                {
                    var weaponList = new List<BackpackWeaponData>();
                    weaponList.Add(new BackpackWeaponData()
                    {
                        Uid = 1001,
                        EquipId = 1,
                        WeaponId = 101,
                    });
                    weaponList.Add(new BackpackWeaponData()
                    {
                        Uid = 1002,
                        EquipId = 2,
                        WeaponId = 201,
                    });
                    weaponList.Add(new BackpackWeaponData()
                    {
                        Uid = 1003,
                        EquipId = 3,
                        WeaponId = 301,
                    });
                    foreach (var backpackWeaponData in weaponList)
                    {
                        GameDataManager.Instance.AddWeaponToBackpack(backpackWeaponData);
                    }
                    BattleManager.Instance.fightingManagerIns.OnWeaponChanged();
                }
            },
            // {"切换账号", ()=>{UIManager.Instance.OpenDialog<>()}},
            // {"切换账号", ()=>{UIManager.Instance.OpenDialog<>()}},
        };

        public DebugViewModel()
        {
            foreach (var item in list)
            {
                var tempInfo = new DebugCellViewModel(item.Key, item.Value);
                DebugList.Add(tempInfo);
            }
        }
    }
}