using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using Spine;
using Spine.Unity;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;


namespace DH.Game.ViewModels
{
    public partial class ChapterCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private string chapterImgPath;
		[AutoNotify] private bool isShowChapterEffectNode;
		[AutoNotify] private string chapterNameStr;
		[AutoNotify] private string recordTextStr;
		private GameObject monsterParentNode;
		private SkeletonAnimation monsterAni;
		private bool isCreateMonsterModel;
		public GameObject MonsterParentNode
		{
			get => null;
			set
			{
				monsterParentNode = value;
				if (monsterParentNode != null)
				{
					CreateMonsterModel();
				}
			}
		}
		private GameObject effectParentNode;
		private string effectPath = "UI/MainStage/MapEffect/";
		private string monsterPath = "Fighting/Enemy/Model/monster_";
		private SkeletonGraphic curSpine;

		public GameObject EffectParentNode
		{
			get=> null;
			set
			{
				effectParentNode = value;
				if (effectParentNode != null)
				{
					UpdateChapterMapEffect().Forget();
				}
			}
		}

		private Action<int> onClickChapterBtnCallback;
		private int chapterId;
		public int ChapterId
		{
			get=> chapterId;
			set
			{
				chapterId = value;
				UpdateChapterInfo();
			}
		}

		[Preserve]
        public ChapterCellViewModel(int id, Action<int> clickChapterBtn)
        {
			chapterId = id;
			onClickChapterBtnCallback = clickChapterBtn;
			UpdateChapterInfo();
        }


        [Command]
        private void OnClickInfoBtn()
        {
	        // 点击关卡详情的预览
	        
        }

        [Command]
        private void OnClickChapterBtn()
        {
	        onClickChapterBtnCallback?.Invoke(ChapterId);
        }

