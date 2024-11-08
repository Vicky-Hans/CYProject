using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class AttrItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string bgPath;
		[AutoNotify] private string targetIconPath;
		[AutoNotify] private string targetValueStr;
		public ParticleSystem upEffect;

		public ParticleSystem UpEffect
		{
			get => upEffect;
			set
			{
				upEffect = value;
				if (IsFirstPlay)
				{
					UIHelper.PlayEffect(upEffect);
					IsFirstPlay = false;
				}
			}
		}

		public string TypeName;
		public float Value;

		public bool IsFirstPlay;
		[Preserve]
		public AttrItemViewModel(string type,float value,float addValue=0)
		{
			TypeName = type;
			Value = value;
			TargetIconPath = AttributesManager.Instance.GetAttrIcon(type);
			targetValueStr = AttributesManager.Instance.GetAttrDesc(type,value, addValue,true);
		}

		public void PlayEffect()
		{
			UIHelper.PlayEffect(upEffect);
		}

    }
}