using DH.HotService;
using DH.NativeCore.MonoSingleton;
using UnityEngine;
using UnityEngine.UI;

public class StartupMainDlg : MonoBehaviour
{
    public static StartupMainDlg Instance { get; set; }
    
    public GameObject ProgressObj;
    public Text DescTxt;
    public Text ProgressTxt;
    public Text VersionTxt;
    public Slider ProgressSlider;
    public GameObject ProgressIcon;

    public Image ProgressBar;
    private void Awake()
    {
        Instance = this;
        VersionTxt.text = $"version:{HotUpdateUtils.GetVersion()}";

        AdjustScreenAdaptor();
    }

    /// <summary>
    /// 刷新登录界面进度条
    /// </summary>
    /// <param name="progress"></param>
    public virtual void RefreshProgress(float progress)
    {
        // ProgressSlider.value = progress;
        ProgressBar.fillAmount = progress;
        if (ProgressTxt)
        {
            var curProgress = Mathf.Min(100, (int)(progress * 100));
            ProgressTxt.text = $"{curProgress}%";
        }
        var total = ProgressIcon.transform.parent.GetComponent<RectTransform>().rect.width;
        ProgressIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(progress*813,8.5f );//位置暂时写死
    }

    /// <summary>
    /// 刷新登录界面文本显示
    /// </summary>
    /// <param name="progress"></param>
    public virtual void RefreshText(string descText)
    {
        DescTxt.text = descText;
    }

    private void AdjustScreenAdaptor()
    {
        float matchValue = 0;
        var screenResolution = Screen.currentResolution;
        if (screenResolution.height / screenResolution.width > 1920f / 1080f)
        {
            matchValue = 0;
        }
        else
        {
            matchValue = 1;
        }
        var cavas = GetComponentInParent<CanvasScaler>();
        cavas.matchWidthOrHeight = matchValue;
    }
}
