using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class TalentChooseItemViewModel : ViewModelBase
    {
        [AutoNotify] private string cellBgImgPath;
		[AutoNotify] private TalentCellItemViewModel talentCellItemViewVm;
		[AutoNotify] private string descTextStr;
		[AutoNotify] private Vector3 cellScale;
		[AutoNotify] private bool isCanOp;
		[AutoNotify] private bool isShowTips;
		private Action<int, int> onClickCellBgBtnCallback;
		private int curTalentId;
		[AutoNotify] private int curIndex;
		private Tweener chooseAction;
        [Preserve]
        public TalentChooseItemViewModel(int index,int id, Action<int, int> clickCellBtn)
        {
	        curIndex = index;
	        curTalentId = id;
	        onClickCellBgBtnCallback = clickCellBtn;
	        UpdateTalentCellInfo();
	        TalentCellItemViewVm = new(curTalentId, 0);
	        TalentCellItemViewVm.SetSize(Vector2.one * 180);
	        if (chooseAction != null)
	        {
		        chooseAction.Kill();
		        chooseAction = null;
	        }
	     
	        DoShowAction().Forget
		        ();
	        IsCanOp = true;
        }

        private async UniTaskVoid DoShowAction()
        {
	        CellScale = Vector3.zero;
	        var actionDuration1 = 0.5f;
	        var scale1 = Vector3.one * 0.85f;
	        await UniTask.Delay(curIndex * 200);
	        chooseAction = DOVirtual.Vector3(CellScale, scale1, actionDuration1, v =>
	        {
		        // 设置目标节点的anchoredPosition
		        CellScale = v;
	        }).SetEase(Ease.OutExpo);
        }

        protected override void OnDispose()
        {
	        talentCellItemViewVm.Dispose();
	        base.OnDispose();
        }


        [Command]
        private void OnClickCellBgBtn()
        {
	        onClickCellBgBtnCallback.Invoke(curIndex, curTalentId);
        }

        private void UpdateTalentCellInfo()
        {
	        var cfg = ConfigCenter.TalentCfgColl.GetDataById(curTalentId);
	        if (cfg == null)
	        {
		        DHLog.Error($"没有配置， 请检查  TalentCfg 配置 {curTalentId}");
		        return;
	        }

	        var index = (cfg.Quality < 2 ? 2 : cfg.Quality ) > 5? 5 : cfg.Quality;
	        CellBgImgPath = $"fight[fight_skillbg_{index - 1}]";
	        var languageCfg = ConfigCenter.TalentLanguageCfgColl.GetDataById(curTalentId);
	        if (languageCfg == null)
	        {
                DHLog.Error($"没有配置， 请检查 TalentLanguageCfg 配置 {curTalentId}");
                return;
	        }
			if (cfg.Value is { Count: > 0 })
			{
				DHLog.Warning("打印此时的多语言ID："+languageCfg.Id);
				DescTextStr =  string.Format(languageCfg.Des,cfg.Value.ToArray());
			}
			else
			{
				DescTextStr = languageCfg.Des;
			}
        }

        public void DoChooseAction(int chooseIndex)
        {
	        if (chooseAction != null)
	        {
		        chooseAction.Kill();
		        chooseAction = null;
		        CellScale = Vector3.one;
	        }
	        
	        if (chooseIndex == curIndex)
	        {
		        DoChooseAnimation().Forget();
	        }
	        else
	        {
		        CellScale = Vector3.zero;
	        }
        }
        public void DoAllInAction(int chooseIndex)
        {
	        if (chooseAction != null)
	        {
		        chooseAction.Kill();
		        chooseAction = null;
		        CellScale = Vector3.one;
	        }
	        
	        if (chooseIndex == curIndex)
	        {
		        DoChooseAnimation().Forget();
	        }
        }

        private async UniTaskVoid DoChooseAnimation()
        {
	        var actionDuration1 = 0.15f;
	        var actionDuration2 = 0.25f;
	        var scale1 = Vector3.one * 0.7f;
	        var scale2 = Vector3.one * 1f;
	        chooseAction = DOVirtual.Vector3(CellScale, scale1, actionDuration1, v =>
	        {
		        // 设置目标节点的anchoredPosition
		        CellScale = v;
	        }).SetEase(Ease.InSine);
	        await UniTask.Delay(150);
	        chooseAction = DOVirtual.Vector3(CellScale, scale2, actionDuration2, v =>
	        {
		        // 设置目标节点的anchoredPosition
		        CellScale = v;
	        }).SetEase(Ease.OutQuint);
        }
    }
}