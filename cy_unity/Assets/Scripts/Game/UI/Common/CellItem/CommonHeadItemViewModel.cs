using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.U2D;

namespace DH.Game.ViewModels
{

    public class CommonHeadData
    {

        /// <summary>
        ///  头像id
        /// </summary>
        public int HeadId;
        /// <summary>
        ///  头像框id
        /// </summary>
        public int HeadFrameId;
        /// <summary>
        /// 整体的缩放
        /// </summary>
        public Vector2 HeadImgSize;

        /// <summary>
        ///  是否解锁
        /// </summary>
        public bool IsUnlock = true;
        /// <summary>
        ///  点击回调
        /// </summary>
        /// <returns></returns>
        public  Action Callback;

        public CommonHeadData(int headId, int frameId, Vector2 size,Action callback)
        {
            InitInfo(headId,frameId,size,callback);
        }
        public CommonHeadData(int headId, int frameId, Vector2 size)
        {
            InitInfo(headId,frameId,size,null);
        }
        public CommonHeadData(int headId, int frameId)
        {
            InitInfo(headId,frameId,Vector2.one*100f,null);
        }
        public CommonHeadData(int headId, int frameId,Action callback)
        {
            InitInfo(headId,frameId,Vector2.one*100f,callback);
        }
        

        private void InitInfo(int headId, int frameId, Vector2 size, Action callback)
        {
            HeadId = headId;
            HeadFrameId = frameId;
            HeadImgSize = size;
            Callback = callback;
        }
    }

    public partial class CommonHeadItemViewModel : ViewModelBase
    {
        [AutoNotify] private string headImgPath;
        [AutoNotify] private string headFrameImgPath;
        [AutoNotify] private SpriteAtlas seasonAtlas;
        [AutoNotify] private bool isShowEffectMask;
        [AutoNotify] private bool newHeadRed;
        private GameObject headNode;
        private Vector2 defaultSize = new Vector2(100, 100);
        private readonly string headPrefabPath = "UI/Common/Head/Head_";
        private float defaultX = 100;
        public GameObject HeadNode
        {
            get => null;
            set
            {
                headNode = value;
                UpdateHeadNodeScale();
            }
        }
        private Transform seasonNode;
        public Transform SeasonNode
        {
            get => null;
            set
            {
                seasonNode = value;
                UpdateSeasonNode();
            }
        }
        
        

        private Transform frameEffectNode;

        public Transform FrameEffectNode
        {
            get => null;
            set
            {
                frameEffectNode = value;
                UpdateHeadFrameEffect();
            }
        }

        private Transform dynamicHeadParent;

        public Transform DynamicHeadParent
        {
            get => null;
            set
            {
                dynamicHeadParent = value;
                GetDynamicParentActive();
            }
        }


        public bool IsMine;//自己头像
        [AutoNotify] private CommonHeadData headInfo;

        [Preserve]
        public CommonHeadItemViewModel(CommonHeadData info, bool isEffectMaskEnable,bool isMine = false)
        {
            IsShowEffectMask = isEffectMaskEnable;
            if (info == null) return;
            UpdatePanel(info);
            IsMine = isMine;
            if (IsMine)
            {
                DataCenter.charcaterData.Digest.PropertyChanged += OnCharcaterDataChanged;
                NewHeadRedEvent();
            }
        }
        
        public static CommonHeadItemViewModel OnCreate(RankMember rankMember)
        {
            if (rankMember == null) return null;
            CommonHeadData tempData = new(rankMember.Logo, rankMember.HeadFrame);
            return new CommonHeadItemViewModel(tempData, true);
        }


        private void UpdateHeadNodeScale()
        {
            if(headNode == null) return;
            if(headInfo == null) return;
            var scaleX = headInfo.HeadImgSize.x / defaultSize.x;
            var scaleY = headInfo.HeadImgSize.y / defaultSize.y;
            headNode.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }

