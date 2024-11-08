using DG.Tweening;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class RoleItemView : BaseItemView
    {
	    public DhImage roleIconBg;
		public DhImage roleIcon;
		public DhText levelText;
		public DhText shardNums;
		
		public GameObject buttonsGo;
		public GameObject deployGo;
		public GameObject levelGo;

		public DhButton but;
		public DhButton infoBut;
		public DhButton deployBut;
		public GameObject selectIconGo;
		public GameObject starPr;
		public GameObject shardPr;
		private int star;
		public int Star
		{
			get => star;
			set
			{
				star = value;
				var childs = starPr.transform.childCount;
				for (int i = 0; i < childs; i++)
				{
					starPr.transform.GetChild(i).gameObject.SetActive(star-1>=i);
				}

			}
		}

		public Slider shardSlider;

		public GameObject redPoint;
		public DhImage botBg;

		#region 动画相关
		public RectTransform bgRectTf;
		private bool selectAnimatorIng = false;
		public bool SelectAnimatorIng
		{
			get => selectAnimatorIng;
			set
			{
				if (selectAnimatorIng != value)
				{
					selectAnimatorIng = value;
					PlayBgDoTween(SelectAnimatorIng);
				}
			}
		}

		#endregion 
		
		private bool isSelect = false;
		public bool IsSelect
		{
			get => isSelect;
			set
			{
				isSelect = value;
				if (transform !=null && isSelect)
				{
					transform.SetAsLastSibling();
				}
			}
		}
		
		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RoleItemView, RoleItemViewModel>();
            bindingSet.Bind(roleIconBg).For(v => v.sprite).To(vm => vm.RoleIconBgPath).WithConversion(this);
			bindingSet.Bind(roleIcon).For(v => v.sprite).To(vm => vm.RoleIconPath).WithConversion(this);
			bindingSet.Bind(levelText).For(v => v.text).To(vm => vm.LevelTextStr);
			bindingSet.Bind(shardNums).For(v => v.text).To(vm => vm.ShardNumsStr);
			
			bindingSet.Bind(buttonsGo).For(v => v.activeSelf).To(vm => vm.ShowButtonsGo);
			bindingSet.Bind(this).For(v => v.IsSelect).ToExpression(vm => vm.ShowButtonsGo);
			bindingSet.Bind(deployGo).For(v => v.activeSelf).To(vm => vm.ShowDeployGo);
			bindingSet.Bind(levelGo).For(v => v.activeSelf).To(vm => vm.ShowLevelGo);
			
			bindingSet.Bind(but).For(v => v.onClick).To(vm => vm.OnClickButtonCommand);
			//bindingSet.Bind(infoBut).For(v => v.onClick).To(vm => vm.OnClickInfoButtonCommand);
			bindingSet.Bind(deployBut).For(v => v.onClick).To(vm => vm.OnClickDeployButtonCommand);
			bindingSet.Bind(this).For(v => v.Star).ToExpression(vm => vm.Star);
			
			bindingSet.Bind(starPr.transform.parent.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowStar);
			bindingSet.Bind(shardPr).For(v => v.activeSelf).ToExpression(vm => vm.ShowShard);
			bindingSet.Bind(shardSlider).For(v => v.value).To(vm => vm.ShardSliderValue);
			bindingSet.Bind(redPoint).For(v => v.activeSelf).To(vm => vm.RedGo);
			bindingSet.Bind(botBg).For(v => v.sprite).To(vm => vm.BotBgPath).WithConversion(this);
			bindingSet.Bind(this).For(v => v.SelectAnimatorIng).ToExpression(vm => vm.PrSelect);
			bindingSet.Bind(selectIconGo).For(v => v.activeSelf).ToExpression(vm => vm.PrSelect);
			
			bindingSet.Build();
        }
		

        private void PlayBgDoTween(bool select)
        {
	        return;
	        float startValue;
	        float endValue;
	        starPr.transform.parent.parent.gameObject.SetActive(false);
	        buttonsGo.gameObject.SetActive(false);
	        if (select)
	        {
		        startValue = 94;
		        endValue = 267;

	        }
	        else
	        {
		        startValue = 267;
		        endValue = 94;
	        }
	        // 使用 DOTween 创建从 startValue 到 endValue 的变化事件
	        DOTween.To(() => startValue, value => {
		        // 在每次更新时执行的操作
		        Debug.Log("Current value: " + value);
		        bgRectTf.sizeDelta = new Vector2(323, value);
	        }, endValue, 0.1f).OnComplete(() => {
		        // 动画完成时执行的操作
		        Debug.Log("Animation complete!");
		        bgRectTf.sizeDelta = new Vector2(323, endValue);
		        starPr.transform.parent.parent.gameObject.SetActive(!select);
		        buttonsGo.gameObject.SetActive(select);
	        });
        }
		
    }
}