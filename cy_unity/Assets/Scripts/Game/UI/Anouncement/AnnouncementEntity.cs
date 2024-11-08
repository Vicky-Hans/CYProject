using Cysharp.Threading.Tasks;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace DH.ComponentUI
{
    public enum ReadFlagType
    {
        Unread = 0,
        ReadFinish = 1,
    }

    [Preserve]
    public class AnnouncementEntity : ObservableObject
    {
        public string url;
        public long time;
        public string title;
        public int type;
        public long end_time;
        public int dialog_type;
        public int dialog_id;
        public string group_id;

        public string Content
        {
            get => content;
            set => Set(ref content, value);
        }
        
        public SimpleCommand SelectCmd
        {
            get => selectCmd;
            set => Set(ref selectCmd, value);
        }
        
        public bool IsLastItem { get; set; }

        public bool Selected
        {
            get => selected;
            set => Set(ref selected, value);
        }

        public ReadFlagType ReadFlag
        {
            get => readFlag;
            set
            {
                Set(ref readFlag, value);
            }
        } 
        
        private string content;
        private SimpleCommand selectCmd;
        private bool selected;
        private ReadFlagType readFlag = ReadFlagType.Unread;
        private bool hasRequestUrl = false;

        public void Init(AnnouncementViewModel owner)
        {
            ReadFlag = owner.LastReadTime < time ? ReadFlagType.Unread : ReadFlagType.ReadFinish;
            hasRequestUrl = string.IsNullOrEmpty(url);
            
            SelectCmd = new SimpleCommand(() =>
            {
                owner.CurrentAnnouncementItem = this;
                // UComponentUI.Instance.Adapter.PlayClickSound();
            });
        }

        public void OnSelected()
        {
            GetAnnouncementContent().Forget();
            Selected = true;
            ReadFlag = ReadFlagType.ReadFinish;
        }

        public void OnUnSelected()
        {
            Selected = false;
        }

        public bool IsRead()
        {
            return ReadFlag == ReadFlagType.ReadFinish;
        }
        
        private async UniTaskVoid GetAnnouncementContent()
        {
            if (hasRequestUrl)
            {
                return;
            }
                
            //download from network
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();
                
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    DHLog.Error($"[Announcement]:{url} Error: {request.error}");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    DHLog.Error($"[Announcement]:{url} Error: {request.error}");
                    break;
                case UnityWebRequest.Result.Success:
                    Content = request.downloadHandler.text;
                    hasRequestUrl = true;
                    break;
            }
                
            request.Dispose();
        }
    }
}