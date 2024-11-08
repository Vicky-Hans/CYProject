using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Config;
using DH.Data;
using DH.Foundations.Event;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using Extend;
using Game.UI.Guide;
using UnityEngine;

namespace DH.Game.ViewModels
{

	public enum EGuideDescPos
	{
		GuideDesPosTop = 0,
		GuideDesPosCenter,
		GuideDesPosBottom,

	}

	public partial class GuideViewModel : ViewModelBase
    {
		[AutoNotify] private bool isShowHandImg;
		[AutoNotify] private bool isShowDescNode;
		[AutoNotify] private string descTextStr;
		[AutoNotify] private bool isShowGuide;
		[AutoNotify] private bool isShowMask;
		[AutoNotify] private RectTransform targetRectTransform;
		[AutoNotify] private DhImage handImg;
		[AutoNotify] private LevelGuide levelGuideInfo;
		[AutoNotify] private Vector3 descNodePos;
		private Sequence continuousSequence;
		private DhImage maskImg;
		[AutoNotify] private EventPermeate eventTarget;
		private Vector3[] corners = new Vector3[4];
		float yVelocity; //速度
		private GameObject curGuideNode;
		private GameObject infoNode;
		public int curGuideId;
		private float maskTime = 0.1f;
		private int completeDelayTriggerTime = 400; //ms
		/// <summary>
		/// 一直播动画的节点
		/// </summary>
		private List<int> actionGuideId = new List<int> {102,104};
		/// <summary>
		/// 材质
		/// </summary>
		private Material material; 
		/// <summary>
		/// 直径
		/// </summary>
		private float diameter; 
		/// <summary>
		/// 当前
		/// </summary>
		private float current;

		private GuideTrigger curGuideTrigger;
		public DhImage MaskImg
		{
			get => null;
			set
			{
				if (value == null) return;
				maskImg = value;
				eventTarget = maskImg.gameObject.GetComponent<EventPermeate>();
				eventTarget.DragEndCheckIsShowAction = CheckDragEndPlayAction;
				material = maskImg.gameObject.GetComponent<DhImage>().material;
			} 
		}

		
        [Preserve]
        public GuideViewModel()
        {
	        AddOrRemoveEventListener(true);
        }
        protected override void OnDispose()
        {
	        AddOrRemoveEventListener(false);
	        base.OnDispose();
        }
        
        private void AddOrRemoveEventListener(bool isAdd)
        {
	        if (isAdd)
	        {
		        EventDispatcher.RegEventListener<int, GameObject,GameObject>(GameConst.EventCode.GuideTrigger, OnGuideTrigger);
		        EventDispatcher.RegEventListener<int>(GameConst.EventCode.GuideComplete, OnGuideComplete);
	        }
	        else
	        {
		        EventDispatcher.UnRegEventListener<int, GameObject,GameObject>(GameConst.EventCode.GuideTrigger, OnGuideTrigger);
		        EventDispatcher.UnRegEventListener<int>(GameConst.EventCode.GuideComplete, OnGuideComplete);
	        }
        }
        
        private void OnGuideTrigger(int guideId, GameObject compNode, GameObject guideNode)
        {
	        if (LevelGuideInfo != null)
	        {
		        LevelGuideInfo.OnDispose();
		        LevelGuideInfo = null;
	        }
	        eventTarget.isTouchStart = false;
	        GuideManager.Instance.CurGuideId = guideId;
	        curGuideId = guideId;
	        curGuideNode = guideNode;
	        LevelGuideInfo = new LevelGuide(guideId, compNode,  GetTriggerEvent(compNode));
	        IsShowGuide = true;
	        // 这里播报引导动画
	        DoMaskNode(compNode, guideNode);
	        ParseGuideInfo();
	        ParseGuideAction();
        }

