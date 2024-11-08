using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.UIFramework;
using Spine.Unity;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class NewRoleShowViewModel : ViewModelBase
    {
        
		[AutoNotify] private string rolePath;
		[AutoNotify] private string heroNameStr;
        private HeroMainCfg cfg;
        
        #region spine
        [AutoNotify] private bool isShowChapterEffectNode;
        private GameObject effectParentNode;
        private string effectPath = "UI/Role/MapEffect/";
        private SkeletonGraphic curSpine;
        public GameObject EffectParentNode
        {
            get=> null;
            set
            {
                effectParentNode = value;
                if (effectParentNode != null)
                {
                    UpdateChapterMapEffect();
                }
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
            var path = $"{effectPath}{cfg.Model}";
            var effectNode= await AssetsManager.InstantiateWithParentAsync(path, effectParentNode.transform, false);
            curSpine = effectNode.GetComponent<SkeletonGraphic>();
            if (curSpine == null) return;
            IsShowChapterEffectNode = true;
            //curSpine.AnimationState.SetAnimation(0, GameConst.MapAniName.Idle, false);
        }

        #endregion

        
        [Preserve]
        public NewRoleShowViewModel(int roleId)
        {
            cfg = ConfigCenter.HeroMainCfgColl.GetDataById(roleId);
            RolePath = $"role[{cfg.Card}]";
            HeroNameStr = string.Format(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.hero_tips_17).Name,
                ConfigCenter.HeroMainLanguageCfgColl.GetDataById(cfg.Id).Name);
        }
        
    }
}