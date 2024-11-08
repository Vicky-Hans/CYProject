using DG.Tweening;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class CommonPlayerNameViewModel : ViewModelBase
    {
        [AutoNotify] private Color nameTextColor;
        [AutoNotify] private string nameTextStr;
        [AutoNotify] private bool isShowGName;
        private RectTransform lightImg;
        

        [Preserve]
        public CommonPlayerNameViewModel(string name,Color NameTextColor,bool isGold = false)
        {
            InitUI(name,NameTextColor,isGold);
        }
        
        public static CommonPlayerNameViewModel OnCreate(RankMember rankMember)
        {
            if (rankMember == null) return null;
            return new CommonPlayerNameViewModel(rankMember.Name,UIHelper.HexColorStrToColor("#6d4f3a"), UIHelper.IsGoldName(rankMember.VipStatus));
        }



        public void InitUI(string name,Color nameTextColor,bool isGold = false)
        {
            NameTextStr = name;
            NameTextColor = nameTextColor;
            IsShowGName = isGold;
        }
    }
}