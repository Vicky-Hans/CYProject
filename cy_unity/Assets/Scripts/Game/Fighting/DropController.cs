using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Asset;
using DH.Config;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class DropController : BaseMonoUnit
    {
        [AssetPath] public string modelPath;
        private Vector3 target;
        private IMonsterAnimator mainAnimator;
        private bool assetReady;
        private GameObject model;
        private FightingBaseManager fightingBaseManager;
        public bool IsNeedRemove { get; set; }
        public Transform TargetTrans { get; set; }
        private float curExp;
        private int curDropId;
        public int DropId=>curDropId;
        private Sequence dropAniSequence;

        private float maxTime = 1000f;
        private float moveSpeed = 0.025f;
        private Collider2D collider2dComponent;
        
        public void Init(int dropId, float expValue)
        {
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            collider2dComponent = GetComponent<Collider2D>();
            collider2dComponent.enabled = true;
            curExp = expValue;
            curDropId = dropId;
            var cfgs = ConfigCenter.MonsterCfgColl.DataItems;
            data = new Monster(cfgs[0]);
            SetupView();
            TargetTrans = fightingBaseManager.playerCtrl.transform;
            IsNeedRemove = false;
            var tempcollider = collider2dComponent as CircleCollider2D;
            if (tempcollider != null)
            {
                var defCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_24);
                if (defCfg != null && defCfg.Content != null && defCfg.Content.Count > 0)
                {
                    tempcollider.radius = defCfg.Content[0] * GameConst.AttributeDivisor;
                }
            }
            gameObject.layer = LayerMask.NameToLayer("Ground");
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            int playerLayer = other.gameObject.layer;
            if (playerLayer == LayerMask.NameToLayer("Player"))
            {
                OnTriggerPlayer();
            }
        }

        public void OnTriggerPlayer()
        {
            collider2dComponent.enabled = false;
            DoMoveAction();
        }

        protected override void OnAssetsLoaded()
        {
            assetReady = true;
            SetupView();
        }

        /// <summary>
        /// 资源可能已经加载完成，防止时序导致MonsterData为空，需要在Init时尝试加载模型
        /// </summary>
        private void SetupView()
        {
           
            if (!assetReady || model || data == null) return;

            if (cts == null || cts.IsCancellationRequested) return;
            
            var modelPoint = transform.Find("Model");
            model = InstantiateObj(modelPath, modelPoint);
        }

        public virtual void OnUpdate(float deltaTime)
        {
            if (GameTime.Instance.Pause && dropAniSequence != null)
            {
                dropAniSequence.Pause();
            } else if (dropAniSequence != null)
            {
                dropAniSequence.Play();
            }
        }
        public void PlayAnimation(string aniName)
        {
            mainAnimator?.PlayAnimation(aniName);
        }
        /// <summary>
        /// 释放对象，方便使用对象池进行管理
        /// </summary>
        public async UniTask ReleaseModel(bool isClear = false)
        {
            if(transform == null)return;
            GetComponent<Collider2D>().enabled = false;
            if (model)
            {
                ReleaseObj(model);
                model = null;
            }
            ReleaseAssets();
        }
        
        private void DoMoveAction()
        {
            var playerPos = BattleManager.Instance.fightingManagerIns.playerCtrl.transform.position;
            playerPos.y += 0.2f;
            var curPos = transform.position;
            var dir = (curPos - playerPos).normalized;
            var localEndPos = curPos + dir * 0.7f;
            var flyTime1 = 200f;
            var distance = Vector3.Distance(playerPos, localEndPos);
            var flyTime2 = distance / moveSpeed;
            flyTime2 = Mathf.Min(flyTime2, maxTime);
            
            Tween moveTween1 = transform.DOLocalMove(localEndPos, flyTime1* GameConst.TimeDivisor).SetEase(Ease.OutCubic);
            Tween moveTween2 = transform.DOLocalMove(playerPos, flyTime2* GameConst.TimeDivisor).SetEase(Ease.InCubic);
            dropAniSequence = DOTween.Sequence();
            dropAniSequence.Insert(0, moveTween1);
            dropAniSequence.Insert(flyTime1* GameConst.TimeDivisor, moveTween2);
            dropAniSequence.SetLoops(1);
            var dropCtrl = this;
            dropAniSequence.Play().OnComplete(() =>
            {
                IsNeedRemove = true;
                dropAniSequence.Kill();
                BattleManager.Instance.fightingManagerIns.dropManager.CheckAndUpdateLastRandomTime();
                BattleManager.Instance.fightingManagerIns.DestroyDrop(dropCtrl);
                BattleManager.Instance.fightingManagerIns.dropManager.DealDropInfo(DropId, curExp);
                dropAniSequence = null;
            });

            if (DropId != 0)
            {
                FightingSoundHelper.Instance.PlaySecretPickUpItem();
            }
        }
        
      
    }
}