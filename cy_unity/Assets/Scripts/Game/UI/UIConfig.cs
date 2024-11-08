using System;
using DHFramework;

public enum UILayersConfig
{
    Min = 0,
    Scene,
    UI,
    Guide,
    Wait,
    Toast,
    Max,
}


public static partial class UIConfig
{
    public static UIConfigItem GetUIConfigItem(Type type)
    {
        UIConfigItem configItem;
        if (configItems.TryGetValue(type, out configItem))
        {
            return configItem;
        }
        else
        {
            DHLog.Error($"Try get a not exist uikey : {type.FullName}");
        }
        return null;
    }
}

public class UIConfigItem
{
    public string path;
    public UILayersConfig layer;
    public bool isFullScreen;
    public bool isMulti;

    public UIConfigItem(string path, UILayersConfig layer = UILayersConfig.UI, bool isFullScreen = true, bool isMulti = false)
    {
        this.path = path;
        this.layer = layer;
        this.isFullScreen = isFullScreen;
        this.isMulti = isMulti;
    }
}