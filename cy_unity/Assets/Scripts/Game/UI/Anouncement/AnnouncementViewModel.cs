using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game;
using DH.Game.Login;
using DH.Game.UIViews;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using DHFramework;
using DHFramework.Localization;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.ComponentUI
{
    public class AnnouncementViewModel : ViewModelBase
    {
        public List<AnnouncementEntity> AnnouncementList
        {
            get => announcementList;
            set
            {
                Set(ref announcementList, value);
                OnSetAnnouncementList();
            }
        }

        public AnnouncementEntity CurrentAnnouncementItem
        {
            get => currentAnnouncementItem;
            set
            {
                if (currentAnnouncementItem == value)
                {
                    return;
                }
                
                if (currentAnnouncementItem != null)
                {
                    currentAnnouncementItem.OnUnSelected();
                }
                
                Set(ref currentAnnouncementItem, value);
                
                if (currentAnnouncementItem != null)
                {
                    currentAnnouncementItem.OnSelected();

                    if (LastReadTime < currentAnnouncementItem.time)
                    {
                        LastReadTime = (int)currentAnnouncementItem.time;
                    }
                }
            }
        }

        public SimpleCommand CloseDlgCmd
        {
            get => closeDlgCmd;
            set => Set(ref closeDlgCmd, value);
        }

        public SimpleCommand NextNoticeCmd
        {
            get => nextNoticeCmd;
            set => Set(ref nextNoticeCmd, value);
        }
    
        public string AnnouncementMd5
        {
            get => announcementMd5;
            set => announcementMd5 = value;
        }
        
        public int AnnouncementTotalDays
        {
            get => DHUnityUtil.PlayerPrefs.GetInt("AnnouncementTotalDays", 0);
            set => DHUnityUtil.PlayerPrefs.SetInt("AnnouncementTotalDays", value);
        }

        public int LastReadTime
        {
            get => DHUnityUtil.PlayerPrefs.GetInt("LastReadAnnouncementTime", 0);
            set => DHUnityUtil.PlayerPrefs.SetInt("LastReadAnnouncementTime", value);
        }

        private AnnouncementEntity currentAnnouncementItem;

        private bool isPulled = false;
        private int localizationId = 0;
        private string announcementMd5 = "";
        private List<AnnouncementEntity> announcementList;
        private SimpleCommand closeDlgCmd;
        private SimpleCommand nextNoticeCmd;
        private long newestAnnouncementTime = 0;
        private int curIndex = 0;
        private int clickServerCount;
        private float clickServerTime;
        
        private ClickTextComponent clickTextCmp;

        public ClickTextComponent ClickTextCmp
        {
            get => null;
            set { 
                clickTextCmp = value;
                if (clickTextCmp != null)
                {
                    clickTextCmp.ClickCallback = OnClickLinkCallback;
                }
            }
        }
        
        [Preserve]
        public AnnouncementViewModel()
        {
            isPulled = false;

            localizationId = (int)DHUtility.GetGameTime(DHUtility.TicksType.NS);
            Localization.RegisterLocalize(localizationId, ResetNotice);

            CloseDlgCmd = new SimpleCommand((() =>
            {
                UIManager.Instance.CloseDialog<AnnouncementDlgView>();
            }));
            
            NextNoticeCmd = new SimpleCommand(SetNextAnnounce);
        }
        
        private void OnClickLinkCallback(string info, Vector3 arg2)
        {
            //处理点击回调
            DHLog.Debug($"Click  {info}");
            if (info == "Mail")
            {
                Application.OpenURL(GameConst.MailUrl);
            }else if (info == "Discord")
            {
                Application.OpenURL(GameConst.DiscordUrl);
            }

        }

        public void Release()
        {
            Localization.UnRegisterLocalize(localizationId);
            AnnouncementMd5 = "";
            AnnouncementList = null;
        }
        
        public async UniTask Open(bool openUIForLogin,CancellationToken token)
        {
            if (!openUIForLogin && isPulled)
            {
                OpenAnnouncementDlg();
                return;
            }

            var code = await RequestAnnouncement();
            if (token.IsCancellationRequested)
            {
                return;
            }
            
            if (code == 0)
            {
                if (!openUIForLogin || newestAnnouncementTime > LastReadTime || CheckAnnouncementAutoOpen() )
                {
                    OpenAnnouncementDlg();
                }
            }
            else
            {
                //OnRequestAnnounceError();
            }
        }

        public void OpenAnnouncementDlg()
        {
            if (!CheckIsHaveAccount())
            {
                return;
            }
            UIManager.Instance.OpenDialog<AnnouncementDlgView>(this).Forget();
        }

        private bool CheckIsHaveAccount()
        {
            var servers = LoginManager.Instance.ServerInfos;
            bool ret = false;
            foreach (var item in servers)
            {
                if (item.Value.Digest != null && item.Value.Digest.roleId != 0)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public async UniTask<int> RequestAnnouncement()
        {
            var languageCode = Localization.GetCurrentLanguageNumber();
            var data = await Usdk.GetNotice(languageCode, AnnouncementMd5);
         
            if (data.errno == 0)
            {
                isPulled = true;
                
                //0:公告未修改,返回的notice和md5都为空。1:公告有修改，返回notice和对应的md5
                if (data.status == 1)
                {
                    var jsonText = data.notice;
                    var rspList = DHUtility.Json.ToObject<List<AnnouncementEntity>>(jsonText);

                    if (rspList == null)
                    {
                        DHLog.Debug($"[Notice] noticeList is Empty");
                        return 0;
                    }

                    AnnouncementMd5 = data.md5;
                    AnnouncementList = rspList;
                }
                else
                {
                    DHLog.Debug($"[Notice] noticeList status is not 1 : {data.status}");
                }

                return 0;
            }
            
            DHLog.Debug($"[Notice] noticeList request Error : {data.errno}");
            return data.errno;
        }

        public bool HasUnreadAnnouncement()
        {
            var exist = false;

            if (AnnouncementList != null)
            {
                foreach (var noticeEntity in announcementList)
                {
                    if (!noticeEntity.IsRead())
                    {
                        exist = true;
                    }
                }
            }

            return exist;
        }

        private bool CheckAnnouncementAutoOpen()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);

            if (timeSpan.TotalDays - AnnouncementTotalDays >= 1)
            {
                AnnouncementTotalDays = (int)timeSpan.TotalDays;
                return true;
            }

            return false;
        }

        private async UniTask ResetNotice()
        {
            isPulled = false;
            AnnouncementList = null;
        }
        
        private void OnSetAnnouncementList()
        {
            if (announcementList != null && announcementList.Count > 0)
            {
                newestAnnouncementTime = (long)LastReadTime;

                var curTime = DHUtility.GetGameTime(DHUtility.TicksType.S);
                for (int i = announcementList.Count - 1; i >= 0; --i)
                {
                    var entity = announcementList[i];

                    if (entity.end_time < curTime)
                    {
                        announcementList.RemoveAt(i);
                        continue;
                    }
                    
                    if (entity.time > newestAnnouncementTime)
                    {
                        newestAnnouncementTime = entity.time;
                    }
                }

                if (announcementList.Count > 0)
                {
                    foreach (var entity in announcementList)
                    {
                        entity.Init(this);
                    }
                    
                    announcementList[^1].IsLastItem = true;
                }
                
                if (CurrentAnnouncementItem == null)
                {
                    CurrentAnnouncementItem = announcementList[0];
                    curIndex = 0;
                }
            }
        }

        private void SetNextAnnounce()
        {
            if(announcementList != null && announcementList.Count > 1)
            {
                ++curIndex;
                curIndex %= announcementList.Count;
                CurrentAnnouncementItem = announcementList[curIndex];
            }
        }
        
        public void OnClickServerBtn()
        {
            if (clickServerCount == 0)
            {
                clickServerTime = Time.time;
            }
            clickServerCount++;
            if (clickServerCount == 10)
            {
                if (Time.time - clickServerTime <= 5f)
                {
                    LoginManager.Instance.ChangeServer();
                }
            }
            else if (clickServerCount > 10)
            {
                if (Time.time - clickServerTime > 5f)
                {
                    clickServerCount = 0;
                }
            }
        }
    }
}
