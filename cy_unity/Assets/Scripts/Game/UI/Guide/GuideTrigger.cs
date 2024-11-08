using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using Game.UI.Guide;
using UnityEngine;

namespace DH.Game.UIViews
{
    
    public enum EGuideTriggerParam
    {
        ChapterId,
        Uid,
        EquipId,
        RandomDataCount,
        CanChooseTalent,
        TalentIndex,
        MergeCount,
        WaveState,
        WaveIndex,
        DragType,
        RandomEquip
    }
    public class GuideTrigger: MonoBehaviour
    {
        /// <summary>
        /// 引导Id
        /// </summary>
        public int gudieId;
        // public int delayTime = 500;
        public string reportStr;
        public EGuideTriggerParam[] reportParamsArray;
        public GameObject targetNode;
        public string clickReportStr;
        public EGuideTriggerParam[] clickReportParamsArray;
        private GuideTrigger[] allTrigger;
        public GameObject[] actionNodeList;
        [SerializeField]
        public GuideData[] GuideList;

        public EGuideState CurState =>GuideManager.Instance.GetGuideState(gudieId);

        private bool isWeaponNode;

        protected void Start()
        {
            var tempCom = GetComponent<WeaponItemView>();
            if (tempCom)
            {
                isWeaponNode = true;
            }

            DelayTriggerGuide().Forget();
        }
        private async UniTaskVoid DelayTriggerGuide()
        {
            if(!GuideManager.Instance.CheckIsTrigger(gudieId)) return;
            await UniTask.Delay(200);
            StartTrigger();
        }
        
        public void StartTrigger()
        {
            
            if(CurState != EGuideState.GuideStateReady) return;
            var tempStr = ParseReportStr(reportStr,reportParamsArray);
            if(!GuideManager.Instance.CheckGuideIsTrigger(tempStr)) return;
            GuideManager.Instance.ReportGuideInfo(tempStr,gameObject,targetNode);
        }

        private string ParseReportStr(string str,EGuideTriggerParam[] paramsArray)
        {
            if (paramsArray.Length == 0)
            {
                return str;
            }

            var mainString = str;
            for (int i = 0; i < paramsArray.Length; ++i)
            {
                var tempStr = ParsrGudieParam(paramsArray[i]);
                mainString = mainString.Replace($"%{i}", tempStr);
            }

            return mainString;
        }

        private string ParsrGudieParam(EGuideTriggerParam paramType)
        {
            switch (paramType)
            {
                case EGuideTriggerParam.ChapterId:
                    return GameManager.Instance.CurChapterId.ToString();
                case EGuideTriggerParam.Uid:
                    return GameDataManager.Instance.Uid.ToString();
                case EGuideTriggerParam.EquipId:
                {
                    var tempCom = GetComponent<WeaponItemView>();
                    if (tempCom)
                    {
                        isWeaponNode = true;
                        return tempCom.EquipModelId.ToString();
                    }
                    else
                    {
                        if (GameDataManager.Instance.RandomWeaponDataList.Count > 0)
                        {
                            return GameDataManager.Instance.RandomWeaponDataList[0].ToString();
                        }
                        else
                        {
                            return "";
                        }


                    }
                } break;
                    
                case EGuideTriggerParam.RandomDataCount:
                    return GameDataManager.Instance.RandomWeaponDataList.Count.ToString();
                case EGuideTriggerParam.CanChooseTalent:
                {
                    var isCanRandom = GameDataManager.Instance.CheckIsCanRandomTalent();
                    return isCanRandom ? "true" : "false";
                }
                case EGuideTriggerParam.TalentIndex:
                {
                    var tempComp = GetComponent<TalentChooseItemView>();
                    var tempVm = tempComp.GetDataContext() as TalentChooseItemViewModel;
                    return tempVm?.CurIndex.ToString();
                }
                case EGuideTriggerParam.MergeCount:
                {
                    var count = GameDataManager.Instance.TotalMergeNum;
                    return count.ToString();
                }
                case EGuideTriggerParam.WaveState:
                {
                    return GameDataManager.Instance.WaveEnd? "end" : "run";
                }
                case EGuideTriggerParam.WaveIndex:
                {
                    return GameDataManager.Instance.Wave.ToString();
                }
                case EGuideTriggerParam.DragType:
                {
                    if (GameManager.Instance.LaseDragModelId != 0)
                    {
                        var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(GameManager.Instance.LaseDragModelId);
                        return modelCfg.Type.ToString();
                    }

                    return "0";
                }
                case EGuideTriggerParam.RandomEquip:
                {
                    var tempCom = GetComponent<RandomItemView>();
                    if (tempCom)
                    {
                        return tempCom.CurModelId.ToString(); 
                    }

                    return "";
                } break;
            }

            return "";
        }

        /// <summary>
        ///  获取操作后的·事件字符串
        /// </summary>
        /// <returns></returns>
        public string GetTriggerEventStr()
        {
            return ParseReportStr(clickReportStr, clickReportParamsArray);
        }
    }
}