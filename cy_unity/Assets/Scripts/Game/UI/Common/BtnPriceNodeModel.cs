using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class BtnPriceNodeModel : ViewModelBase
    {
        public PackageCfg Cfg;
        public Reward Reward;
        public string PriceStr=string.Empty;
        public bool Deficiency;
        public Color StartColor;
        public Color NoneColor;

        [AutoNotify] private bool isShowIcon;
        [AutoNotify] private bool isShowTmp;
        
        [Preserve]
        public BtnPriceNodeModel(int packId,bool deficiency = false,string noneColor = null)
        {
            Cfg = ConfigCenter.PackageCfgColl.GetDataById(packId);
            if(Cfg==null) return;
            if (Cfg.RechargeId > 0)
            {
                IsShowIcon = false;
                IsShowTmp = false;
                var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(Cfg?.RechargeId ?? 0);
                if (cfgInfo != null) PriceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
                
            }else if (Cfg.RechargeId == -3)
            {
                IsShowIcon = true;
                IsShowTmp = true;
                Reward = Cfg.Cost[0];
                PriceStr = Cfg.Cost[0].Count.ToString();
            }

            Deficiency = deficiency;
            if (noneColor != null)
            {
                if (!ColorUtility.TryParseHtmlString(noneColor, out NoneColor))
                {
                    NoneColor = Color.red;
                }
            }
            else
            {
                NoneColor = Color.red;
            }
        }
        

        [Preserve]
        public BtnPriceNodeModel(string rewardPrice,bool deficiency = false,string noneColor = null)
        {
            PriceStr = rewardPrice;
            Deficiency = deficiency;
            if (noneColor != null)
            {
                if (!ColorUtility.TryParseHtmlString(noneColor, out NoneColor))
                {
                    NoneColor = Color.red;
                }
            }
            else
            {
                NoneColor = Color.red;
            }
        }
        protected override void OnDispose()
        {
            base.OnDispose();
        }

        public Color CostColor()
        {
            if (Deficiency && !Lodash.CheckRewardIsEnough(Reward))
            {
                return NoneColor;
            }

            return StartColor;
        }

        public void Refresh()
        {
            RaisePropertyChanged(nameof(CostColor));
        }
    }
}