using System.Collections;
using DH.ComponentUI;
using DH.UIFramework;
using DH.UIFramework.Builder;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class AnnouncementTabItemView : BaseItemView
{
    public GameObject LineObj;
    public GameObject SelectedObj;
    public GameObject RedDotObj;
    public TextMeshProUGUI TabText;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            SelectedObj.SetActive(value);
            TabText.color = value
                ? new Color(103 / 255f, 46 / 255f, 0f, 1f)
                : new Color(1f, 246 / 255f, 223 / 255f, 1f);
            
        }
    }

    private Button selectBtn;
    private bool selected;
    public override async Cysharp.Threading.Tasks.UniTask Create()
    {
        await base.Create();
        selectBtn = GetComponent<Button>();
        
        BindingSet<AnnouncementTabItemView, AnnouncementEntity> bindingSet = this.CreateBindingSet<AnnouncementTabItemView, AnnouncementEntity>();
        
        bindingSet.Bind(LineObj).For(v => v.activeSelf).ToExpression(vm => !vm.IsLastItem && !vm.Selected);
        bindingSet.Bind(this).For(v => v.Selected).To(vm => vm.Selected);
        bindingSet.Bind(selectBtn).For(v => v.onClick).To(vm => vm.SelectCmd);
        bindingSet.Bind(TabText).For(v => v.text).To(vm => vm.title);
        bindingSet.Bind(RedDotObj).For(v => v.activeSelf).ToExpression(vm => vm.ReadFlag == ReadFlagType.Unread);
        bindingSet.Build();
    }
}
