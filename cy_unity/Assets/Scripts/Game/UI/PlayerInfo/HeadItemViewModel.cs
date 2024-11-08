using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class HeadItemViewModel : ViewModelBase
    {

        [AutoNotify] private ProPictureCfg cfg;
        [AutoNotify] private readonly int id;
        public CommonHeadItemViewModel CommonHeadVm;
        public SimpleCommand SelectCmd { get; set; }
        [AutoNotify] private bool selected;
        [AutoNotify] private bool isLock;
        [AutoNotify] private bool use;
        [AutoNotify] private bool isNew;
        [AutoNotify] private string nameText;
        private Transform effectTransform;

        public Transform EffectTransform
        {
            get => null;
            set
            {
                effectTransform = value;
                UpdateEffectNode();
            }
        }
        private Transform seasonNode;

        public Transform SeasonNode
        {
            get => null;
            set
            {
                seasonNode = value;
            }
        }
        
        

        [Preserve]
        public HeadItemViewModel(int id,bool islock = true)
        {
            this.id = id; 
            var cfgId = DataCenter.charcaterData.GetHeadCfgId(id);
            Cfg = ConfigCenter.ProPictureCfgColl.GetDataById(cfgId);
            isLock = islock;
            bool isHead = cfg.Type == 1;
            CommonHeadData tempData = new(isHead?cfg.Id:-1, isHead?-1:cfg.Id);
            tempData.IsUnlock = !isLock;
            CommonHeadVm = new CommonHeadItemViewModel(tempData, true);
            UpdateEffectNode();
        }
        private void UpdateEffectNode()
        {
            if(effectTransform == null) return;
            while (effectTransform.childCount>0)
            {
                var tempNode = effectTransform.GetChild(0).gameObject;
                tempNode.transform.SetParent(null);
                AssetsManager.ReleaseInstance(tempNode);
            }
            effectTransform.gameObject.SetActive(Cfg.SpEffect != null);
            if (Cfg.SpEffect == null) return;
            
            UIEffectManager.Instance.AddItemIconEffect(Cfg.SpEffect, effectTransform).Forget();
        }
        
    }
}
