using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class SubTitleBtnCellView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhButton subTitleBtn;
		public DhImage chooseImg;
		public DhImage unChooseImg;
		public DhText titleText;

		private Color unChooseColor = UIHelper.HexColorStrToColor("#C1A08B");
		private Color chooseColor1 = UIHelper.HexColorStrToColor("#FFD9A3");
		private Color chooseColor2 = UIHelper.HexColorStrToColor("#FCFAE3");
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<SubTitleBtnCellView, SubTitleBtnCellViewModel>();
            
			bindingSet.Bind(subTitleBtn).For(v => v.onClick).To(vm => vm.OnClickSubTitleBtnCommand);
			bindingSet.Bind(chooseImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsChoose);
			bindingSet.Bind(unChooseImg.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsChoose);
			bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleTextStr);
			bindingSet.Bind(titleText).For(v => v.colorGradient).ToExpression(vm => GetTextGradientColor(vm.IsChoose));

            bindingSet.Build();
        }

        private VertexGradient  GetTextGradientColor(bool isChoose)
        {
	        if (!isChoose)
	        {
				return new VertexGradient(unChooseColor, unChooseColor, unChooseColor, unChooseColor);
	        }

	        return new VertexGradient(chooseColor1, chooseColor1, chooseColor2, chooseColor2);
        }
       
    }
}