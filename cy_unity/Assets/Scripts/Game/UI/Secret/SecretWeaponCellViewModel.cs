using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Extend;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class SecretWeaponCellViewModel : ViewModelBase
    {
        
        [AutoNotify] private string weaponBgImgPath;
        [AutoNotify] private float timeSliderValue;
        [AutoNotify] private int equipModelId;
        private BackpackWeaponData curWeaponData;
        [AutoNotify] private EquipCfg curEquipCfg;
        [AutoNotify] private EquipModelCfg curEquipModelCfg;
        [AutoNotify] private ECellItemSizeType cellSize=ECellItemSizeType.Size120X100;
        [AutoNotify] private bool isShowEffect;
        private DhImage weaponIcon;
        public DhImage WeaponIcon
        {
            get => weaponIcon;
            set
            {
                weaponIcon = value;
                if(weaponIcon ==null) return;
                if (curWeaponData == null) return;
                UpdateWeaponIcon(curWeaponData.WeaponId);
            }
        }
        private DhImage weaponBg;
        public DhImage WeaponBg
        {
            get => weaponBg;
            set
            {
                weaponBg = value;
                if(weaponBg ==null) return;
                if (curWeaponData == null) return;
                UpdateWeaponBg(curWeaponData.WeaponId);
            }
        }
        
        [Preserve]
        public SecretWeaponCellViewModel(BackpackWeaponData weaponData)
        {
            curWeaponData = weaponData;
            curEquipCfg = ConfigCenter.EquipCfgColl.GetDataById(curWeaponData.EquipId);
            curEquipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(curWeaponData.WeaponId);
            WeaponBgImgPath = UIHelper.NoneImagePath();
            InitPanel();
            IsShowEffect = false;
        }

        protected override void OnDispose()
        {
            curWeaponData = null;
            WeaponBgImgPath = UIHelper.NoneImagePath();
            TimeSliderValue = 0;
            EquipModelId = 0;
            base.OnDispose();
        }

        private void InitPanel()
        {
            TimeSliderValue = 0;
            equipModelId = curWeaponData.WeaponId;
            WeaponBgImgPath = UIHelper.GetRewardBgPath(RewardType.Equip, curWeaponData.WeaponId);
            UpdateWeaponIcon(curWeaponData.WeaponId);
        }

        public override void Update()
        {
            base.Update();
            UpdateTimeSlider();
        }

        private void UpdateTimeSlider()
        {
            if(GameDataManager.Instance.WaveEnd || GameTime.Instance.Pause) return;

            TimeSliderValue = BattleManager.Instance.fightingManagerIns.GetWeaponProgress((int)curWeaponData.Uid);
        }

        private void UpdateWeaponIcon(int modelId)
        {
            if(curWeaponData ==null) return;
            if(weaponIcon == null) return;
            var iconPath = UIHelper.GetRewardsIconPath(RewardType.Equip, modelId);
            AssetsManager.LoadAssetAsyncWithCallback(iconPath,
                (Sprite iconSprite) =>
                {
                    if(curWeaponData ==null )return;
                    if (modelId != curWeaponData.WeaponId)
                    {
                        UpdateWeaponIcon(curWeaponData.WeaponId);
                        return;
                    }
                    if(weaponIcon == null) return;
                    weaponIcon.sprite = iconSprite;
                });
        }
        private void UpdateWeaponBg(int modelId)
        {
            if(curWeaponData ==null) return;
            if(weaponIcon == null) return;
            var iconPath = UIHelper.GetRewardBgPath(RewardType.Equip, modelId);
            AssetsManager.LoadAssetAsyncWithCallback(iconPath,
                (Sprite bgSprite) =>
                {
                    if(curWeaponData ==null )return;
                    if (modelId != curWeaponData.WeaponId)
                    {
                        UpdateWeaponBg(curWeaponData.WeaponId);
                        return;
                    }
                    if(weaponBg == null) return;
                    weaponBg.sprite = bgSprite;
                });
        }

        public async UniTaskVoid DelayShowMergeEffect()
        {
          
            if(curWeaponData ==null) return;
            if (GameDataManager.Instance.MergeWeaponList.Contains(curWeaponData.Uid))
            {
                IsShowEffect = true;
                AudioManager.Instance.PlayAudio($"SFX_UI/ui_game_merge0{curWeaponData.WeaponLev}");
                await UniTask.Delay(2000);
                IsShowEffect = false;
            }
        }
    }
}