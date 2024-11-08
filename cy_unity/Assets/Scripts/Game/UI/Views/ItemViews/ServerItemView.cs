using DH.Game.Login;
using DH.UIFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class ServerItemView : BaseItemView
    {
        public TextMeshProUGUI serverName;
        public TextMeshProUGUI roleName;
        public TextMeshProUGUI serverStatus;
        public Image outline;
        public Button selectBtn;
        public Color[] statusColor;
        public Image statusIcon;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();

            var bindingSet = this.CreateBindingSet<ServerItemView, ServerInfo>();
            bindingSet.Bind(serverName).For(v => v.text).ToExpression(vm => GetServerName(vm.Name, vm.Sid));
            bindingSet.Bind(outline).For(v => v.enabled).To(vm => vm.Selected);
            bindingSet.Bind(selectBtn).For(v => v.onClick).To(vm => vm.SelectCmd);
            bindingSet.Bind(roleName).For(v => v.text).To(vm => vm.Digest.name);
            bindingSet.Bind(serverStatus).For(v => v.text).ToExpression(vm => StatusText(vm.Status));
            bindingSet.Bind(statusIcon).For(v => v.color).ToExpression(vm => StatusColor(vm.Status));
            bindingSet.Bind(serverStatus).For(v => v.color).ToExpression(vm => StatusColor(vm.Status));
            bindingSet.Build();
        }


        /// <summary>
        /// 0 维护 1 流畅 2 拥挤 3 爆满
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private Color StatusColor(uint status)
        {
            return statusColor[status];
        }

        public string GetServerName(string name, int sid)
        {
            return $"{sid} {name} ";
        }

        /// <summary>
        /// 0 维护 1 流畅 2 拥挤 3 爆满
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private string StatusText(uint status)
        {
            switch (status)
            {
                case 0:
                    return "维护";

                case 1:
                    return "流畅";

                case 2:
                    return "拥挤";

                case 3:
                    return "爆满";

                default:
                    return "Unknown";
            }
        }
    }
}