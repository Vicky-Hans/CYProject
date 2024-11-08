using System.Security.Cryptography;
using System.Text;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class SecretSurveysViewModel : ViewModelBase
    {
        [AutoNotify] private string contentStr;
        [AutoNotify] private ObservableList<CellItemBaseViewModel> rewardsScrollviewList = new();
        private ClickTextComponent clickTextComp;

        public ClickTextComponent ClickTextCmp
        {
            get => clickTextComp;
            set
            {
                clickTextComp = value;
                if(clickTextComp == null) return;
                clickTextComp.ClickCallback = OnClickLinkCallback;
            }
        }

        [Preserve]
        public SecretSurveysViewModel()
        {
            ContentStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_07);
            var defCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_20);
            if(defCfg== null || defCfg.Reward == null || defCfg.Reward.Count == 0) return;
            RewardsScrollviewList.Clear();
            foreach (var reward in defCfg.Reward)
            {
                var tempVm = CellItemBaseViewModel.Create(reward);
                RewardsScrollviewList.Add(tempVm);
            }
            
        }
        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<SecretSurveysView>();
        }


        private void OnClickLinkCallback(string info, Vector3 arg2)
        {
            //处理点击回调
            if (info == "handler")
            {
                var roleId = DataCenter.charcaterData.Digest.RoleId;
                string roleIdStr = GetMD5Hash($"{roleId}cyx");
                var tempUrl = $"{GameConst.SecretSurveysUrl}&entry.1450903716={roleId}&entry.447254343={roleIdStr}";
                Application.OpenURL(tempUrl);
            }
        }
        
        string GetMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
            
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
            // return "";
        }

    }
}