        private void UpdateChapterInfo()
        {
	        var cfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
	        if (cfg == null)
	        {
		        DHLog.Error($"没有 CopyCfg 配置 {chapterId}");
		        return;
	        }
	        var languageCfg = ConfigCenter.CopyLanguageCfgColl.GetDataById(chapterId);
	        if (languageCfg == null)
	        {
		        DHLog.Error($"没有 CopyLanguageCfg 配置 {chapterId}");
		        return;
	        }

	        ChapterImgPath = $"map[{cfg.Preview}]";
	        ChapterNameStr = $"{chapterId}.{languageCfg.Name}";
	        var chapterInfo = DataCenter.mainStageData.GetChapterInfo(chapterId);
	        if (chapterInfo == null)
	        {
		        RecordTextStr = "";
	        }
	        else
	        {
		        if (chapterInfo.Star > 2)
		        {
			        RecordTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips05);
		        }
		        else
		        {
			        RecordTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips04, chapterInfo.Wave);
		        }
	        }
	        IsShowChapterEffectNode = false;
	        
	        if (effectParentNode != null)
	        {
		        UpdateChapterMapEffect().Forget();
	        }
        }

        private async UniTaskVoid UpdateChapterMapEffect()
        {
	        for (int i = 0; i < effectParentNode.transform.childCount; i++)
	        {
		        var child = effectParentNode.transform.GetChild(i);
		        AssetsManager.ReleaseInstance(child.gameObject);
	        }
	        curSpine = null;
	        
	        var cfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
	        if (cfg == null)
	        {
		        DHLog.Error($"没有 CopyCfg 配置 {chapterId}");
		        return;
	        }
	        var path = $"{effectPath}{cfg.Preview}";
	        var effectNode= await AssetsManager.InstantiateWithParentAsync(path, effectParentNode.transform, false);
	        if(effectParentNode ==null ||effectNode ==null) return;
	        curSpine = effectNode.GetComponent<SkeletonGraphic>();
	        if (curSpine == null) return;
	        IsShowChapterEffectNode = true;
	        curSpine.AnimationState.SetAnimation(0, GameConst.MapAniName.Enter, false);
	        curSpine.AnimationState.Complete += OnMapAniComplete;
	        CreateMonsterModel();
        }

        private void OnMapAniComplete(TrackEntry trackentry)
        {
	        curSpine.AnimationState.Complete -= OnMapAniComplete;
	        curSpine.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, true);
        }

        private async void CreateMonsterModel()
        {
	        if(monsterParentNode == null || isCreateMonsterModel) return;
	        
	        for (int i = 0; i < monsterParentNode.transform.childCount; i++)
	        {
		        var child = monsterParentNode.transform.GetChild(i);
		        AssetsManager.ReleaseInstance(child.gameObject);
	        }
	        var cfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
	        if (cfg == null ||cfg.MonPreview == null || cfg.MonPreview.Count == 0)
	        {
		        DHLog.Error($"没有 CopyCfg 配置 {chapterId}");
		        return;
	        }
	        var monsterId = int.Parse(cfg.MonPreview[0]);
	        var monsterCfg = ConfigCenter.MonsterCfgColl.GetDataById(monsterId);
	        if (monsterCfg == null || monsterCfg.ModelId == 0)
	        {
		        DHLog.Error($"没有 MonsterCfg 配置 {monsterId}");
		        return;
	        }
	        monsterParentNode.transform.localScale = new Vector3(100, 0, 100);
	        var pathStr = $"{monsterPath}{monsterCfg.ModelId}";
	        isCreateMonsterModel = true;
	        var monster = await AssetsManager.InstantiateWithParentAsync(pathStr, monsterParentNode.transform, false);
	        isCreateMonsterModel = false;
	        if (monsterParentNode == null)
	        {
		        AssetsManager.ReleaseInstance(monster);
		        return;
	        }

	       
	        var parentCanvas = GetNearestCanvas(monsterParentNode.transform);
	        // 设置节点属于 ui 层级
	        monster.layer = 5;
	        if (monster.TryGetComponent(out monsterAni))
	        {
		        var monsterRender = monsterAni.GetComponent<Renderer>();
		        if (parentCanvas != null)
		        {
			        monsterRender.sortingOrder = parentCanvas.sortingOrder +1;
		        }
		        else
		        {
			        monsterRender.sortingOrder = 801;
		        }
		        monsterAni.AnimationState.SetAnimation(0, "idle", false);
		        monsterAni.AnimationState.TimeScale = 1;
		        monsterAni.AnimationState.Complete += OnMonsterAniComplete;
		        DoMonsterParentAction().Forget();
		        // 设置模型透明度
		        // var material = monsterRender.material;
		        // var tagName = "_Transparency";
		        // material.SetFloat(tagName, 0);
		        // DOVirtual.Float(0, 1, 10, v =>
		        // {
		        //  // 设置目标节点 的alpha
		        //  material.SetFloat(tagName, v);
		        // }).SetEase(Ease.InCirc);
	        }
        }

        private void OnMonsterAniComplete(TrackEntry trackentry)
        {
	        monsterAni.AnimationState.Complete-= OnMonsterAniComplete;
	        monsterAni.AnimationState.TimeScale = 1 / 2.67f;
	        monsterAni.AnimationState.SetAnimation(0, "idle", true);
        }
        
        private Canvas GetNearestCanvas(Transform currentTransform)
        {
            Canvas nearestCanvas = null;
            int nearestSortingOrder = int.MinValue;
        
            while (currentTransform != null)
            {
                Canvas canvas = currentTransform.GetComponent<Canvas>();
                if (canvas != null && canvas.sortingOrder > nearestSortingOrder)
                {
                    nearestCanvas = canvas;
                    nearestSortingOrder = canvas.sortingOrder;
                }
        
                currentTransform = currentTransform.parent;
            }
        
            return nearestCanvas;
        }

        private Sequence curAction;
        private async UniTaskVoid DoMonsterParentAction()
        {
	        if(monsterParentNode ==null) return;
	        if (curAction != null)
	        {
		        curAction.Kill();
		        curAction = null;
	        }
	        var tempScale1 = 0;
	        var tempScale2 = 105;
	        var tempScale3 = 98;
	        var tempScale4 = 100;
	        var time1 = 0.43f;
	        var time2 = 0.5f;
	        var time3 = 0.57f;
	        var time4 = 0.64f;
	        
	        await UniTask.Delay((int)time1 * 1000 + 500);
	        if(monsterParentNode == null) return;
	        curAction = DOTween.Sequence();
	        Tween scaleTween1 = monsterParentNode.transform.DOScaleY(tempScale2, time2 - time1);
	        Tween scaleTween2 = monsterParentNode.transform.DOScaleY(tempScale3, time3 - time2);
	        Tween scaleTween3 = monsterParentNode.transform.DOScaleY(tempScale4, time4 - time3);
	        curAction.Insert(0, scaleTween1);
	        curAction.Insert(time2 - time1, scaleTween2);
	        curAction.Insert(time3 - time2, scaleTween3);
	        curAction.SetLoops(1);
	        curAction.Play().OnComplete(() =>
	        {
		        curAction.Kill();
		        curAction = null;
	        });
        }
    }
}