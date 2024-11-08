using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using Cysharp.Threading.Tasks;
using DH.Game.UIViews;
using DH.UIFramework;
using TMPro;


namespace DH.Game.ViewModels
{
    public enum GmParamType
    {
        Base,
        Item,
        Talent,
        Equip,
        Clothes
    }

    public partial class GmParamViewModel : ViewModelBase
    {

        public int Pos;
		private TMP_InputField inputParamField;
		[AutoNotify] private string placeholderStr;

        private Action<int,string> inputChangedAction;

        public TMP_InputField InputParamField
        {
            get => inputParamField;
            set
            {
                Set(ref inputParamField, value);
                if (inputParamField != null)
                {
                    inputParamField.text = string.Empty;
                    inputParamField.onValueChanged.AddListener(OnValueChanged);
                }
            }
        }

        [AutoNotify] private GmParamType paramType;
        [Preserve]
        public GmParamViewModel(int pos,string desc,Action<int,string> action)
        {
            PlaceholderStr = desc;
            Pos = pos;
            inputChangedAction = action;
            RefreshSelect(desc);
        }

        private void RefreshSelect(string desc)
        {
            if (desc == "道具Id")
            {
                ParamType = GmParamType.Item;
            }
            else if (desc == "天赋Id")
            {
                ParamType = GmParamType.Talent;
            }
            else if (desc == "装备Id")
            {
                ParamType = GmParamType.Equip;
            }else if (desc == "服饰Id")
            {
                ParamType = GmParamType.Clothes;
            }
            else
            {
                ParamType = GmParamType.Base;
            }
        }

        private void OnValueChanged(string value)
        {
            inputChangedAction?.Invoke(Pos,value);
        }

        [Command]
        private void OnClockSelect()
        {
            if (ParamType != GmParamType.Base)
            {
                var model = new GmSelectItemViewModel(ParamType);
                model.SelectInfoAction = (itemId) =>
                {
                    inputChangedAction?.Invoke(Pos,itemId.ToString());
                    InputParamField.text = itemId.ToString();
                };
                UIManager.Instance.OpenDialog<GmSelectItemView>(model).Forget();
            }
        }
    }
}