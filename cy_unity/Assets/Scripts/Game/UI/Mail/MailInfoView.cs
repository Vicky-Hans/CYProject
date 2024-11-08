using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using Extend;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DH.Game.UIViews
{
    public partial class MailInfoView : BaseView,IPointerClickHandler
    {
        public DhButton closeBtn;
        public DhButton opBtn;
        public RectTransform contentRectTransform;
        public DhText opBtnText;
        public DhText titleText;
        public DhText contentText;
        [AssetPath] public string awardItemPrefab;
        public UICircularScrollView awardScrollView;
        public ClickToClose clickToClose;
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {

            awardScrollView.PrefabPath = awardItemPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MailInfoView, MailInfoViewModel>();
            bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtn);
            bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
            bindingSet.Bind(opBtn.image).For(v => v.sprite).To(vm => vm.OpBtnImgPath).WithConversion(this);
            bindingSet.Bind(opBtnText).For(v => v.text).To(vm => vm.OpBtnStr);
            //bindingSet.Bind(opBtnText).For(v => v.color).To(vm => vm.OpBtnStrColor);
            bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleStr);
            bindingSet.Bind(contentText).For(v => v.text).To(vm => vm.ContentStr);
            bindingSet.Bind(awardScrollView).For(v => v.Collection).To(vm => vm.AwardList);
            bindingSet.Bind(contentRectTransform).For(v => v.sizeDelta).To(vm => vm.ContentSize);
            bindingSet.Bind(clickToClose).For(v => v.CloseCallback).To(vm => vm.OnClickCloseBtn);
            bindingSet.Build();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(contentText, eventData.position,  AppGlobal.Instance.UICamera);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = contentText.textInfo.linkInfo[linkIndex];
                string linkID = linkInfo.GetLinkID();
                // 处理超链接点击事件
                DHLog.Debug($"Hyperlink clicked! ID: {linkID},  Index:  + {linkIndex}");
                // 触发事件，通知其他脚本超链接被点击
                Application.OpenURL(linkID);
            }
        }
    }
}