        private void OnGuideComplete(int guideId)
        {
	        // 这里完成引导
	        
	        curGuideNode = null;
	        IsShowGuide = false;
	        
	        // 检查下一步引导
	        CheckNextGuide().Forget();
        }


        private void DoMaskNode(GameObject compNode, GameObject guideNode)
        {
	        // 这里根据引导节点位置，设置遮罩图片的位置
	        // 设置事件透传对象
	        if (eventTarget == null)
	        {
		        DHLog.Error("引导 错误");
		        return;
	        }
	        eventTarget.target = guideNode;
	        eventTarget.GuideId = curGuideId;
	        eventTarget.TriggerEvent = GetTriggerEvent(compNode);
	        if (guideNode == null)
	        {
		        material.SetFloat("_Silder", 0);
		        // 设置 引导类型
		        eventTarget.IsEventInterceptor = false;
		        return;
	        }
	        // 设置 引导类型
	        eventTarget.IsEventInterceptor = true;
	        Canvas canvas = AppGlobal.Instance.UICanvas;
	        var rectTran = guideNode.GetComponent<RectTransform>();
	        rectTran.GetWorldCorners(corners);
	        diameter = Vector2.Distance(WordToCanvasPos(canvas, corners[0]), WordToCanvasPos(canvas, corners[2])) / 2f;
	        
	        float x = corners[0].x + ((corners[3].x - corners[0].x) / 2f);
	        float y = corners[0].y + ((corners[1].y - corners[0].y) / 2f);
	        
	        Vector3 center = new Vector3(x, y, 0f);
	        Vector2 position = Vector2.zero;
	        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, center, canvas.GetComponent<Camera>(), out position);
	        
	        center = new Vector4(position.x, position.y, 0f, 0f);
	       
	        material.SetVector("_Center", center);
	        
	        // 设置材质的float4矩形属性
	        (canvas.transform as RectTransform).GetWorldCorners(corners);
	        for (int i = 0; i < corners.Length; i++)
	        {
				current = Mathf.Max(Vector3.Distance(WordToCanvasPos(canvas, corners[i]), center), current);
	        }
	        material.SetFloat("_Silder", current);
        }

        private void ParseGuideInfo()
        {
	        var cfg = GuideManager.Instance.GetGuideCfg(curGuideId);
	        if(cfg ==null) return;
	        IsShowHandImg = cfg.IsShowTouch != 0;
	        IsShowMask = cfg.IsShowMask == 0;
	        IsShowDescNode = cfg.IsShowDesc != 0;
	        if (IsShowDescNode)
	        {
		        var guideLanguageCfg = ConfigCenter.GuideLanguageCfgColl.GetDataById(curGuideId);
		        DescTextStr = guideLanguageCfg.Desc;
		        // 这里根据配置的文本位置设置位置
		        var posTag =(EGuideDescPos) cfg.DescPosition;
		        switch (posTag)
		        {
			        case EGuideDescPos.GuideDesPosTop: DescNodePos = new Vector3(0, 500f, 0); break;
			        case EGuideDescPos.GuideDesPosCenter: DescNodePos = new Vector3(0, 0f, 0); break;
			        case EGuideDescPos.GuideDesPosBottom: DescNodePos = new Vector3(0, -700f, 0); break;
		        }
	        }
        }

