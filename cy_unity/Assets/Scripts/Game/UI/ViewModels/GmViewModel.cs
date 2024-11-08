using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public enum GMType{
        Base,
        EquipTalent,
        Equip,
    }

    public class GMItemInfo
    {
        public int DMId;
        public GMType DmType;
        public string DMDesc;
        public string DMInfo;
        public List<string> GMParams;
    }

    public partial class RecordItemViewModel : ViewModelBase
    {
        [AutoNotify] private Action clickCallBack;
        [AutoNotify] private string recordStr;
        public RecordItemViewModel(string record)
        {
            RecordStr = record;
        }

        [Command]
        private void OnClickRecordBtn()
        {
            ClickCallBack?.Invoke();
        }
    }

    public partial class GmViewModel : ViewModelBase
    {
        [AutoNotify] private bool isShowItemList = false;
        public ObservableList<GMItemInfo> GMItemList= new ()
        {
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "Item 101",DMInfo = "添加道具",GMParams = new List<string>{"道具Id","道具数量"}},//"道具Id" 这个值会在其他地方判断类型，切勿修改
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "heroEquip 101",DMInfo = "添加服饰",GMParams = new List<string>{"服饰Id","服饰数量","服饰品质"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "heroEquip 102",DMInfo = "清空服饰",GMParams = new List<string>()},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "heroEquip 103",DMInfo = "自动合成",GMParams = new List<string>()},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "heroEquip 104",DMInfo = "修改服饰等级",GMParams = new List<string>(){"服饰Uid","服饰等级"}},
            new GMItemInfo(){DmType = GMType.EquipTalent,DMDesc = "Battle Talent",DMInfo = "战斗天赋",GMParams = new List<string>{"天赋Id","天赋Id","天赋Id"}},
            new GMItemInfo(){DmType = GMType.Equip,DMDesc = "Battle Equip",DMInfo = "战斗装备",GMParams = new List<string>{"装备Id","装备Id","装备Id"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "gfs",DMInfo = "一键变强",GMParams = new List<string>()},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "offset",DMInfo = "推迟时间多少秒",GMParams = new List<string>{"推迟秒数"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "time",DMInfo = "修改日期到指定时间",GMParams = new List<string>{"年","月","日","时","分","秒"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "card 101",DMInfo = "购买月卡 1-普通 2-高级",GMParams = new List<string>{"月卡等级"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "stage 101",DMInfo = "解锁关卡",GMParams = new List<string>{"关卡Id"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "lives 101",DMInfo = "扣除体力",GMParams = new List<string>{"扣除数量"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "lives 104",DMInfo = "补充体力",GMParams = new List<string>()},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "lives 105",DMInfo = "增加体力",GMParams = new List<string>{"增加数量"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "lives 106",DMInfo = "消耗体力",GMParams = new List<string>{"消耗的数量"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "lives 102",DMInfo = "查看体力信息",GMParams = new List<string>()},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "lives 103",DMInfo = "重置体力可获得次数",GMParams = new List<string>()},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "role 101",DMInfo = "增加等级",GMParams = new List<string>{"等级"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "role 102",DMInfo = "增加经验",GMParams = new List<string>{"经验"}},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "role 103",DMInfo = "重置改名cd",GMParams = new List<string>()},
            new GMItemInfo(){DmType = GMType.Base,DMDesc = "Shop 105",DMInfo = "刷新每日商店",GMParams = new List<string>()},

        };

        [AutoNotify] private ObservableList<GmItemInfoVIewModel> gmItemInfoList = new();
        [AutoNotify] private GMItemInfo selectGmInfo;
        [AutoNotify] private ObservableList<GmParamViewModel> gmItemInfoparamList = new();
        [AutoNotify] private ObservableDictionary<int, string> paramInfoDic = new ObservableDictionary<int, string>();
        public class GmCommandItemModel : ObservableObject
        {
            private string desc;
            private string command;
            private string commandKey;
            
            public string Desc
            {
                get => desc;
                set => Set(ref desc,value);
            }

            public string Command
            {
                get => command;
                set => Set(ref command,value);
            }

            public string CommandKey
            {
                get => commandKey;
                set => Set(ref commandKey,value);
            }

            public ICommand SelectCmd { get; set; }
        }
        
        private string defaultStr =" 格式：客户端(c/C)+空格+技能(skill/Skill)或者物品(item/Item)/ 科技(tech/Tech) + 空格 + 操作id(101/102/weaponskillId)+ 竖线(|)" +
                                   "+等级或者数量\neg\n C/c skill weaponSkillId \n(主动添加技能（weaponskillid 不会检查前后置 次数，解锁条件等关系）) " +
                                   "\n C/c item 101 100|100  \n(加物品 id为100 数量 100个) \n c/c tech 101 1";


        private string gmCommandInput;

        public string GmCommandInput
        {
            get => gmCommandInput;
            set
            {
                Set(ref gmCommandInput, value);
            }
        }

        private string desc;
        
        private TMP_InputField commandInput;

        public TMP_InputField CommandInput
        {
            get => commandInput;
            set
            {
                Set(ref commandInput, value);
                if (commandInput != null)
                {
                    commandInput.onValueChanged.AddListener(OnValueChanged);
                }
            }
        }
       [Preserve]
        public GmViewModel()
        {
            GetGmData().Forget();
            InitRecordDic();
            InitItemInfo();
            InitItemInfoParams();
        }

        private void InitItemInfo()
        {
            GmItemInfoList.Clear();
            for (int i = 0; i < GMItemList.Count; i++)
            {
                GmItemInfoList.Add(new GmItemInfoVIewModel(i,$"{GMItemList[i].DMDesc}    {GMItemList[i].DMInfo}",OnClickItemInfo));
            }
        }

        private void OnClickItemInfo(int index)
        {
            IsShowItemList = false;
            SelectGmInfo = GMItemList[index];
            InitItemInfoParams();
        }
        
        private void InitItemInfoParams()
        {
            GmItemInfoparamList.Clear();
            if(SelectGmInfo == null) return;
            for (int i = 0; i < SelectGmInfo.GMParams.Count; i++)
            {
                GmItemInfoparamList.Add(new GmParamViewModel(i,$"{SelectGmInfo.GMParams[i]}", (pos,value) =>
                {
                    ParamInfoDic[pos] = value;
                    InitGmInfo();
                }));
            }
            InitGmInfo();
        }

        private List<int> GetEquipGmInfo()
        {
            var infoList = new List<int>();
            for (int i = 0; i < SelectGmInfo.GMParams.Count; i++)
            {
                if (ParamInfoDic.TryGetValue(i, out string value))
                {
                    int id = 0;
                    int.TryParse(value,out id);
                    if(id!=0) infoList.Add(id);
                }
            }

            return infoList;
        }

        private void InitGmInfo()
        {
            if(SelectGmInfo==null) return ;
            string desc = SelectGmInfo.DMDesc+" ";
            for (int i = 0; i < SelectGmInfo.GMParams.Count; i++)
            {
                if (ParamInfoDic.TryGetValue(i, out string value))
                {
                    if (i != 0) desc += "|";
                    desc += value;
                }
                else
                {
                    if (i != 0) desc += "|";
                    desc += "0";
                }
            }
            GmCommandInput = desc;
        }
        
        private bool CheckGmInfo(string value)
        {
            if(SelectGmInfo==null) return false;
            return value.Contains(SelectGmInfo?.DMDesc ?? string.Empty);
        }

       
        
        private void OnValueChanged(string value)
        {
            if (!CheckGmInfo(value))
            {
                SelectGmInfo = null;
                InitItemInfoParams();
            }
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SendGmCommand();
            }
        }

        public void SendGmCommand()
        {
            if (SelectGmInfo is { DmType: GMType.EquipTalent or GMType.Equip })
            {
                SendGmRefreshEquipBattle().Forget();
            }
            else
            {
                SendGmCommandWrap().Forget();
                SelectGmInfo = null;
            }
        }

        private async UniTaskVoid SendGmRefreshEquipBattle()
        {
            var sendInfo = new ReqBattleGm()
            {
                Cmd = SelectGmInfo.DmType == GMType.EquipTalent ? 1 : 2,
            };
            sendInfo.Ids.AddRange(GetEquipGmInfo());
            var result = await GameNetworkManager.Instance.SendAsync<RspBattleGm>(sendInfo);
            NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
            {
                if (sendInfo.Cmd == 1)
                {
                    GameDataManager.Instance.UnOpTalents = result.rsp.Ids.ToList();
                    List<int> tempList = new();

                    Action<int, Action<List<int>>> selectCallback = (index, callback) =>
                    {
                        UIManager.Instance.CloseDialog<TalentChooseView>();
                    };

                    TalentChooseViewModel tempVm = new(result.rsp.Ids.ToList(),selectCallback);
                    UIManager.Instance.OpenDialog<TalentChooseView>(tempVm).Forget();
                }
                else
                {
                    GameDataManager.Instance.Equips = result.rsp.Ids.ToList();
                }

                SelectGmInfo = null;
            });
        }

        public void OnClickOpenGmList()
        {
            IsShowItemList = !IsShowItemList;
        }
        
        public void Exit()
        {
            UIManager.Instance.CloseTopDialog().Forget();
        }

        private async UniTaskVoid SendGmCommandWrap()
        {
            var strArray = GmCommandInput?.Split(" ");
            if(strArray==null) return;
            if(strArray.Length <=0) return;
            if (strArray[0] is "C" or "c")
            {
                if (strArray.Length is 3 or 4)
                {
                    var opId = int.Parse(strArray[2]);
                    var id = 0;
                    var count = 1;

                    if (strArray.Length == 4)
                    {
                        var infoArray = strArray[3].Split("|");
                        id = int.Parse(infoArray[0]);
                        count = infoArray.Length > 1 ? int.Parse(infoArray[1]) : 1; 
                    }

                    Desc = $"操作成功 {GmCommandInput}";
                    
                    ToastManager.Show($"CGM指令{gmCommandInput}发送成功!");
                    
                    AddRecordToList(gmCommandInput);
                    UpdateRecordStringList(gmCommandInput);
                }
                else
                {
                    ToastManager.Show($"前端指令格式错误!");
                    Desc = defaultStr;
                }
            }
            else
            {
                var result = await GameNetworkManager.Instance.SendAsync<RspGm>(new ReqGm()
                {
                    Cmd = gmCommandInput
                });
                if ((result.rsp?.Status ?? -1) == 0)
                {
                    ToastManager.Show($"GM指令{gmCommandInput}发送成功!");
                    Desc = $"\n {result.rsp.RetInfo}" ;
                    AddRecordToList(gmCommandInput);
                    UpdateRecordStringList(gmCommandInput);
                }
                else
                {
                    ToastManager.Show($"GM指令{gmCommandInput}发送失败!错误信息{result.rsp?.RetInfo}");
                }   
            }
        }

        public void OnNotifySyncGm(RspGmSync gmNotify)
        {
            DataCenter.Init(gmNotify.SyncInfo);
        }

        private async UniTaskVoid GetGmData()
        {
            
            var result = await GameNetworkManager.Instance.SendAsync<RspGm>(new ReqGm(){Cmd = "Help"});
            var rsp = result.rsp;
            if (rsp == null || rsp.Status != 0)
            {
                ToastManager.Show(rsp?.RetInfo ?? "无法连接服务器，GM工具无法触发自动重连");
                return;
            }

            Desc = $"{rsp.RetInfo}";
        }

        private void OnSelectCommand(GmCommandItemModel commandItemModel)
        {
            GmCommandInput = commandItemModel.Command;
            

        }

        public string Desc
        {
            get => desc;
            set
            {
                Set(ref desc, value);
            }
        }
        

        [AutoNotify] private ObservableList<string> recordStringList = new();
        [AutoNotify] private ObservableList<RecordItemViewModel> recordList = new();
        private string ParseFileName()
        {
            return "gm_record";
        }

        private void InitRecordDic()
        {
            recordStringList = LoadRecordData();
            foreach (var item in recordStringList)
            {
                AddRecordToList(item);
            }
        }

        private void AddRecordToList(string key)
        {
            RecordItemViewModel tempVm = new RecordItemViewModel(key);
            tempVm.ClickCallBack = () =>
            {
                GmCommandInput = key;
            };
            RecordList.Insert(0,tempVm);
        }

        /// <summary>
        /// 存玩家的信息
        /// </summary>
        private void SaveRecordData()
        {
            // 存储数据
            string filePath = Path.Combine(Application.persistentDataPath, ParseFileName());
            
            string json = DHUtility.Json.ToJson(recordStringList);
            // 存储 JSON 数据到文件
            File.WriteAllText(filePath, json);
        }
        /// <summary>
        /// 加载玩家的引导文件
        /// </summary>
        /// <returns></returns>
        public ObservableList<string> LoadRecordData()
        {
            ObservableList< string> loadedData = new();
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, ParseFileName());
                string loadedJson = File.ReadAllText(filePath);
                loadedData = DHUtility.Json.ToObject<ObservableList<string>>(loadedJson);
                if (loadedData == null)
                {
                    ObservableList<string> ret = new();
                    return ret;
                }
                return loadedData;
            }
            catch (Exception e)
            {
                return loadedData;
            }
        }

        private void UpdateRecordStringList(string command)
        {
            if (recordStringList.Count > 20)
            {
                recordStringList.RemoveAt(0);
            }
            recordStringList.Add(command);
            SaveRecordData();
        }
        
        
        [Command]
        private void OnClickOneKeyGFS()
        {
            gmCommandInput = "gfs";
            SendGmCommandWrap().Forget();
        }
        
        
        [Command]
        private void OnCloseItemBg()
        {
            IsShowItemList = false;
        }
    }
    
}