        private void UpdateHeadFrameEffect()
        {
            if(frameEffectNode == null) return;
            if(headInfo == null) return;
            for (int i = 0; i < frameEffectNode.childCount; i++)
            {
                var tempNode = frameEffectNode.GetChild(i).gameObject;
                AssetsManager.ReleaseInstance(tempNode);
            }

            var effectName = UIHelper.GetRewardsIconEffectPath(RewardType.Head, headInfo.HeadFrameId);
            frameEffectNode.gameObject.SetActive(effectName != null);
            if(effectName == null)return;
            UIEffectManager.Instance.AddItemIconEffect(effectName, frameEffectNode).Forget();
            
            var scaleX = headInfo.HeadImgSize.x / defaultSize.x;
            var scaleY = headInfo.HeadImgSize.y / defaultSize.y;
            frameEffectNode.localScale = new Vector3(1.4f, 1.4f, 1);
        }
        private void UpdateSeasonNode()
        {
            if(SeasonAtlas == null) return;
            if(seasonNode == null) return;
            if(headInfo == null) return;
            for (int i = 0; i < seasonNode.childCount; i++)
            {
                var tempNode = seasonNode.GetChild(i).gameObject;
                AssetsManager.ReleaseInstance(tempNode);
            }

            var season = DataCenter.charcaterData.GetHeadSeasonById(headInfo.HeadFrameId);
            if (season > 0)
            {
                UIEffectManager.Instance.AddSeason(season,seasonNode,seasonAtlas,"g_").Forget(); 
            }

        }

        [Command]
        private void OnClickHeadBtn()
        {
            headInfo.Callback?.Invoke();
        }

        public void UpdatePanel(CommonHeadData info)
        {
            if (info == null) return;
            
            Action callback = headInfo?.Callback;
            headInfo = info;
            if (callback != null)
            {
                headInfo.Callback = callback;
            }

            HeadImgPath = DataCenter.charcaterData.GetPlayerHeadImgPath(headInfo.HeadId,headInfo.IsUnlock);
            var headFraneId = info.HeadFrameId;
            var cfg = ConfigCenter.ProPictureCfgColl.GetDataById(headFraneId);
            if (cfg == null)
            {
                HeadFrameImgPath = "common[common_alpha]";
            }
            else
            {
                if (cfg.Type != 2)
                {
                    headFraneId = 200;
                }
                HeadFrameImgPath = UIHelper.GetRewardsIconPath(RewardType.Head,headFraneId);
            }

     
            UpdateHeadNodeScale();
            UpdateHeadFrameEffect();
            UpdateSeasonNode();
            GetDynamicParentActive();
        }

        
        private void OnCharcaterDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName  is nameof(DataCenter.charcaterData.Digest.HeadFrame)
                or nameof(DataCenter.charcaterData.Digest.HeadId)
               )
            {
                CommonHeadData tempDate = new(DataCenter.charcaterData.Digest.HeadId,DataCenter.charcaterData.Digest.HeadFrame, headInfo.Callback);
                UpdatePanel(tempDate);
            }
        }
        protected override void OnDispose()
        {
            DataCenter.charcaterData.Digest.PropertyChanged += OnCharcaterDataChanged;
            base.OnDispose();
        }
        
        private float time;
        public override void Update()
        {
            base.Update();
            if (!IsMine)return;
            if (!UIHelper.CalculateTime(ref time)) return;
            NewHeadRedEvent();
        }

        void NewHeadRedEvent()
        {
            if (IsMine)
            {
                NewHeadRed = DataCenter.charcaterData.Digest.IsNewHead() || DataCenter.charcaterData.Digest.IsNewFrame();
            }
        }
        private void GetDynamicParentActive()
        {
            if (headInfo == null) return;
            if (dynamicHeadParent == null) return;
            
            for (int i = 0; i < dynamicHeadParent.childCount; ++i)
            {
                var child = dynamicHeadParent.GetChild(i);
                AssetsManager.ReleaseInstance(child.gameObject);
            }
            var headCfg = ConfigCenter.ProPictureCfgColl.GetDataById(headInfo.HeadId);
            if (headCfg == null) return;
            if (headCfg.ShowType == (int) EHeadShowType.HeadShowTypeStatic)
            {
                return;
            }
            
            // 这里处理动态显示
            LoadDynamicHead().Forget();
        }

        private async UniTask LoadDynamicHead()
        {
            var path = $"{headPrefabPath}{headInfo.HeadId}";
            var tempX = headInfo.HeadImgSize.x;
            if (tempX != 0)
            {
                tempX = defaultX;
            }

            var scale = tempX /defaultX;
            var tempHead =  await AssetsManager.InstantiateWithParentAsync(path,dynamicHeadParent, false);
            if(dynamicHeadParent ==null || tempHead==null ) return;
            tempHead.transform.localScale = new Vector3(scale, scale, 1);
            SkeletonGraphic tempHeadSkele = tempHead.transform.Find("ActionNode").gameObject.GetComponent<SkeletonGraphic>();
            if(tempHeadSkele == null) return;
            tempHeadSkele.AnimationState.SetAnimation(0, "animation", true);
        }
    }
}