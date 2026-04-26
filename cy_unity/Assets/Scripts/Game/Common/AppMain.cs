using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Contexts;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class AppMain : MonoBehaviour
{
    [SerializeField] [Header("UI节点列表")] private List<Transform> UIRoots;
    [SerializeField] [Header("BaseCamera")] private Camera BaseCamera;
    [SerializeField] [Header("UICamera")] private Camera UICamera;
    [SerializeField] [Header("调试标记")] private bool IsDebug;
    [SerializeField] [Header("战斗调试标记")] private bool IsBattleDebug;
    [SerializeField] [Header("音效节点")] private GameObject AudioRoot;
    [SerializeField] [Header("音效节点")] private Transform audioListener;

    [SerializeField] [Header("常驻节点")] private List<GameObject> DontDestroys;

    [SerializeField] [Header("常驻节点")] private Button  DebugBtn;
    [SerializeField] [Header("Bgm混音器")] private AudioMixerGroup BgmMixer;
    [SerializeField] [Header("UI混音器")] private AudioMixerGroup UIMixer;
    [SerializeField] [Header("Skill混音器")] private AudioMixerGroup SkillMixer;
    [SerializeField] [Header("UI特效节点")] private Transform UIEffectRoot;
    [SerializeField] [Header("Canvas")] private Canvas UICanvas;

#if DH_DEBUG
    public KeyCode[] gmKeysToCheck= new []{KeyCode.LeftControl, KeyCode.Space};
    private bool isGmCheck = true;
    public KeyCode[] debugToolKeysToCheck= new []{KeyCode.LeftShift, KeyCode.Space};
    private bool isDebugToolCheck = true;

#endif
    
    private void Awake()
    {
        DebugBtn.gameObject.SetActive(false);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        foreach (var go in DontDestroys)
        {
            DontDestroyOnLoad(go);
        }

        AdjustScreenAdaptor();

        Application.targetFrameRate = 60;

        AppGlobal.Instance.UIRoots = UIRoots;
        AppGlobal.Instance.UICamera = UICamera;
        AppGlobal.Instance.BaseCamera = BaseCamera;
        AppGlobal.Instance.AudioRoot = AudioRoot;
        AppGlobal.Instance.IsDebug = IsDebug;
        AppGlobal.Instance.IsBattleDebug = IsDebug && IsBattleDebug;
        AppGlobal.Instance.GlobalMono = this;
        AppGlobal.Instance.BgmMixer = BgmMixer;
        AppGlobal.Instance.UIMixer = UIMixer;
        AppGlobal.Instance.SkillMixer = SkillMixer;
        AppGlobal.Instance.UIEffectRoot = UIEffectRoot;
        AppGlobal.Instance.UICanvas = UICanvas;
        
        DontDestroyOnLoad(EventSystem.current.gameObject);

        if (IsBattleDebug)
        {
            StartGameInternal().Forget();
        }
        
        CameraManager.Instance.Init();
        
        var value = PlayerPrefs.GetInt("DebugMode");
        if (value != 0)
        {
            SRDebug.Init();
        }
#if DH_DEBUG
        InitDebugInfo();
        
#endif
    }

    private void AdjustScreenAdaptor()
    {
        float matchValue = 0;
        var screenResolution = Screen.currentResolution;
        if (screenResolution.height * 1f / screenResolution.width > 1920f / 1080f)
        {
            matchValue = 0;
        }
        else
        {
            matchValue = 1;
        }
        var cavas = GetComponentInChildren<CanvasScaler>();
        cavas.matchWidthOrHeight = matchValue;
    }

    private void InitDebugInfo()
    {
        DebugBtn.gameObject.SetActive(!GameConst.IsIosAuditState);
        DebugBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenDialog<DebugView, DebugViewModel>().Forget();
        });
    }

    private void Update()
    {
        ProcedureManager.Instance.Update(Time.deltaTime,Time.unscaledDeltaTime);
#if DH_DEBUG
        AddListenerForKeyBord();
#endif
    }

#if DH_DEBUG
    private void AddListenerForKeyBord()
    {
        CheckGMKeyBord().Forget();
        CheckDebugToolKeyBord();
    }

    private async void CheckDebugToolKeyBord()
    {
        if (!isDebugToolCheck) return;
        bool allKeysPressed = true;
        // 检查所有按键是否被按下
        foreach (KeyCode key in debugToolKeysToCheck)
        {
            if (!Input.GetKey(key))
            {
                allKeysPressed = false;
                break;
            }
        }
        // 如果所有按键都被按下，则执行相应的操作
        if (allKeysPressed)
        {
            Debug.Log("Combo key pressed!");
            // 在这里执行你想要的操作
            bool isOpenDebug = UIManager.Instance.IsOpen<DebugView>();
            if (isOpenDebug)
            {
                UIManager.Instance.CloseDialog<DebugView>();
            }
            else
            {
                UIManager.Instance.OpenDialog<DebugView, DebugViewModel>().Forget();
            }
            
            isDebugToolCheck = false;
            await UniTask.Delay(700);
            isDebugToolCheck = true; 
        }
    }

    private async UniTaskVoid CheckGMKeyBord()
    {
        if (!isGmCheck) return;
        bool allKeysPressed = true;
        // 检查所有按键是否被按下
        foreach (KeyCode key in gmKeysToCheck)
        {
            if (!Input.GetKey(key))
            {
                allKeysPressed = false;
                break;
            }
        }
        // 如果所有按键都被按下，则执行相应的操作
        if (allKeysPressed)
        {
            Debug.Log("Combo key pressed!");
            // 在这里执行你想要的操作
            bool isOpenDebug = UIManager.Instance.IsOpen<GmView>();
            if (isOpenDebug)
            {
                UIManager.Instance.CloseDialog<GmView>();
            }
            else
            {
                UIManager.Instance.OpenDialog<GmView, GmViewModel>().Forget();
            }
            
            isGmCheck = false;
            await UniTask.Delay(700);
            isGmCheck = true; 
        }
    }
#endif
    private void OnDestroy()
    {
        ProcedureManager.Instance.Dispose();
    }

    private async UniTask StartGameInternal()
    {
        UIManager.Instance.Init();
        ProcedureManager.Instance.Init();
        DataTableManager.SetLoadAdapter(new DataLoadAdapter());
        ConfigCenter.InitConfigs();
        var context = Context.GetApplicationContext();
        var bundle = new BindingServiceBundle(context.GetContainer());
        bundle.Start();
        if (IsBattleDebug) return;
#if UNITY_WEBGL || WECHAT_MINI
        StartupEntry.Instance.WebGLStartGame = () =>
        {
            ProcedureManager.Instance.Change(ProcedureConfigKey.GlobalInitProcedure).Forget();
        };
#else
        await ProcedureManager.Instance.ChangeAsync(nameof(GlobalInitProcedure));
#endif
    }
    
    [Preserve]
    public static async UniTask StartGame()
    {
        var root = GameObject.Find("AppMain");
        if (root)
        {
            await root.GetComponent<AppMain>().StartGameInternal();
            return;
        }
        
        var startupCamera = GameObject.Find("StartupCamera");
        root = await AssetsManager.InstantiateAsync("LauncherPrefabs/AppMain");
        Destroy(startupCamera);
        await root.GetComponent<AppMain>().StartGameInternal();
    }
}
