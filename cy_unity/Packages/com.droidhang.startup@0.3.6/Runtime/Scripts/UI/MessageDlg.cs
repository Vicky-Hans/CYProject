using System;
using System.Collections;
using System.Collections.Generic;
using DH.Launch;
using UnityEngine;
using UnityEngine.UI;

public class MessageDlg : MonoBehaviour
{
    public static MessageDlg Instance { get; set; }

    public GameObject CancelBtnObj;
    public Text TitleTxt;
    public Text DescTxt;
    public Text ConfirmBtnTxt;
    public Text CancelBtnTxt;
    
    private Action<bool> onButtonClick;
    
    private void Awake()
    {
        Instance = this;
    }

    public void InitUI(TaskManager.DlgType dlgType, string title, string desc, string yesBtnText, string noBtnText, Action<bool> clickCallback)
    {
        onButtonClick = clickCallback;
        CancelBtnObj.SetActive(dlgType == TaskManager.DlgType.ConfirmCancel);
        
        TitleTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(title);
        DescTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(desc);
        ConfirmBtnTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(yesBtnText);
        CancelBtnTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(noBtnText);
    }

    public void OnConfirmButtonClick()
    {
        onButtonClick?.Invoke(true);
    }

    public void OnCancelButtonClick()
    {
        onButtonClick?.Invoke(false);
    }
}
