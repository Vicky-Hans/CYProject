using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public enum ECellItemState
    {
        None,
        GetIng,
        Finish,
        Select
    }

    public partial class CellItemViewModel : ViewModelBase
    {
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
        [AutoNotify] private ECellItemState state;
        /// <summary>
        /// 是否显示锁 跟 CellItemBaseViewModel 里面的锁不是一个东西，使用的时，需要自己判断
        /// </summary>
        [AutoNotify] private bool isShowLock;

        [AutoNotify] private bool isShowAdvEffectFirst;

        private ParticleSystem effectParticleSystem;

        public ParticleSystem EffectParticleSystem
        {
            get => effectParticleSystem;
            set
            {
                effectParticleSystem = value;
                if (effectParticleSystem != null)
                {
                    PlayEffect();
                }
            }
        }

        public static CellItemViewModel Create(long uid,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
        {
            var heroEquipData = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
            return  heroEquipData==null || heroEquipData.IsNull() ? null : new  CellItemViewModel(heroEquipData.Id,(int)RewardType.HeroEquip,1,sizeType,showLimit,isShowNum,UIHelper.HeroEquipDataToHeroEquip(heroEquipData));
        }
        
        public static CellItemViewModel Create(Resource reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
        {
            return new  CellItemViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,reward.HeroEquip);
        }
        
        public static CellItemViewModel Create(ResourceData reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
        {
            return new  CellItemViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,UIHelper.HeroEquipDataToHeroEquip(reward.HeroEquip));
        }

        public static CellItemViewModel Create(Reward reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
        {
            return new  CellItemViewModel(reward.Id,(int)reward.Type,reward.Count,sizeType,showLimit,isShowNum);
        }
        
        /// <summary>
        /// 基础Item显示
        /// </summary>
        /// <param name="id">唯一id</param>
        /// <param name="type">类型</param>
        /// <param name="count">数量</param>
        /// <param name="sizeType">尺寸</param>
        /// <param name="showLimit">是否显示拥有数量</param>
        /// <param name="isShowNum">是否显示数量</param>
        [Preserve]
        public CellItemViewModel(int id,int type,long count,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,HeroEquip heroEquip=null)
        {
            CellItemBaseViewVm = new CellItemBaseViewModel(id,type,count,sizeType,showLimit,isShowNum,heroEquip);
        }

        public void OpenHighRewardTips()
        {
            PlayAdvEffect();
        }

        public void SetSize(ECellItemSizeType sizeType=ECellItemSizeType.Size166X150)
        {
            CellItemBaseViewVm.SetSize(sizeType);
        }
        
        public bool Merge(Reward reward)
        {
            return CellItemBaseViewVm.Merge(reward);
        }

        public void SetClickAction(Action<Tuple<Vector3, Vector3>> clickAction)
        {
            CellItemBaseViewVm.OnClickEvent = clickAction;
        }
        /// <summary>
        /// 初始化高级奖励播放
        /// </summary>
        private void PlayAdvEffect()
        {
            if (cellItemBaseViewVm.BaseData.Type == (int)RewardType.HeroEquip)
            {
                if (cellItemBaseViewVm.BaseData != null && cellItemBaseViewVm.BaseData.HeroEquip != null)
                {
                    IsShowAdvEffectFirst = ClothesManager.Instance.GetQuaSmallByQuaId(cellItemBaseViewVm.BaseData.HeroEquip.QuaId) > (int)GameConst.QuaType.Blue ;
                }
            }

            PlayEffect().Forget();
        }

        public void StartPlayAdvEffect()
        {
            IsShowAdvEffectFirst = true;
            PlayEffect().Forget();
        }

        private async UniTaskVoid PlayEffect()
        {
            await UniTask.Delay(800);
            if(!IsShowAdvEffectFirst) return;
            if(effectParticleSystem == null) return;
            UIHelper.PlayEffect(effectParticleSystem);
            IsShowAdvEffectFirst = false;
        }
    }
}