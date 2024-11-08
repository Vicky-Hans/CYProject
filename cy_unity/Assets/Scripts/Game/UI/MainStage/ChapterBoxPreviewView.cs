using System;
using DH.Game;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.MainStage
{
    public partial class ChapterBoxPreviewView: BaseView
    {
        // public TextMeshProUGUI titleStr;
        public CanvasGroup canvasGroup;
        public Button closeBtn;
        public ScrollRectExtend list;
        public Transform content;
        public Transform tipsNode;
        [AssetPath] public string prefabPath;
        public RectTransform bgRect;
        public GameObject bottomTipNode;
        [NonSerialized] public Vector3 position;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            list.PrefabPath = prefabPath;
            var bindSet = this.CreateBindingSet<ChapterBoxPreviewView, ChapterBoxPreviewViewModel>();
            bindSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
            bindSet.Bind(list).For(v => v.Collection).To(vm => vm.Items);
            // bindSet.Bind(tipsNode).For(v => v.localPosition).ToExpression(vm => SetPosition(vm.NodePos, vm.Offset));
            bindSet.Bind(bgRect).For(v => v.sizeDelta).To(vm => vm.BgSize);
            bindSet.Bind(this).For(v => v.position).ToExpression(vm => CalcPos(vm.NodePos,vm.Offset, vm.BgSize));
            // bindSet.Bind(titleStr).For(v => v.text).To(vm => vm.TitleStr);
            bindSet.Build();

            if (list.AssetReady)
            {
                canvasGroup.alpha = 1;
            }
            list.OnAssetLoaded += ListOnOnAssetLoaded;
        }
        public Vector3 CalcPos(Vector3 position, Vector3 offset, Vector2 bgSize)
        {
            float offsetx = UIHelper.GetOffsetX(position, bgSize.x);
            var tipsPos = bottomTipNode.transform.localPosition;
            tipsPos.x = -offsetx;
            bottomTipNode.transform.localPosition = tipsPos;
       
            Vector3 localPos = content.InverseTransformPoint(position);
            tipsNode.localPosition = new Vector3(localPos.x + offsetx, localPos.y + offset.y, 0);
            return  position;
        }

        public override void Release()
        {
            list.OnAssetLoaded -= ListOnOnAssetLoaded;
            base.Release();
        }

        private void ListOnOnAssetLoaded()
        {
            canvasGroup.alpha = 1;
        }

        public Vector3 SetPosition(Vector3 pos, Vector3 offset)
        {
            Vector3 localPos = content.InverseTransformPoint(pos);
            return new Vector3(localPos.x + offset.x, localPos.y + offset.y, localPos.z + offset.z);
        }
        
    }
}