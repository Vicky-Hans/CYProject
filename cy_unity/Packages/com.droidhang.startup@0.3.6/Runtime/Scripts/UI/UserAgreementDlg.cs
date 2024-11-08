using System;
using System.Collections;
using System.Collections.Generic;
using DH.Launch;
using UnityEngine;
using UnityEngine.UI;

public class UserAgreementDlg : MonoBehaviour
{
    public static UserAgreementDlg Instance { get; set; }

    public Text TitleTxt;
    public Text ContentTxt;
    public Text UserAgreementTxt;
    public Text PrivacyAgreementTxt;
    public Text RefuseBtnTxt;
    public Text AgreeBtnTxt;
    
    private Action<bool> onButtonClick;
    
    private void Awake()
    {
        Instance = this;
    }

    public void InitUI(Action<bool> clickCallback)
    {
        onButtonClick = clickCallback;
        
        TitleTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(LanguageId.DialogTitle);
        ContentTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(LanguageId.UserAgreementContent);
        UserAgreementTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(LanguageId.UserAgreement);
        PrivacyAgreementTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(LanguageId.PrivacyAgreement);
        AgreeBtnTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(LanguageId.Agree);
        RefuseBtnTxt.text = StartupEntry.Instance.LanguageConfig.GetDataById(LanguageId.Reject);
    }

    public void OnOpenUserAgreementClick()
    {
        var url = StartupEntry.Instance.GetUserAgreementUrl();
        Application.OpenURL(url);
    }

    public void OnOpenPrivacyAgreementClick()
    {
        var url = StartupEntry.Instance.GetPrivacyAgreement();
        Application.OpenURL(url);
    }

    public void OnAgreeBtnClick()
    {
        onButtonClick?.Invoke(true);
    }

    public void OnRefuseBtnClick()
    {
        onButtonClick?.Invoke(false);
    }
}
