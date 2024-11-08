---
--- 文件名称:  UnityType
--- 创建者:    nieshihai
--- 创建时间:  2021/10/20 17:32
-------------------------------------------------------------------
--- 功能描述：
--- unitytype的集合， 属于全局的定义
---

--模块
---@class UnityType
local UnityType = {
    Addressable = 'Addressable', -- special 用于表示是通过资源加载的
    GameObject = "UnityEngine.GameObject",
    RectTransform = 'UnityEngine.RectTransform',
    Transform = 'UnityEngine.Transform',
    Button = 'UnityEngine.UI.Button',
    Dropdown = 'UnityEngine.UI.Dropdown',
    Text = 'UnityEngine.UI.Text',
    InputField = 'UnityEngine.UI.InputField',
    Image = 'UnityEngine.UI.Image',
    ToggleGroup = 'UnityEngine.UI.ToggleGroup',
    Toggle = 'UnityEngine.UI.Toggle',
    TMPText = 'TMPro.TMP_Text',
    TMPInputField = 'TMPro.TMP_InputField',
    UICircularScrollView = 'DH.UIFramework.UICircularScrollView',
    HoldButton = "DH.UIFramework.HoldButton",
    Slider = 'UnityEngine.UI.Slider',
    TMPTextEventHandler = "DH.UIFramework.TMP_TextEventHandler"
}

return UnityType