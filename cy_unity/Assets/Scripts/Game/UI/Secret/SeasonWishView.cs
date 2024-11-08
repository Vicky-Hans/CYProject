using DH.Asset;
using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class SeasonWishView : BaseView
    {
        public override bool FullScreen => false;
		public DhButton closeBtn;
		public DhText wishText;
		public RawImage infoBg;
		public Texture defaultBg;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<SeasonWishView, SeasonWishViewModel>();
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(wishText).For(v => v.text).To(vm => vm.WishTextStr);
			bindingSet.Bind(this).For(v => v.SeasonId).To(vm => vm.BaseData.Season);
            bindingSet.Build();
        }

        private int seasonId;

        public int SeasonId
        {
	        get=> seasonId;
	        set => UpdateSeasonInfo(value);
        }

        private async void UpdateSeasonInfo(int id)
        {
	        if(id == 0) return;
	        var cfg = ConfigCenter.SecretSeasonCfgColl.GetDataById(id);
	        if(cfg ==null) return;
	        var path = $"Assets/GameAssets/UI/Image/bg/{cfg.Pic}.png";
            
	        var texture = await AssetsManager.LoadAssetAsync<Texture2D>(path);
	        if (texture == null)
	        {
		        DHLog.Warning($"加载banner出错  path {path}" );
		        infoBg.texture = defaultBg;
		        return;
	        }
	        infoBg.texture = texture;
        }
    }
}