        Vector2 WordToCanvasPos(Canvas canvas, Vector3 world)
        {
	        Vector2 position = Vector2.zero;
	        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, world, canvas.GetComponent<Camera>(), out position);
	        return position;
        }

        public override void Update()
        {
	        base.Update();
	        if(!IsShowMask)
		        material.SetFloat("_Silder", 1000);
	        if (curGuideNode == null) return;
	        if (IsShowMask)
	        {
		        float value = Mathf.SmoothDamp(current, diameter, ref yVelocity, maskTime);
		        if (!Mathf.Approximately(value, current))
		        {
			        current = value;
			        material.SetFloat("_Silder", current);
		        } 
	        }
        }


        private GuideTrigger GetCurGuideTrigger(GameObject infoNode, int guideId)
        {
	        GuideTrigger ret = null;
	        GuideTrigger[] tempComps = infoNode.GetComponents<GuideTrigger>();
	        foreach (var item in tempComps)
	        {
		        if (item.gudieId == guideId)
		        {
			        ret = item;
		        }
	        }
	        
			return ret;       
        }

        private Action GetTriggerEvent(GameObject compNode)
        {
	        curGuideTrigger = GetCurGuideTrigger(compNode, curGuideId);
	        
	        if (curGuideTrigger == null)
	        {
		        DHLog.Error("引导 错误");
		        return null;
	        }
	        return () =>
	        {
		        if(curGuideTrigger.CurState != EGuideState.GuideStateRunning) return;
		        // IsShowGuide = false;
		        var repStr = curGuideTrigger.GetTriggerEventStr();
		        GuideManager.Instance.ReportGuideInfo(repStr, infoNode, null);
	        };
        }
        
        private async UniTaskVoid CheckNextGuide()
        {
	        await UniTask.Delay(completeDelayTriggerTime);
	        GuideManager.Instance.CheckNextGuide();
        }

        private void ParseGuideAction()
        {
	        if(curGuideTrigger == null) return;
	        if(curGuideTrigger.actionNodeList == null || curGuideTrigger.actionNodeList.Length == 0) return;
	        var sPos = curGuideTrigger.actionNodeList[0].transform.position;
	        var ePos = curGuideTrigger.actionNodeList[1].transform.position;
	        if (curGuideId is 102 or 104 or 105 or 112 or 114 )
	        {
		        var node = curGuideTrigger.actionNodeList[1].transform.Find("Viewport/Content/0");
		        if (node != null)
		        {
			        ePos = node.position;
		        }
	        }
	        Vector2 localStartPoint;
	        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, sPos, null, out localStartPoint);
	        Vector2 localEndPoint;
	        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRectTransform, ePos, null, out localEndPoint);

	        if (continuousSequence != null)
	        {
		        continuousSequence.Kill();
		        continuousSequence = null;
	        }
	        IsShowHandImg = true;
	        continuousSequence = DOTween.Sequence();
	        Tween moveAni1 = handImg.gameObject.transform.DOLocalMove(localEndPoint, 0);
	        Tween moveAni2 = handImg.gameObject.transform.DOLocalMove(localStartPoint, 1.5f);
	        continuousSequence.Insert(0,moveAni1);
	        continuousSequence.Insert(0,moveAni2);
	        continuousSequence.SetLoops(-1);
	        continuousSequence.Play();
        }
        
        public void StopHandAction()
        {
	        IsShowHandImg = false;
	        if(continuousSequence != null)
	        {
		        continuousSequence.Kill();
		        continuousSequence = null;
	        }
        }

        private void CheckDragEndPlayAction()
        {
	        // 检查是否完成
	        if (GuideManager.Instance.GetGuideState(curGuideId) != EGuideState.GuideStateComplete && actionGuideId.Contains(curGuideId))
	        {
		        ParseGuideAction();
	        }
        }
        
    }
	  public class LevelGuide
    {
	    private int curGuideId;
	    /// <summary>
	    /// 关卡引导的节点
	    /// </summary>
	    private GameObject gameRootNode;
	    private MainStageGameUiView mainStageUiView;
	    private MainStageGameUiViewModel mainStageUiVm;
	    private GameObject curTriggerCompNode;
	    private Action endCallback;


	    
	    public LevelGuide(int guideId, GameObject triggerCompNode, Action callback)
	    {
		    curTriggerCompNode = triggerCompNode;
		    curGuideId = guideId;
		    endCallback = callback;
		    if (IsLevelGuide)
		    {
			    mainStageUiView = triggerCompNode.GetComponentInParent<MainStageGameUiView>();
			    if (mainStageUiView != null)
			    {
				    gameRootNode = mainStageUiView.gameObject;
				    mainStageUiVm = mainStageUiView.GetDataContext() as MainStageGameUiViewModel;
			    }
		    }
		    GameDataManager.Instance.RandomWeaponDataList.CollectionChanged += OnCollectionChanged;
		    GameDataManager.Instance.PropertyChanged += OnGameDataChanged;
		    GameManager.Instance.PropertyChanged += OnGameChanged;
		    // GameManager.Instance.PropertyChanged += OnGameManagerChanged;
		    PlayerStats.Instance.PropertyChanged += OnPlayerStatsChanged;
		    
	    }

	    public void OnDispose()
	    {
		    GameDataManager.Instance.RandomWeaponDataList.CollectionChanged -= OnCollectionChanged;
		    GameDataManager.Instance.PropertyChanged -= OnGameDataChanged;
		    PlayerStats.Instance.PropertyChanged -= OnPlayerStatsChanged;
		    GameManager.Instance.PropertyChanged -= OnGameChanged;
	    }

	    private void OnGameChanged(object sender, PropertyChangedEventArgs e)
	    {
		    if (e.PropertyName == nameof(GameManager.Instance.DragWeaponId))
		    {
			    if (GameManager.Instance.DragWeaponId == 0)
			    {
				    endCallback?.Invoke();
			    }
			    else
			    {
				    CheckIsPass();
			    }
		    }
	    }

	    private void CheckIsPass()
	    {
		    if (curGuideId == 104)
		    {
			    // var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(GameManager.Instance.DragWeaponId);
			    // GuideManager.Instance.UpdateEventIsPass(equipModelCfg.Equip == 2);
			    // 20240902 142830 拖动任意装备
			    GuideManager.Instance.UpdateEventIsPass(true);
		    } 
		    else if (curGuideId == 112)
		    {
			    var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(GameManager.Instance.DragWeaponId);
			    GuideManager.Instance.UpdateEventIsPass(equipModelCfg.Type == 2);
		    }
	    }

	    private void OnPlayerStatsChanged(object sender, PropertyChangedEventArgs e)
	    {
		    if (GameDataManager.Instance.CheckIsCanRandomTalent())
		    {
			    if (curGuideId == 107)
			    {
				    GuideManager.Instance.CheckNextGuide();
			    }
		    }
	    }
	    

	    private void OnGameDataChanged(object sender, PropertyChangedEventArgs e)
	    {
		    if (e.PropertyName == nameof(GameDataManager.Instance.TotalMergeNum))
		    {
			    if (curGuideId == 104)
			    {
				    endCallback?.Invoke();
			    }
		    } else if (e.PropertyName == nameof(GameDataManager.Instance.WaveEnd))
		    {
			    if (GameDataManager.Instance.WaveEnd && curGuideId == 109)
			    {
				    GuideManager.Instance.CheckNextGuide();
			    }
		    }
	    }

	    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	    {
		    if (GameDataManager.Instance.RandomWeaponDataList.Count == 0)
		    {
			    if (curGuideId == 102 || curGuideId == 105 || curGuideId == 114)
			    {
				    // 拖动完成 检查按钮
				    endCallback?.Invoke();
			    }
		    }
		    else if(GameDataManager.Instance.RandomWeaponDataList.Count == 2)
		    {
			    if ( curGuideId == 104 || curGuideId == 112)
			    {
				    // 拖动完成 检查按钮
				    endCallback?.Invoke();
			    }
		    }
	    }

	    /// <summary>
	    /// 判断是否是关卡引导
	    /// </summary>
	    public bool IsLevelGuide
	    {
		    get
		    {
			    var cfg = GuideManager.Instance.GetGuideCfg(curGuideId);
			    if (cfg == null) return false;
			    return cfg.Type == (int)EGuideType.GuideTypeLevel;
		    }
	    }
    }
    
}