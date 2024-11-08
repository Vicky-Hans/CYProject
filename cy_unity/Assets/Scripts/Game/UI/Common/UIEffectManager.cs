using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Asset;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace DH.Game
{
    public enum EActionType
    {
        /// <summary>
        /// 散开依次飞上去
        /// </summary>
        ActionTypeSpreadContinue,
        /// <summary>
        /// 炸开飞上去
        /// </summary>
        ActionTypeExplosionGather,
    }
    public partial class UIEffectManager : ObservableSingleton<UIEffectManager>
    {
        [AutoNotify] private Dictionary<int,CommonTopViewModel> commonTopVmDataList = new();
        private readonly string actionItemPrefabPath = "UI/Common/Items/ActionItem";
        public Func<int> GetCommonTopVmNewId => getKeyFunc;
        
        private Func<int> getKeyFunc;


        public void Init()
        {
            CommonTopVmDataList.Clear();
            getKeyFunc = CreateIncrementFunction();
        }
        public Func<int> CreateIncrementFunction()
        {
            int count = 0;

            Func<int> incrementFunction = () =>
            {
                count++;
                return count;
            };

            return incrementFunction;
        }

        private CommonTopViewModel GetLastTopVm()
        {
            if (commonTopVmDataList.Count > 0)
            {
                return commonTopVmDataList.Last().Value;
            }
            return null;
        }

        private Vector3 GetScreenCenterPos()
        {
            // 将屏幕坐标转换为屏幕中心坐标
            Vector3 centerScreenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
            // 将屏幕中心坐标转换为世界坐标
            Vector3 startWPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(centerScreenPos);
            return startWPos;
        }

        private async UniTask<GameObject> GetActionItem(Transform parent, int itemId)
        {
            GameObject actionItem = await AssetsManager.InstantiateWithParentAsync(actionItemPrefabPath, parent, false);
            if (actionItem == null) return null;
            if (parent == null)
            {
                AssetsManager.ReleaseInstance(actionItem);
                return null;
            }
            Image itemImg = actionItem.transform.Find("Icon").gameObject.GetComponent<Image>();
            var iconPath = DataCenter.itemsData.GetItemIconPathById(itemId);
            itemImg.sprite = AssetsManager.LoadSpriteSync(iconPath);
            itemImg.gameObject.SetActive(true);
            
            return actionItem;
        }

        /// <summary>
        /// 播放物品获得动画 （仅展示顶部有的）
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="parentNode"></param>
        /// <param name="itemCount"></param>
        /// <param name="callabck"></param>
        public async UniTaskVoid PlayItemClaimedActionInShowTop(int itemId, Transform  parentNode, int itemCount = 10, Action callabck = null)
        {
            var tempVm = GetLastTopVm();
            if(tempVm == null) return;
            if (!tempVm.IsInShowItem(itemId)) return;
            var endItem = tempVm.GetResItem(itemId);
            if (endItem == null || endItem.BgRect == null) return;
            // 结束位置
            var endWPos = endItem.BgRect.position;
            // 开始位置
            var startWPos = GetScreenCenterPos();
            // 将世界坐标转换为节点坐标
            Vector3 localStartPos = parentNode.InverseTransformPoint(startWPos);
            Vector3 localEndPos = parentNode.InverseTransformPoint(endWPos);
            
            for (int i = 0; i < itemCount; i++)
            {
                var actionItem = await GetActionItem(parentNode ,itemId);
                if(actionItem == null) return;
                actionItem.transform.localPosition = localStartPos;
                PlayBoomTopShowItemAction(actionItem, localEndPos,null);
            }

            await UniTask.Delay(1000);
            callabck?.Invoke();
        }


        /// <summary>
        /// 游戏中怪物死亡变成金币 / 棋盘上钱包的钱 ，金币飞到顶部 GameCoin 位置的动画
        /// </summary>
        /// <param name="startWorldPos"> 场景的世界坐标</param>
        /// <param name="count"></param> 数量
        /// <param name="isRandomPos"></param> 是否随机起始位置
        /// <param name="endCallback"> 回调</param>
        /// <param name="isScenePos"> 是否是场景的世界坐标</param>
        public async UniTaskVoid PlayerGameCoinAction(Vector3 startWorldPos,int count=1,bool isRandomPos = false, Action endCallback=null, bool isScenePos = true)
        {
            var tempVm = GetLastTopVm();
            if(count == 0 || tempVm ==null) return;
            var itemId = (int)GameConst.ItemIdCode.GameCoin;
            if (!tempVm.IsInShowItem(itemId)) return;
            var endItem = tempVm.GetResItem(itemId);
            if (endItem == null || endItem.BgRect == null) return;
            Vector3 localStartPos;
            if (isScenePos)
            {
                localStartPos = UIHelper.GetScenePosToUINodeLocalPosition(startWorldPos, endItem.BgRect, BattleManager.Instance.fightingManagerIns.camCtrl.MainCamera);     
            }
            else
            {
                localStartPos =  endItem.BgRect.InverseTransformPoint(startWorldPos);
            }
            
            // 开始位置
            Vector3 localEndPos = Vector3.zero;
            Vector3 flyEndScale = Vector3.one * 0.6f;
            var flyTime = 500;// 500ms
            // 单个飞出间隔
            var timeOffset = 10; //20ms
            for (int i = 0; i < count; i++)
            {
                var actionItem = await GetActionItem(endItem.BgRect ,itemId);
                if(actionItem == null) return;
                if (isRandomPos)
                {
                    // 随机半径 半径
                    var randoLen =Lodash.RandRange(0, 50);
                    var randomAngle = Lodash.RandRange(0, 360);
                    var dir = Lodash.Angle2Direction(randomAngle);
                    // 随机的位置
                    var randomStartPos = localStartPos + dir * randoLen; 
                    actionItem.transform.localPosition = randomStartPos;
                }
                else
                {
                    actionItem.transform.localPosition = localStartPos;
                }

                await UniTask.Delay(timeOffset * i);
                actionItem.transform.localScale = Vector3.one * 2.0f;
                await UniTask.Delay(10);
                Tween moveTween = actionItem.transform.DOLocalMove(localEndPos, flyTime* GameConst.TimeDivisor).SetEase(Ease.InCubic);
                Tween scaleTween = actionItem.transform.DOScale(flyEndScale, flyTime * GameConst.TimeDivisor).SetEase(Ease.InCubic);
                Sequence parallelSequence = DOTween.Sequence();
                parallelSequence.Insert(0, moveTween);
                parallelSequence.Insert(0, scaleTween);
                parallelSequence.SetLoops(1);
                parallelSequence.Play().OnComplete(() =>
                {
                    AssetsManager.ReleaseInstance(actionItem);
                });
            }
            AudioManager.Instance.Play(AudioType.FlyGameCoin);
            await UniTask.Delay(flyTime);
            
            endCallback?.Invoke();
        }


        private async UniTaskVoid PlayItemAction(GameObject item, Vector3 dir, float speed, Vector3 endPos, Action endCallback, int index,EActionType type=EActionType.ActionTypeExplosionGather)
        {
            var startPos = item.transform.localPosition;
            var time1 = 0.33f;
            var scaleTiem1 = 0.03f;
            var pos1 = startPos +  dir * speed;
            if (type == EActionType.ActionTypeExplosionGather)
            {
                var targetPos = dir * speed;
                pos1 = startPos + new Vector3(Lodash.RandRangeFloat(-targetPos.x, targetPos.x), Lodash.RandRangeFloat(-targetPos.y, targetPos.y), 0);
            }

            var scale1 = Vector3.one * 2.0f;
            if (type == EActionType.ActionTypeSpreadContinue)
            {
                await UniTask.Delay(100 * index);
            }
            else
            {
                scaleTiem1 = 0;
            }

            if(item == null) return;
            //炸开
            Sequence parallelSequence1 = DOTween.Sequence();
            Tween moveTween1 = item.transform.DOLocalMove(pos1, time1).SetEase(Ease.OutCubic);
            Tween scaleTween1 = item.transform.DOScale(scale1, scaleTiem1).SetEase(Ease.OutCubic);
            Tween scaleTween101 = item.transform.DOScale(scale1, time1 - scaleTiem1).SetEase(Ease.Linear);
            parallelSequence1.Insert(0f,moveTween1);
            parallelSequence1.Insert(0,scaleTween1);
            parallelSequence1.Insert(scaleTiem1,scaleTween101);
            parallelSequence1.SetLoops(1);
            parallelSequence1.Play().OnComplete(() =>
            {
                DHLog.Debug("muzili log start move");
                
            });
            // 停留340毫秒
            var delayTime = 340;
            await UniTask.Delay(delayTime);
            
            if(item == null) return;
            var time2 = 0.8f;
            var scaleTime = 0.5f;

            var scale2 = Vector3.one * 0.8f;
            Sequence parallelSequence2 = DOTween.Sequence();
            Tween moveTween2 = item.transform.DOLocalMove(endPos, time2).SetEase(Ease.InCubic);
            Tween scaleTween2 = item.transform.DOScale(scale2, scaleTime).SetEase(Ease.InCubic);
            parallelSequence2.Insert(0f,moveTween2);
            parallelSequence2.Insert(time2-scaleTime,scaleTween2);
            parallelSequence2.SetLoops(1);
            parallelSequence2.Play().OnComplete(async () =>
            {
                if (item == null)
                {
                    AssetsManager.ReleaseInstance(item);
                    return;
                }
                endCallback?.Invoke();
                Image itemImg = item.transform.Find("Icon").gameObject.GetComponent<Image>();
                itemImg.gameObject.SetActive(false);
                ParticleSystem effectSystem = item.transform.Find("EffectNode").gameObject.GetComponent<ParticleSystem>();
               
                if (effectSystem != null)
                {
                    item.transform.localScale = new Vector3(0.6f,0.6f,0.6f);
                    effectSystem.gameObject.SetActive(true);
                    effectSystem.Play();
                    await UniTask.Delay(500);
                    AssetsManager.ReleaseInstance(item);
                }
                else
                {
                    AssetsManager.ReleaseInstance(item);
                }
            });
        }

        /// <summary>
        /// 炸开飞上去
        /// </summary>
        /// <param name="actionNode"></param>
        /// <param name="endPos"></param>
        /// <param name="callabck"></param>
        private void PlayBoomTopShowItemAction(GameObject actionNode, Vector3 endPos, Action callabck)
        {
            var startPos = actionNode.transform.localPosition;
            if(actionNode== null) return;
            float moveSpeed = 8000f;
            // 爆炸范围 半径
            var boomLen =Lodash.RandRange(0, 300);
            var boomTime = 0.2f;
            var randomAngle = Lodash.RandRange(0, 360);
            var dir = Lodash.Angle2Direction(randomAngle);
            var boomEndPos = startPos + dir * boomLen;
            boomEndPos.z = 0;
            var boomScale = Vector3.one * 3.0f;
            var dis = Vector3.Distance(startPos, boomEndPos);
            var flyTime = dis / moveSpeed;
            actionNode.transform.localScale = boomScale;
            Sequence parallelSequence1 = DOTween.Sequence();
            Tween moveTween1 = actionNode.transform.DOLocalMove(boomEndPos, boomTime).SetEase(Ease.OutCirc);
            Tween scaleTween1 = actionNode.transform.DOScale(boomScale, boomTime).SetEase(Ease.InCubic);
            var flyEndScale = Vector3.one * 0.6f;
            Tween moveTween2 = actionNode.transform.DOLocalMove(endPos, flyTime).SetEase(Ease.InCubic);
            Tween scaleTween2 = actionNode.transform.DOScale(flyEndScale, flyTime).SetEase(Ease.InCubic);
            parallelSequence1.Insert(0, moveTween1);
            parallelSequence1.Insert(0, scaleTween1);
            parallelSequence1.Insert(boomTime, moveTween2);
            parallelSequence1.Insert(boomTime, scaleTween2);
            parallelSequence1.SetLoops(1);
            parallelSequence1.Play().OnComplete(() =>
            {
                callabck?.Invoke();
                AssetsManager.ReleaseInstance(actionNode);
            });
        }


        public async UniTask<GameObject> AddItemIconEffect(string effectName, Transform parentTransform)
        {
            return await AddItemIconEffect(effectName, parentTransform, Vector3.zero);
        }
        public async UniTask<GameObject> AddItemIconEffect(string effectName, Transform parentTransform, Vector3 localPos)
        {
            if (effectName == null) return null;
            var path = $"UIEffects/{effectName}";
            var tempNode = await AssetsManager.InstantiateWithParentAsync(path, parentTransform, false);
            if (tempNode == null) return null;
            if (parentTransform.gameObject == null && tempNode == null)
            {
                AssetsManager.ReleaseInstance(tempNode);
                return null;
            }
            tempNode.transform.localPosition = localPos;
            return tempNode;
        }

        public async UniTask AddSeason(int season, Transform parentNode,SpriteAtlas seasonAtlas, string namePre)
        {
              var path = $"UI/EndlessGame/SeasonNode";
              var seasonStr = $"s{season}";
              for (int i = 0; i < seasonStr.Length; i++)
              {
                  var tempStr = seasonStr[i];
                  var tempNode = await AssetsManager.InstantiateWithParentAsync(path,parentNode, false);
                  if(tempNode == null) return;
                  if (parentNode == null)
                  {
                      AssetsManager.ReleaseInstance(tempNode);
                      return;
                  }

                  if (tempNode.TryGetComponent(out Image seasonImg))
                  {
                      seasonImg.sprite = seasonAtlas.GetSprite($"{namePre}{tempStr}");
                  }
                  else
                  {
                      DHLog.Error($"加载  AddSeason 出错  path {path}" );
                  }
              }
        }

        public async UniTask PlayLuckDrawFlyEffect(List<int> list, Transform parentNode, Action callback)
        {
            foreach (var id in list)
            {
                GameObject actionItem = await AssetsManager.InstantiateWithParentAsync(actionItemPrefabPath, parentNode, false);
                if (actionItem == null) continue;
                if(parentNode == null)
                {
                    AssetsManager.ReleaseInstance(actionItem);
                    return;
                }
                AudioManager.Instance.Play(AudioType.LuckyFlyNum);
                var startScale = 0.5f;
                var offsetY = 40f;
                actionItem.transform.localScale = Vector3.one * startScale;
                await UniTask.Delay(20);
                Image itemImg = actionItem.transform.Find("Icon").gameObject.GetComponent<Image>();
                var iconPath = $"turntable[turntable_num_{id}]";
                itemImg.sprite = AssetsManager.LoadSpriteSync(iconPath);
                itemImg.SetNativeSize();
                itemImg.gameObject.SetActive(true);
                Vector3 startPos = Vector3.zero;
                startPos.x = Lodash.RandRange(-60, 60);
                var endPos = startPos;
                endPos.y += offsetY;
                actionItem.transform.localPosition = startPos;
                Sequence parallelSequence1 = DOTween.Sequence();
                Tween moveTween1 = actionItem.transform.DOLocalMove(endPos, 0.5f).SetEase(Ease.OutCirc);
                parallelSequence1.Insert(0, moveTween1);
                parallelSequence1.SetLoops(1);
                parallelSequence1.Play().OnComplete(() =>
                {
                    AssetsManager.ReleaseInstance(actionItem);
                });
                endPos.y += 60;
                DOVirtual.Float(itemImg.color.a, 180, 0.5f, v =>
                {
                    // 设置目标节点的anchoredPosition
                    var curColor = itemImg.color;
                    curColor.a = v;
                    itemImg.color = curColor;
                });
                DOVirtual.Float(startScale, 1.2f, 0.5f, v =>
                {
                    // 设置目标节点的anchoredPosition
                    actionItem.transform.localScale = Vector3.one * v;
                });
            }
            await UniTask.Delay(500);
            callback?.Invoke();
        }
    }
}