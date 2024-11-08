using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;
using DHFramework.Localization;
using Extend;
using Game.UI.CommonView;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Attribute = DH.Config.Attribute;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public enum AlignType
{
    Vertical,
    Horizontal
}

public enum AlignPriority
{
    None,
    First,
    Last
}

public enum TimeRefreshCycle
{
    Base,
    Day,
    Week,
}

public struct AdjustParam
{
    public static readonly AdjustParam Default = new()
    {
        alignType = AlignType.Vertical,
        forceAlignEdge = AlignPriority.None,
        forceAlignPosi = AlignPriority.None,
        distance = 0,
        rebuildLayoutBeforeAdjust = false
    };

    public AlignType alignType;

    /// <summary>
    ///     force align position.
    ///     when alignType is Vertical, then AlignPriority.First mean put adjust RectTransform on top,
    ///     otherwise, down.
    ///     when alignType is Horizontal, then AlignPriority.First mean put adjust RectTransform on left,
    ///     otherwise, right.
    /// </summary>
    public AlignPriority forceAlignPosi;

    /// <summary>
    ///     force align edge.
    ///     when alignType is Vertical, then AlignPriority.First mean put adjust RectTransform left edge
    ///     align with target
    ///     RectTransform left edge, otherwise, right edge.
    ///     when alignType is Horizontal, then AlignPriority.First mean put adjust RectTransform top edge
    ///     align with target
    ///     RectTransform top edge, otherwise, bottom edge.
    /// </summary>
    public AlignPriority forceAlignEdge;

    /// <summary>
    ///     align distance between adjust RectTransform and target RectTransform
    /// </summary>
    public float distance;

    /// <summary>
    ///     rebuild ui layout before start adjust
    /// </summary>
    public bool rebuildLayoutBeforeAdjust;
}

public enum SkillPreviewSkillType
{
    None,
    EquipModel,
    HeroEquip,
}

public static class UIHelper
{
    public static List<string> QuaColor = new()
    {
        "#D0DAE5",
        "#9EDE91",
        "#A4E9FF",
        "#FDC7FF",
        "#F3D174",
        "#F58C7E",
        "#ff4a4d"
    };
    
    public static List<string> QuaColor1 = new()
    {
        "#5F6B78",
        "#34652A",
        "#2C8DB5",
        "#C13BC6",
        "#DF7900",
        "#9E2818",
        "#ff4a4d"
    };

    public static void SetText(TMP_Text textComp, string text)
    {
        textComp.text = text;
    }

    public static void SetGray(GameObject gameObject, bool gray, bool recursive = true,
        bool purple = false)
    {
        var textMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
        if (textMeshProUGUI != null)
            SetTextGray(textMeshProUGUI, gray, purple);

        var imageCom = gameObject.GetComponent<Image>();
        if (imageCom != null)
            SetImageGray(imageCom, gray, purple).Forget();
        if (recursive)
        {
            var children = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var childTMPText in children)
                SetTextGray(childTMPText, gray, purple);

            var images = gameObject.GetComponentsInChildren<Image>();
            foreach (var image in images)
                SetImageGray(image, gray, purple).Forget();
        }
    }

    private static readonly StringBuilder strBuilder = new();

    private static void SetTextGray(TextMeshProUGUI texCmp, bool gray, bool purple)
    {
        if (texCmp.fontSharedMaterial.Equals(null))
        {
            DHLog.Error(
                $"[localization]fontSharedMaterial is null, current language: {Localization.GetCurrentLanguage()}");
            return;
        }

        var font = texCmp.font;
        var curProps = texCmp.fontSharedMaterial.name.Split('_');
        var props = new string[3];
        for (var i = 0; i < Mathf.Clamp(curProps.Length, 0, 3); ++i) props[i] = curProps[i];

        if (gray)
        {
            if (font.material.name != props[0])
                props[0] = props[0] ?? "default"; // 无描边需要填充default
            else
                props[0] = "default";
            if (purple)
                props[1] = "pure";
            else
                props[1] = "gray";
        }
        else
        {
            props[1] = null;
            if (props[0] == "default")
                props[0] = null;
        }

        // 如果属性数量大于1 表示有特殊效果 拼接材质名
        if (props.Length > 1)
        {
            strBuilder.Clear();
            if (props[0] != null)
            {
                strBuilder.Append(props[0]);
                if (props[1] != null)
                {
                    strBuilder.Append('_');
                    strBuilder.Append(props[1]);
                }
            }

            SetTextMaterial(texCmp, strBuilder.ToString());
        }
        else
        {
            SetTextMaterial(texCmp, string.Empty);
        }
    }

    private const string imageGrayMatPath = "UI/Materials/DH_UI_Gray.mat";
    private const string imagePurpleGrayMatPath = "UI/Materials/DH_UI_PureGray.mat";

    public static string GetGrayMatPath(bool gray, bool purple)
    {
        return gray ? purple ? imagePurpleGrayMatPath : imageGrayMatPath : string.Empty;
    }

    private static async UniTaskVoid SetImageGray(Image image, bool gray, bool purple)
    {
        var materialPath = gray ? purple ? imagePurpleGrayMatPath : imageGrayMatPath : string.Empty;
        if (materialPath != string.Empty)
        {
            var material = await AssetsManager.LoadAssetAsync<Material>(materialPath);
            image.material = material;
        }
        else
        {
            image.material = null;
        }
    }

    private static void SetTextMaterial(TextMeshProUGUI texCmp, string materialName)
    {
        Localization.SetTextMaterial(texCmp, materialName);
    }

    public static void SetBtnInteractable(Button btn, bool interactable, bool needGray = true,
        bool recursive = true,
        bool purple = false)
    {
        btn.interactable = interactable;
        if (!interactable && needGray) SetGray(btn.gameObject, true, recursive, purple);

        if (interactable) SetGray(btn.gameObject, false, recursive, purple);
    }

    public static void SetColor(this MaskableGraphic graph, Color color)
    {
        graph.color = color;
    }

    public static void SetColor(this MaskableGraphic graph, string hexColor)
    {
        graph.color = HexColorStrToColor(hexColor);
    }

    public static string ToHexColor(this Color color)
    {
        var str = new StringBuilder();
        str.Append(Mathf.FloorToInt(color.r * 255).ToString("X2"));
        str.Append(Mathf.FloorToInt(color.g * 255).ToString("X2"));
        str.Append(Mathf.FloorToInt(color.b * 255).ToString("X2"));
        return str.ToString();
    }

    public static Color HexColorStrToColor(string HexColorStr)
    {
        float[] value = { 0, 0, 0, 1 };
        if (HexColorStr.Length > 0)
        {
            if (HexColorStr[0] == '#') HexColorStr = HexColorStr.Substring(1);

            var i = 0;
            while (HexColorStr.Length > 1 && i < 4)
            {
                var hexColor = HexColorStr.Substring(0, 2);
                try
                {
                    value[i] = int.Parse(hexColor, NumberStyles.HexNumber) / 255f;
                }
                finally
                {
                    ++i;
                    HexColorStr = HexColorStr.Substring(2);
                }
            }
        }

        return new Color(value[0], value[1], value[2], value[3]);
    }

    private const float designRatio = 720 / 1280f;

    public static Vector2 GetUISize()
    {
        var width = Screen.width;
        var height = Screen.height;
        var ratio = (float)width / height;
        return new Vector2(ratio > designRatio ? 1280 * ratio : 720,
            ratio < designRatio ? 720 / ratio : 1280);
    }

    /// <summary>
    ///     automatic adjust ui position around the target RectTransform inner screen.
    /// </summary>
    /// <param name="adjustRectTrans"></param>
    /// <param name="tarRectTrans">target RectTransform to show around</param>
    /// <param name="adjustParam"></param>
    public static void AdjustUIPosition(this RectTransform adjustRectTrans,
        RectTransform tarRectTrans,
        AdjustParam adjustParam)
    {
        var uiCamera = AppGlobal.Instance.UICamera;

        // Force reset adjust RectTransform anchor and pivot to center.
        var halfVec = Vector2.one / 2;
        adjustRectTrans.anchorMin = halfVec;
        adjustRectTrans.anchorMax = halfVec;
        adjustRectTrans.pivot = halfVec;

        if (adjustParam.rebuildLayoutBeforeAdjust)
            LayoutRebuilder.ForceRebuildLayoutImmediate(adjustRectTrans);

        // Get needed coordination data.
        // target RectTransform:
        var tarCorners = new Vector3[4];
        tarRectTrans.GetWorldCorners(tarCorners);
        for (var i = 0; i < 4; ++i) tarCorners[i] = uiCamera.WorldToScreenPoint(tarCorners[i]);
        var tarScreenCoord = uiCamera.WorldToScreenPoint(tarRectTrans.position);
        var tarSize = new Vector2(tarCorners[2].x - tarCorners[0].x,
            tarCorners[2].y - tarCorners[0].y);
        var tarHalfSize = tarSize / 2;

        // adjust RectTransform:
        var adjustCorners = new Vector3[4];
        adjustRectTrans.GetWorldCorners(adjustCorners);
        for (var i = 0; i < 4; ++i)
            adjustCorners[i] = uiCamera.WorldToScreenPoint(adjustCorners[i]);
        var adjustSize = new Vector2(adjustCorners[2].x - adjustCorners[0].x,
            adjustCorners[2].y - adjustCorners[0].y);
        var adjustHalfSize = adjustSize / 2;

        // screen
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        var screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);
        var screenCorners = new[]
        {
            Vector2.zero,
            new(0, screenHeight),
            new(screenWidth, screenHeight),
            new(screenWidth, 0)
        };

        float adjustCenterY, adjustCenterX;
        if (adjustParam.alignType == AlignType.Vertical)
        {
            var downYCenter = tarScreenCoord.y - tarHalfSize.y - adjustParam.distance -
                              adjustHalfSize.y;
            var upYCenter = tarScreenCoord.y + tarHalfSize.y + adjustParam.distance +
                            adjustHalfSize.y;
            var downYDis = downYCenter - screenCenter.y;
            var upYDis = upYCenter - screenCenter.y;

            // put it up of target RectTransform
            if (adjustParam.forceAlignPosi == AlignPriority.First)
                adjustCenterY = upYCenter;
            // put it down of target RectTransform
            else if (adjustParam.forceAlignPosi == AlignPriority.Last)
                adjustCenterY = downYCenter;
            else
                adjustCenterY = Mathf.Abs(downYDis) <= Mathf.Abs(upYDis) ? downYCenter : upYCenter;

            var leftEdgeX = tarScreenCoord.x - tarHalfSize.x + adjustHalfSize.x;
            var rightEdgeX = tarScreenCoord.x + tarHalfSize.x - adjustHalfSize.x;
            var leftDisX = Mathf.Abs(leftEdgeX - screenCenter.x);
            var rightDisX = Mathf.Abs(rightEdgeX - screenCenter.x);

            // align to left edge of target RectTransform
            if (adjustParam.forceAlignEdge == AlignPriority.First)
                adjustCenterX = leftDisX;
            // align to right edge of target RectTransform
            else if (adjustParam.forceAlignEdge == AlignPriority.Last)
                adjustCenterX = rightEdgeX;
            else
                adjustCenterX = leftDisX <= rightDisX ? leftEdgeX : rightEdgeX;
        }
        else
        {
            var leftXCenter = tarScreenCoord.x - tarHalfSize.x - adjustParam.distance -
                              adjustHalfSize.x;
            var rightXCenter = tarScreenCoord.x + tarHalfSize.x + adjustParam.distance +
                               adjustHalfSize.x;
            var leftXDis = leftXCenter - screenCenter.x;
            var rightXDis = rightXCenter - screenCenter.x;

            // put it left of target RectTransform
            if (adjustParam.forceAlignPosi == AlignPriority.First)
                adjustCenterX = leftXCenter;
            // put it right of target RectTransform
            else if (adjustParam.forceAlignPosi == AlignPriority.Last)
                adjustCenterX = rightXCenter;
            else
                adjustCenterX = Mathf.Abs(leftXDis) <= Mathf.Abs(rightXDis)
                    ? leftXCenter
                    : rightXCenter;

            var downEdgeY = tarScreenCoord.y - tarHalfSize.y + adjustHalfSize.y;
            var upEdgeY = tarScreenCoord.y + tarHalfSize.y - adjustHalfSize.y;
            var downDisY = Mathf.Abs(downEdgeY - screenCenter.y);
            var upDisY = Mathf.Abs(upEdgeY - screenCenter.y);

            // align to down edge of target RectTransform
            if (adjustParam.forceAlignEdge == AlignPriority.First)
                adjustCenterY = downEdgeY;
            // align to up edge of target RectTransform
            else if (adjustParam.forceAlignEdge == AlignPriority.Last)
                adjustCenterY = upEdgeY;
            else
                adjustCenterY = downDisY <= upDisY ? downDisY : upDisY;
        }

        // check adjust RectTransform is out of screen area
        if (adjustSize.x < screenWidth)
        {
            if (adjustCenterX - adjustHalfSize.x < screenCorners[0].x)
                adjustCenterX = screenCorners[0].x + adjustHalfSize.x;
            else if (adjustCenterX + adjustHalfSize.x > screenCorners[2].x)
                adjustCenterX = screenCorners[2].x - adjustHalfSize.x;
        }
        else
        {
            adjustCenterX = screenCenter.x;
            DHLog.Warning(
                "adjust RectTransform size is equal to or larger than screen width! Just put it on center of the screen x axis.",
                adjustRectTrans);
        }

        if (adjustSize.y < screenHeight)
        {
            if (adjustCenterY - adjustHalfSize.y < screenCorners[0].y)
                adjustCenterY = screenCorners[0].y + adjustHalfSize.y;
            else if (adjustCenterY + adjustHalfSize.y > screenCorners[2].y)
                adjustCenterY = screenCorners[2].y - adjustHalfSize.y;
        }
        else
        {
            adjustCenterY = screenCenter.y;
            DHLog.Warning(
                "adjust RectTransform size is equal to or larger than screen height! Just put it on center of the screen y axis.",
                adjustRectTrans);
        }

        var targetPosition =
            uiCamera.ScreenToWorldPoint(new Vector3(adjustCenterX, adjustCenterY,
                adjustRectTrans.transform.position.z));
        adjustRectTrans.transform.position = targetPosition;
    }

    /// <summary>
    ///     转换时间  秒数转换成字符串 两种格式 1d 23m  / 00:00:00
    ///     默认格式显示为 1d 23m
    /// </summary>
    /// <param name="leftTime"> 秒数</param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string ConvertTimeSecondToString(long leftTime, ETimeFormatType type)
    {
        string GetShowStr(int value)
        {
            var ret = "";
            switch (type)
            {
                case ETimeFormatType.Default:
                case ETimeFormatType.DefaultWithUnit:
                case ETimeFormatType.TimeFormatTypeHourMinute:
                case ETimeFormatType.TimeFormatTypeMinuteWithUnit:
                case ETimeFormatType.TimeFormatTypeHourMinuteWithUnit:
                case ETimeFormatType.TimeFormatChampion:
                    ret = value >= 10 ? $"{value}" : $"0{value}";
                    break;
                //case ETimeFormatType.TimeFormatTypeMinuteWithUnit:
                case ETimeFormatType.TimeFormatTypeMail:
                    ret = value >= 10 ? $"{value}" : value.ToString();
                    break;
            }

            return ret;
        }

        var ret = "";
        switch (type)
        {
            case ETimeFormatType.Default:
            {
                if (leftTime <= 0)
                {
                    ret = "00:00:00";
                }
                else
                {
                    var d = Mathf.FloorToInt(leftTime / 86400);
                    var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
                    var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
                    var s = Mathf.FloorToInt(leftTime % 60);
                    var hourStr = GetShowStr(h);
                    var minuteStr = GetShowStr(m);
                    var secondStr = GetShowStr(s);
                    var dayUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime01);
                    ret = d > 0 ? $"{d}{dayUnitStr}" : $"{hourStr}:{minuteStr}:{secondStr}";
                }
            }
                break;
            case ETimeFormatType.DefaultWithUnit:
            {
                var dayUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime01);
                var hourUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime02);
                var minUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime03);
                var senUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime04);
                if (leftTime <= 0)
                {
                    ret = $"00{hourUnitStr}00{minUnitStr}00{senUnitStr}";
                }
                else
                {
                    var d = Mathf.FloorToInt(leftTime / 86400);
                    var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
                    var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
                    var s = Mathf.FloorToInt(leftTime % 60);
                    var dStr = $"{d}{dayUnitStr}";
                    var hourStr = $"{GetShowStr(h)}{hourUnitStr}";
                    var minuteStr = $"{GetShowStr(m)}{minUnitStr}";
                    var secondStr = $"{GetShowStr(s)}{senUnitStr}";
                    ret = d > 0 ? dStr : $"{hourStr}{minuteStr}{secondStr}";
                }
            }
                break;
            case ETimeFormatType.TimeFormatTypeHourMinute:
            {
                if (leftTime <= 0)
                {
                    ret = "00:00";
                }
                else
                {
                    var d = Mathf.FloorToInt(leftTime / 86400);
                    var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
                    var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
                    var s = Mathf.FloorToInt(leftTime % 60);
                    var dayStr = d > 0 ? $"{d}" : "";
                    var hourStr = $"{GetShowStr(h)}";
                    var minuteStr = $"{GetShowStr(m)}";
                    var secondStr = $"{GetShowStr(s)}";
                    ret = d > 0 ? $"{dayStr} {hourStr}" :
                        h > 0 ? $"{hourStr}:{minuteStr}" : $"{minuteStr}:{secondStr}";
                }
            }
                break;
            case ETimeFormatType.TimeFormatTypeHourMinuteWithUnit:
            {
                var dayUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime01);
                var hourUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime02);
                var minUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime03);
                var senUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime04);
                if (leftTime <= 0)
                {
                    ret = $"00{minUnitStr}:00{senUnitStr}";
                }
                else
                {
                    var d = Mathf.FloorToInt(leftTime / 86400);
                    var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
                    var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
                    var s = Mathf.FloorToInt(leftTime % 60);
                    var dayStr = d > 0 ? $"{d}{dayUnitStr}" : "";
                    var hourStr = $"{GetShowStr(h)}{hourUnitStr}";
                    var minuteStr = $"{GetShowStr(m)}{minUnitStr}";
                    var secondStr = $"{GetShowStr(s)}{senUnitStr}";

                    ret = d > 0 ? $"{dayStr} {hourStr}" :
                        h > 0 ? $"{hourStr} {minuteStr}" : $"{minuteStr} {secondStr}";
                }
            }
                break;
            case ETimeFormatType.TimeFormatTypeMail:
            {
                if (leftTime <= 0)
                {
                    ret = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.Mail_tips_4)}";
                }
                else
                {
                    var d = Mathf.FloorToInt(leftTime / 86400);
                    var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
                    var dayStr =
                        $"{LocalizeHelper.GetGlobal(GlobalLanguageId.Mail_tips_2).Replace("{0}", d.ToString())}";
                    var hourStr =
                        $"{LocalizeHelper.GetGlobal(GlobalLanguageId.Mail_tips_3).Replace("{0}", h.ToString())}";
                    var defStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.Mail_tips_4)}";
                    ret = d > 0 ? dayStr : h > 0 ? hourStr : defStr;
                }

                break;
            }
            case ETimeFormatType.TimeFormatMonthCard:
            {
                if (leftTime <= 0) return "";
                var d = Mathf.FloorToInt(leftTime / 86400);
                var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
                var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
                var s = Mathf.FloorToInt(leftTime % 60);
                if (d >= 1) return $"{d}{LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime01)}";

                if (h > 0)
                    return
                        $"{h}{LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime02)}{m}{LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime03)}";

                var mStr = m >= 10 ? $"{m}" : $"0{m}";
                var sStr = s >= 10 ? $"{s}" : $"0{s}";
                return $"{mStr}:{sStr}";
            }
                break;
            case ETimeFormatType.TimeFormatChampion:
            {
                var dayUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime01);
                var hourUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime02);
                var minUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime03);
                var senUnitStr = LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime04);
                if (leftTime <= 0)
                {
                    ret = $"00{hourUnitStr}00{minUnitStr}00{senUnitStr}";
                }
                else
                {
                    var d = Mathf.FloorToInt(leftTime / 86400);
                    var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
                    var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
                    var s = Mathf.FloorToInt(leftTime % 60);
                    var dStr = $"{d}{dayUnitStr}";
                    var hourStr = $"{GetShowStr(h)}{hourUnitStr}";
                    var minuteStr = $"{GetShowStr(m)}{minUnitStr}";
                    var secondStr = $"{GetShowStr(s)}{senUnitStr}";
                    ret = d > 0
                        ? $"{dStr}{hourStr}{minuteStr}"
                        : $"{hourStr}{minuteStr}{secondStr}";
                }
            }
                break;
            case ETimeFormatType.TimeFormatTypeMinuteWithUnit:
            {
                if (leftTime <= 0)
                {
                    ret = "00:00";
                }
                else
                {
                    var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
                    var s = Mathf.FloorToInt(leftTime % 60);
                    var minuteStr = GetShowStr(m);
                    var secondStr = GetShowStr(s);
                    ret = $"{minuteStr}:{secondStr}";
                }
            }
                break;
        }

        return ret;
    }

    /// <summary>
    ///     转换时间  秒数转换成字符串 两种格式 1d 23m  / 00:00:00
    ///     默认格式显示为 1d 23m
    /// </summary>
    /// <param name="leftTime"> 秒数</param>
    /// <param name="len"> 显示格式 2 表示 1d 23m 否则显示  00:00:00 格式 </param>
    /// <returns></returns>
    public static string ConvertTimeSecondToString(long leftTime)
    {
        if (leftTime < 0) leftTime = 0;
        var d = Mathf.FloorToInt(leftTime / 86400);
        var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
        var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
        var s = Mathf.FloorToInt(leftTime % 60);
        string time;
        var dayStr = d >= 10 ? $"{d}" : $"0{d}";
        var hourStr = h >= 10 ? $"{h}" : $"0{h}";
        var minStr = m >= 10 ? $"{m}" : $"0{m}";
        var sendStr = s >= 10 ? $"{s}" : $"0{s}";
        if (d > 0)
        {
            dayStr = $"{dayStr}{LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime01)}";
            hourStr = $"{hourStr}{LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime02)}";
            minStr = $"{minStr}{LocalizeHelper.GetGlobal(GlobalLanguageId.TfTime03)}";
            time = $"{dayStr}{hourStr}{minStr}";
        }
        else
        {
            time = $"{hourStr}:{minStr}:{sendStr}";
        }

        return time;
    }

    public static int[] ConvertTimeSecondToArray(long leftTime)
    {
        var d = Mathf.FloorToInt(leftTime / 86400);
        var h = Mathf.FloorToInt(leftTime % 86400 / 3600);
        var m = Mathf.FloorToInt(leftTime % 86400 % 3600 / 60);
        var s = Mathf.FloorToInt(leftTime % 60);
        var list = new List<int>();
        list.Add(d);
        list.Add(h);
        list.Add(m);
        list.Add(s);
        return list.ToArray();
    }


    /// <summary>
    ///     计算位置偏移量
    /// </summary>
    /// <param name="size"></param>
    /// <param name="total"></param>
    /// <param name="curIndex"></param>
    /// <param name="isX"></param>
    /// <returns></returns>
    public static float GetItemPosOffsetByInfo(Size size, int total, int curIndex, bool isX = true)
    {
        var offset = size.Width;
        if (!isX) offset = size.Height;

        var totalW = offset * (total - 1);
        var ret = -totalW / 2 + offset * curIndex;

        return ret;
    }


    public static string GetRewardName(Reward reward)
    {
        return GetRewardName(RewardToResourceData(reward));
    }

    public static string GetRewardName(ResourceData reward)
    {
        var ret = "";
        switch (reward.Type)
        {
            case (int)RewardType.Head:
            case (int)RewardType.MagicCredit:
            case (int)RewardType.Exp:
            case (int)RewardType.Lives:
            case (int)RewardType.Secret:
            case (int)RewardType.Item:
                
                var itemCfg = ConfigCenter.ItemLanguageCfgColl.GetDataById(reward.Id);
                return itemCfg?.Name;
            case (int)RewardType.HeroEquip:
                return ClothesManager.Instance.GetClothesItemName(reward.Id);
            case (int)RewardType.Equip:
                return EquipManager.Instance.GetEquipName(reward.Id);
            case (int)RewardType.Skill: return EquipManager.Instance.GetEquipSkillName(reward.Id);
            

        }

        return ret;
    }

    public static string GetRewardsIconPath(RewardType type, int id)
    {
        switch (type)
        {
            case RewardType.MagicCredit:
            case RewardType.Secret:
            case RewardType.Item:
            {
                var cfg = ConfigCenter.ItemCfgColl.GetDataById(id);
                if (cfg == null)
                {
                    DHLog.Error($"GetRewardsIconPath 中Item 为空 {id}");
                    return UIHelper.NoneImagePath();
                }

                if (cfg.Type == 2)
                {
                    // 装备的小图
                    return EquipManager.Instance.GetIconPath(cfg.TypeValue);
                }

                return DataCenter.itemsData.GetItemIconPathById(id);
            }
            case RewardType.Exp: return DataCenter.itemsData.GetItemIconPathById((int)GameConst.ItemIdCode.Exp);
            case RewardType.Lives:
                return DataCenter.itemsData.GetItemIconPathById((int)GameConst.ItemIdCode.EnergyDrink);
            // case RewardType.DailyPoints: return DataCenter.itemsData.GetItemIconPathById((int)GameConst.ItemIdCode.DailyPoint);
            case RewardType.Head: return DataCenter.charcaterData.GetPlayerHeadImgPath(id);
            case RewardType.Skill: return EquipManager.Instance.GetEquipSkillIcon(id);
            case RewardType.Equip: return EquipManager.Instance.GetModelIconPath(id);
            case RewardType.HeroEquip: return ClothesManager.Instance.GetClothesIconPath(id);
            default:
            {
                DHLog.Error($"GetRewardsBgPath  没有处理的类型{type}，请及时处理");
                return null;
            }
        }
    }

    public static string GetRewardsBgPath(RewardType type, int id, HeroEquipData data = null)
    {
        switch (type)
        {
            case RewardType.MagicCredit: 
            case RewardType.Item: return DataCenter.itemsData.GetItemBgPathById(id);
            case RewardType.Exp:
                return DataCenter.itemsData.GetItemBgPathById((int)GameConst.ItemIdCode.Exp);
            // case RewardType.DailyPoints:
            //     return DataCenter.itemsData.GetItemBgPathById((int)GameConst.ItemIdCode.DailyPoint);
            case RewardType.WeeklyPoints:
                return DataCenter.itemsData.GetItemBgPathById((int)GameConst.ItemIdCode
                    .WeeklyPoint);
            case RewardType.Lives: return DataCenter.itemsData.GetItemBgPathById(id);
            case RewardType.Head: return DataCenter.charcaterData.GetPlayerHeadBgPath(id);
            case RewardType.Equip: return EquipManager.Instance.GetBgPathByEquipModelId(id);
            case RewardType.Skill: return NoneImagePath();
            case RewardType.HeroEquip: return ClothesManager.Instance.GetClothesQuaBgPath(id, data);
            case RewardType.Secret: return DataCenter.itemsData.GetItemBgPathById((int)GameConst.ItemIdCode.SecretScore);
            default:
            {
                DHLog.Error($"GetRewardsBgPath 没有处理的类型{type}，请及时处理");
                return "item[item_panel_1]";
            }
        }
    }

    public static int GetRewardsQuality(RewardType type, int id)
    {
        switch (type)
        {
            case RewardType.MagicCredit:
            case RewardType.Item: return DataCenter.itemsData.GetItemQuality(id);
            case RewardType.Exp:
                return DataCenter.itemsData.GetItemQuality((int)GameConst.ItemIdCode.Exp);
            case RewardType.WeeklyPoints:
                return DataCenter.itemsData.GetItemQuality((int)GameConst.ItemIdCode
                    .WeeklyPoint);
            case RewardType.Lives: return DataCenter.itemsData.GetItemQuality(id);
            case RewardType.Head: return DataCenter.charcaterData.GetPlayerHeadQuality(id);
            case RewardType.Equip: return EquipManager.Instance.GetQualityByEquipModelId(id);
            case RewardType.Skill: return 0;
            case RewardType.Secret: return DataCenter.itemsData.GetItemQuality((int)GameConst.ItemIdCode.SecretScore);
            default:
            {
                DHLog.Error($"GetRewardsQuality 没有处理的类型{type}，请及时处理");
                return 0;
            }
        }
    }

    public static string GetRewardsIconEffectPath(RewardType type, int id)
    {
        switch (type)
        {
            case RewardType.Head: return DataCenter.charcaterData.GetPlayerHeadFrameEffectPath(id);
            default:
            {
                DHLog.Warning($" GetRewardsIconEffectPath 没有处理的类型{type}-id=={id}，请及时处理");
                return null;
            }
        }
    }


    public static string GetRewardBgPath(ResourceData reward)
    {
        return GetRewardBgPath((RewardType)reward.Type, reward.Id, reward.HeroEquip);
    }

    public static string GetRewardBgPath(Reward reward)
    {
        return GetRewardBgPath(reward.Type, reward.Id);
    }

    public static string GetRewardBgPath(RewardType type, int id, HeroEquipData heroEquipData = null)
    {
        return GetRewardsBgPath(type, id, heroEquipData);
    }

    /// <summary>
    ///     设置两个scrollview 的事件透传
    /// </summary>
    /// <param name="coreScrollView">内层scrollview</param>
    /// <param name="outerScrollView">外层scrollview</param>
    public static void ScrollViewSwallowTouch(ScrollRect coreScrollView, ScrollRect outerScrollView)
    {
        var listener = coreScrollView.GetComponent<DragEventTriggerListener>();
        if (listener)
        {
            listener.BeginDragHandle = outerScrollView.OnBeginDrag;
            listener.OnDragHandle = outerScrollView.OnDrag;
            listener.EndDragHandle = outerScrollView.OnEndDrag;
        }
    }

    /// <summary>
    ///     根据类型 解析数值 返回字符串
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="showType">显示类型</param>
    /// <param name="type">是否万分比</param>
    /// <returns></returns>
    public static string ParseValueToStringByType(float value, GameConst.ENumType showType, int type = 0,bool isIntger=false,int decimals = 1)
    {
        float valueData = type == 1 ? value / 10000f : value;
        switch (showType)
        {
            case GameConst.ENumType.NumberTypeValue:
                return isIntger ? Mathf.FloorToInt(valueData).ToString() : valueData.ToString();
            case GameConst.ENumType.PercentTypeValue:
                string str;
                switch (decimals)
                {
                    case 2 : str = (Mathf.FloorToInt(valueData * 10000) / 100f).ToString("0.##") + "%";break;
                    case 3 : str = (Mathf.FloorToInt(valueData * 100000) / 1000f).ToString("0.###") + "%";break;
                    default: str = (Mathf.FloorToInt(valueData * 1000) / 10f).ToString("0.#") + "%";break;
                }
                return str;
            case GameConst.ENumType.Time:
                return $"{valueData / 1000} s";
        }

        return "";
    }

    public static float GetRoundResult(string name,float value,bool round=true)
    {
        
        var cfg = ConfigCenter.AttributesCfgColl.GetDataByName(name);
        if (cfg != null && cfg.Type == 1)
        {
            return value;
        }
        return round?(int)Math.Round(value):Mathf.FloorToInt(value);
    }

    /// <summary>
    ///     获取指定长度的字符串
    /// </summary>
    /// <param name="str"></param>
    /// <param name="count"> 指定长度 不传就是默认 15个字节</param>
    /// <returns></returns>
    public static string GetStringByLength(string str, int count = 15)
    {
        var len = str.Length;

        string retStr;

        if (count < len)
            retStr = $"{str.Substring(0, count)}...";
        else
            retStr = str;

        return retStr;
    }

    public static string GetRewardsIconPath(ResourceData resourceData)
    {
        return resourceData == null
            ? NoneImagePath()
            : GetRewardsIconPath((RewardType)resourceData.Type, resourceData.Id);
    }

    public static string GetRewardsIconPath(Reward reward)
    {
        return reward == null ? NoneImagePath() : GetRewardsIconPath(reward.Type, reward.Id);
    }

    public static string GetRewardsIconPath(int type, int id)
    {
        return GetRewardsIconPath((RewardType)type, id);
    }

    public static void OpenCommonItemTipsVIew(CommonItemTipsData data, bool isOpenOffset = false)
    {
        GetRewardInfo(data.Reward, out string nameStr, out string descStr);
        CommonItemTipsViewModel tempVm =
            new CommonItemTipsViewModel(nameStr, descStr, IsCommonItemTipsPosLeft(data.Reward));
        tempVm.Position = data.Pos;
        tempVm.OffSet = data.Offset;
        tempVm.IsOpenOffset = isOpenOffset;
        tempVm.CloseCallback = data.Callback;
        UIManager.Instance.OpenDialog<CommonItemTipsView>(tempVm).Forget();
    }

    public static void OpenCommonItemTipsVIew(string desc, string titleName, Vector3 position)
    {
        CommonItemTipsViewModel tempVm = new CommonItemTipsViewModel(titleName, desc);
        tempVm.Position = position;
        UIManager.Instance.OpenDialog<CommonItemTipsView>(tempVm).Forget();
    }

    public static void GetRewardInfo(Reward reward, out string nameStr, out string descStr)
    {
        nameStr = string.Empty;
        descStr = string.Empty;
        if (reward == null) return;
        switch (reward.Type)
        {
            case RewardType.MagicCredit:
            case RewardType.Item:
            case RewardType.Lives:
            case RewardType.DailyPoints:
            case RewardType.WeeklyPoints:
            case RewardType.Exp:
            case RewardType.Secret:
            {
                var itemLanguage = ConfigCenter.ItemLanguageCfgColl.GetDataById(reward.Id);
                if (itemLanguage != null)
                {
                    nameStr = itemLanguage.Name;
                    descStr = itemLanguage.Dec;
                }
                else
                {
                    DHLog.Error($"no language cfg  type is {reward.Type}  id is {reward.Id}");
                }
            }
                break;
            case RewardType.Skill:
            {
                var skillLanguage = ConfigCenter.EquipSkillLanguageCfgColl.GetDataById(reward.Id);
                var skillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(reward.Id);
                if (skillLanguage != null)
                {
                    nameStr = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);

                    descStr = skillLanguage.Dec;
                    if (skillCfg.Value != null)
                    {
                        descStr = string.Format(skillLanguage.Dec,skillCfg.Value.ToArray());
                    }
                }
                else
                {
                    DHLog.Error($"no language cfg  type is {reward.Type}  id is {reward.Id}");
                }
            }
                break;
            case RewardType.HeroEquip:
                nameStr = ClothesManager.Instance.GetClothesItemName(reward.Id);
                descStr = ClothesManager.Instance.GetClothesItemDesc(reward.Id);
                break;
            case  RewardType.Head:
                var headLanguageCfg = ConfigCenter.ProPictureLanguageCfgColl.GetDataById(reward.Id);
                if (headLanguageCfg != null)
                {
                    descStr =  headLanguageCfg.Description;
                }

                var headCfg = ConfigCenter.ProPictureCfgColl.GetDataById(reward.Id);
                if (headCfg != null)
                {
                    if (headCfg.Type == 1)
                    {
                        nameStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.ProPicture02);
                    }
                    else
                    {
                        nameStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.ProPicture03);
                    }

                }
                break;
            case RewardType.Equip:
            {
                int equipId = reward.Id;
                var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(reward.Id);
                if (equipModelCfg != null)
                {
                    equipId = equipModelCfg.Equip;
                }
                nameStr = EquipManager.Instance.GetEquipName(equipId);
                descStr = EquipManager.Instance.GetTargetDesc(equipId);
                break;
            }
            default:
                DHLog.Error($"没处理的类型，请及时处理 {reward.Type}");
                break;
        }
    }

    public static bool IsCommonItemTipsPosLeft(Reward reward)
    {
        var isLeft = true;
        switch (reward.Type)
        {
            case RewardType.MagicCredit:
            case RewardType.Item:
            case RewardType.Lives:
            case RewardType.DailyPoints:
            case RewardType.WeeklyPoints:
            case RewardType.Skin:
            case RewardType.Exp:
            {
                // var itemLanguage = ConfigCenter.ItemLanguageCfgColl.GetDataById(reward.Id);
                // if (itemLanguage != null)
                // {
                //     var itemData = ConfigCenter.ItemCfgColl.GetDataById(reward.Id);
                //     if (itemData != null && (itemData.Type == 10 || itemData.Type == 14 ||
                //                              itemData.Type == 15))
                //         isLeft = true;
                // }
            }
                break;
        }

        return isLeft;
    }

    public static void OpenCommonTipsForMonster(int id, Vector3 pos, Vector3 offset,
        bool isOpenOffset = false, Action callback = null)
    {
    }

    public static void OpenCommonTips(string title, string desc, Vector3 pos, Vector3 offset,
        bool isOpenOffset = false, Action callback = null)
    {
        var tempVm = new CommonItemTipsViewModel(title, desc);
        tempVm.Position = pos;
        tempVm.OffSet = offset;
        tempVm.IsOpenOffset = isOpenOffset;
        tempVm.CloseCallback = callback;
        UIManager.Instance.OpenDialog<CommonItemTipsView>(tempVm).Forget();
    }

    public static void OpenCommonSkillTips(string title, string desc, Vector3 pos, Vector3 offset,
        bool isOpenOffset = false, Action callback = null)
    {
        var tempVm = new CommonSkillTipsModel(title, desc);
        tempVm.Position = pos;
        tempVm.OffSet = offset;
        tempVm.IsOpenOffset = isOpenOffset;
        tempVm.CloseCallback = callback;
        UIManager.Instance.OpenDialog<CommonSkillTipsView>(tempVm).Forget();
    }

    public static void OpenCommonRule(string title, string desc)
    {
        var tempRule = new CommonRuleViewModel(title, desc);
        UIManager.Instance.OpenDialog<CommonRuleView>(tempRule).Forget();
    }

    public static void OpenCommonRuleProbability(string ruleStr,int jackpotId,int superJackpotId=0)
    {
            List<LimitCellRatioData> limitList = new();
            List<LimitCellRatioData> normalList = new();
            
            var jackpotCfg = ConfigCenter.JackpotCfgColl.GetDataById(jackpotId);
            if (jackpotCfg != null)
            {
                var weight = GetJackpotWeight(jackpotId);
                if (jackpotCfg is { RandomReward: { Count: > 0 } })
                {
                    for (int i = 0; i < jackpotCfg.RandomReward.Count; ++i)
                    {
                        var item = jackpotCfg.RandomReward[i];
                        var tempReward = new Reward(item.Type, item.Id, item.Count);
                        var ratioStr = ((float)item.Weight / weight * 100).ToString("0.##")+"%";
                        LimitCellRatioData tempData = new(tempReward, ratioStr, normalList.Count);
                        normalList.Add(tempData);
                    }
                }
            }

            if (superJackpotId != 0)
            {
                var superJackpotCfg = ConfigCenter.JackpotCfgColl.GetDataById(superJackpotId);
                if (superJackpotCfg != null)
                {
                    var weight = GetJackpotWeight(superJackpotId);
                    if (superJackpotCfg is { RandomReward: { Count: > 0 } })
                    {
                        foreach (var item in superJackpotCfg.RandomReward)
                        {
                            var tempReward = new Reward(item.Type, item.Id, item.Count);
                            var ratioStr = ((float)item.Weight / weight).ToString("0.##")+"%";
                            LimitCellRatioData tempData = new(tempReward, ratioStr, normalList.Count);
                            limitList.Add(tempData);
                        }
                    }
                }
            }
            
            ActivityRuleAndRatioData curData = new(ruleStr, limitList,normalList);
            ActivityRuleAndRatioViewModel tempVm = new(curData);
            UIManager.Instance.OpenDialog<ActivityRuleAndRatioView>(tempVm).Forget();
    }


    public static void ViewModelBaseOnDisposes<T>(ObservableList<T> list)
    {
        foreach (var t in list)
            ViewModelBaseOnDispose(t as ViewModelBase);
    }


    public static void ViewModelBaseOnDispose<T>(ObservableList<T> list)
    {
        for (var i = 0; i < list.Count; i++) ViewModelBaseOnDispose(list[i] as ViewModelBase);
    }

    public static void ViewModelBaseOnDispose(ViewModelBase item)
    {
        item?.Dispose();
    }

    /// <summary>
    ///     统一处理错误码的信息
    /// </summary>
    /// <param name="errorCode"></param>
    /// <returns></returns>
    public static string GetNetErrorMessage(int errorCode)
    {
        var cfg = ConfigCenter.ErrMsgCfgColl.GetDataById(errorCode);
        if (cfg != null)
        {
            DHLog.Debug($"error code is {errorCode}  des is {cfg.Msg}");
            var languageCfg = ConfigCenter.GlobalLanguageCfgColl.GetDataById(cfg.LanguageId);
            var retStr = languageCfg != null
                ? languageCfg.Name
                : LocalizeHelper.GetGlobal(GlobalLanguageId.NetworkConnectError);
            return retStr;
        }
        else
        {
            var retStr = $"Error Code {errorCode}";
            return retStr;
        }


    }

    //计算倒计时
    public static long GetResidueCD(long Residue, long Start)
    {
        var now = DateTimeOffset.Now.ToUnixTimeSeconds();
        var cd = Residue - (now - Start);
        return cd >= 0 ? cd : 0;
    }

    public static Reward RandomRewardToReward(RandomReward reward)
    {
        return new Reward(reward.Type, reward.Id, reward.Count);
    }

    public static Reward ResourceDataToReward(ResourceData reward)
    {
        return new Reward((RewardType)reward.Type, reward.Id, reward.Count);
    }

    public static List<Resource> RewardsToResources(List<Reward> resources)
    {
        List<Resource> rewards = new();
        foreach (var item in resources)
        {
            var resource = new Resource();
            resource.Type = (int)item.Type;
            resource.Id = item.Id;
            resource.Count = item.Count;
            rewards.Add(resource);
        }

        return rewards;
    }

    public static Resource RewardToResource(Reward reward)
    {
        var tempRes = new Resource();
        tempRes.Type = (int)reward.Type;
        tempRes.Count = reward.Count;
        tempRes.Id = reward.Id;
        return tempRes;
    }

    public static ResourceData RewardToResourceData(Reward reward)
    {
        var tempRes = new ResourceData();
        tempRes.Type = (int)reward.Type;
        tempRes.Count = reward.Count;
        tempRes.Id = reward.Id;
        return tempRes;
    }

    public static bool CheckSpecialReward(List<Resource> list)
    {
        for (var i = 0; i < list.Count; i++)
            if (CheckSpecialReward(list[i]))
                return true;
        return false;
    }

    public static bool CheckSpecialReward(Resource resource)
    {
        return resource.Type == (int)RewardType.Item &&
               resource.Id == (int)GameConst.ItemIdCode.QuickGet;
    }

    /// <summary>
    ///     获取武器技能描述
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    public static string GetWeaponSkillDesc(int skillId)
    {
        return string.Empty;
    }


    public static string NoneImagePath()
    {
        return "common[common_alpha]";
    }

    public static void ShowChild(GameObject parent, int star)
    {
        if (parent == null) return;
        var show = false;
        for (var i = 0; i < parent.transform.childCount; i++)
        {
            show = i < star;
            if (parent.transform.GetChild(i).gameObject.activeSelf != show)
                parent.transform.GetChild(i).gameObject.SetActive(show);
        }
    }

    public static string GetIsEnoughDesc(long own, long all)
    {
        if (own < all) return $"<color=#FF0000>{own}</color>/{all}";
        return $"<color=#02fb24>{own}</color>/{all}";
    }

    public static void PlayEffect(ParticleSystem effect)
    {
        if (effect == null) return;
        effect.gameObject.SetActive(true);
        if (effect.isPlaying)
        {
            effect.Stop();
            effect.Simulate(0f);
        }

        effect.Play();
    }

    public static void StopEffect(ParticleSystem effect)
    {
        if (effect == null) return;
        effect.gameObject.SetActive(false);
        effect.Stop();
    }
    
    /// <summary>
    ///     将游戏场景里面的坐标转换到UI坐标
    /// </summary>
    /// <param name="worldPos"></param>
    /// <param name="parentRectTransform"></param>
    /// <param name="sceneCamera"></param>
    /// <returns></returns>
    public static Vector3 GetScenePosToUINodeLocalPosition(Vector3 worldPos,
        RectTransform parentRectTransform, Camera sceneCamera)
    {
        var screenPoint = sceneCamera.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPoint,
            AppGlobal.Instance.UICamera, out var uiPosition);

        return uiPosition;
    }

    //检查当前时间是否跨天
    public static bool CheckTimeNowDay(long lastTime)
    {
        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(lastTime).DateTime;
        return dateTime.Year != DateTimeOffset.Now.Year ||
               dateTime.Month != DateTimeOffset.Now.Month || dateTime.Day != DateTimeOffset.Now.Day;
    }

    //Update中调整时间间隔刷新
    public static bool CalculateTime(ref float time, float interval = 1f)
    {
        time += Time.deltaTime;
        if (time < interval) return false;
        time -= interval;
        return true;
    }

    /// <summary>
    ///     获取品质的颜色
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetQuaColor(int Qua,bool isBase = true)
    {
        if (isBase)
        {
            if (Qua - 1 >= 0 && Qua - 1 < QuaColor.Count) return QuaColor[Qua - 1];
            return QuaColor[0]; 
        }
        else
        {
            if (Qua - 1 >= 0 && Qua - 1 < QuaColor1.Count) return QuaColor1[Qua - 1];
            return QuaColor1[0]; 
        }
    }

    public static string GetQuaColor(string desc, int qua,bool isBase = true)
    {
        return $"<color={GetQuaColor(qua,isBase)}>{desc}</color>";
    }

    /// <summary>
    ///     获取品质名字
    /// </summary>
    /// <param name="qua"></param>
    /// <returns></returns>
    public static string GetQuaName(int qua)
    {
        var quaCfg = ConfigCenter.QuaLanguageCfgColl.GetDataById(qua);
        return quaCfg?.Name ?? string.Empty;
    }

    //获取偏移量
    public static float GetOffsetX(Vector3 pos, float width = 420f)
    {
        var screenPosition = AppGlobal.Instance.UICamera.WorldToScreenPoint(pos);
        var offsetx = 0.0f;
        var leftCurx = screenPosition.x - width * 0.5f;
        var rightCurx = screenPosition.x + width * 0.5f;
        var widthX = Screen.width;
        if (leftCurx < 0 + 20) //左边超框
            offsetx = Math.Abs(leftCurx - 20);
        else if (rightCurx > widthX - 20) //右边超框
            offsetx = -Math.Abs(rightCurx - widthX + 20);
        return offsetx;
    }

    public static string GetTimeDesc(long endCd, string desc)
    {
        if (desc != null) return DHUtility.Format(desc, ConvertTimeSecondToString(endCd));

        return ConvertTimeSecondToString(endCd);
    }

    public static string GetTimeDesc(long endCd, string desc, ETimeFormatType type)
    {
        if (desc != null) return DHUtility.Format(desc, ConvertTimeSecondToString(endCd, type));

        return ConvertTimeSecondToString(endCd, type);
    }

    // 将小数转换为百分数的方法
    public static string DecimalToPercentage(float decimalNumber)
    {
        // 确保小数在0到1之间
        decimalNumber = Mathf.Clamp01(decimalNumber);
        // 转换为百分比，并保留两位小数
        var percentage = (decimalNumber * 100).ToString("F2") + "%";
        return percentage;
    }

    public static void SortList<T>(List<T> sortList, Func<T, T, bool> condition)
    {
        var n = sortList.Count;
        for (var i = 1; i < n; ++i)
        for (var j = 0; j < n - i; ++j)
            if (condition(sortList[j], sortList[j + 1]))
                (sortList[j], sortList[j + 1]) = (sortList[j + 1], sortList[j]);
        // var temp = sortList[j];
        // sortList[j] = sortList[j+1];
        // sortList[j+1] = temp;
    }

    public static void SortList<T>(ObservableList<T> sortList, Func<T, T, bool> condition)
    {
        var n = sortList.Count;
        for (var i = 1; i < n; ++i)
        for (var j = 0; j < n - i; ++j)
            if (condition(sortList[j], sortList[j + 1]))
                (sortList[j], sortList[j + 1]) = (sortList[j + 1], sortList[j]);
    }

    public static bool IsActivityReward(Reward reward)
    {
        return IsActivityReward(reward.Type, reward.Id);
    }

    public static bool IsActivityReward(Resource reward)
    {
        return IsActivityReward((RewardType)reward.Type, reward.Id);
    }

    public static bool IsActivityReward(RewardType type, int id)
    {
        return false;
    }

    //获得限制item 最大可选择次数
    public static int GetMaxBuyNum(int limit, Reward reward)
    {
        return (int)GetMaxBuyNum(limit, reward.Type, reward.Id, reward.Count);
    }

    /// <summary>
    ///     目前只处理了 Item
    /// </summary>
    /// <param name="type"></param>
    /// <param name="price"></param>
    public static long GetMaxBuyNum(int limit, RewardType type, int id, long price)
    {
        long buyNum = limit;
        if (type == RewardType.Item)
        {
            var resourceData = DataCenter.itemsData.ResourceDatas[id];
            if (resourceData != null)
            {
                var buyMaxNum = resourceData.Count;
                var num = buyMaxNum / price;
                if (buyNum > num) buyNum = num;

                if (buyNum == 0) buyNum = 1;
            }
        }

        return buyNum;
    }

    public static void SetChildActive(Transform parent, int level)
    {
        for (var i = 0; i < parent.childCount; i++)
            parent.GetChild(i).gameObject.SetActive(i < level);
    }

    public static void SetImageAlpha(Image image, float alpha)
    {
        // 获取当前颜色
        var color = image.color;

        // 设置透明度
        color.a = alpha;

        // 应用新的颜色
        image.color = color;
    }

    /// <summary>
    ///     设置图片的剪影
    /// </summary>
    /// <param name="image"></param>
    /// <param name="isTrue"></param>
    public static async UniTask<Material> GetSilhouetteMaterial(string materialPath)
    {
        var material = await AssetsManager.LoadAssetAsync<Material>(materialPath);
        return material;
    }

    public static void OpenItemTips(string title, string desc, Tuple<Vector3, Vector3> info, Action callback = null)
    {
        CommonItemTipsViewModel tempVm = new CommonItemTipsViewModel(title, desc);
        tempVm.Position = info.Item1;
        tempVm.OffSet = info.Item2;
        tempVm.IsOpenOffset = false;
        tempVm.CloseCallback = callback;
        UIManager.Instance.OpenDialog<CommonItemTipsView>(tempVm).Forget();
    }

    public static void OpenItemTips(ResourceData data, Tuple<Vector3, Vector3> info)
    {
        if (data == null) return;

        if (data.Type == (int)RewardType.Item)
        {
            var cfg = ConfigCenter.ItemCfgColl.GetDataById(data.Id);
            if (cfg != null)
            {
                if (cfg.Type == (int)GameConst.ItemType.EquipFragment)
                {
                    UIManager.Instance.OpenDialog<EquipInfoView>(new EquipInfoViewModel(cfg.TypeValue, true)).Forget();
                    return;
                }

                if (cfg.Type == (int)GameConst.ItemType.EquipFragmentRandom)
                {
                    UIManager.Instance
                        .OpenDialog<ShopRewardInfoView>(new ShopRewardInfoViewModel(ResourceDataToReward(data)))
                        .Forget();
                    return;
                }
            }
        }
        else if (data.Type == (int)RewardType.HeroEquip)
        {
            if (data.HeroEquip!=null && !data.HeroEquip.IsNull())
            {
                UIManager.Instance.OpenDialog<ClothesInfoView>(new ClothesInfoViewModel(data.HeroEquip)).Forget();
            }
            return;
        }

        var commonItemTipsData = new CommonItemTipsData(data, info.Item1, info.Item2);
        OpenCommonItemTipsVIew(commonItemTipsData);
    }

    public static Reward GetDiamond(long count)
    {
        return new Reward(RewardType.Item, (int)GameConst.ItemIdCode.Stone, count);
    }

    public static Reward GetGold(long count)
    {
        return new Reward(RewardType.Item, (int)GameConst.ItemIdCode.Money, count);
    }


    public static async void ShowRewardAds(Action callBack,Action sendAdUploadCb = null)
    {
        if (DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.AdRreeReward) ||
            DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever))
        {
            callBack?.Invoke();
            ReportedManager.Instance.SendAdUpload(false,sendAdUploadCb:sendAdUploadCb).Forget();
        }
        else
        {
            if (DataCenter.itemsData.GetItemCount((int)GameConst.ItemIdCode.AdFreeVouche) > 0)
            {
                var req = new ReqItemUseFreeAd();
                var result = await GameNetworkManager.Instance.SendAsync<RspItemUseFreeAd>(req);
                NetHelper.CheckNetErrorMessage(result.rsp, true, () =>
                {
                    Lodash.DealRewards(result.rsp.Cost.ToList(), false);
                    callBack?.Invoke();
                    ReportedManager.Instance.SendAdUpload(false,sendAdUploadCb:sendAdUploadCb).Forget();
                });
            }
            else
            {
                AdController.Instance.ShowRewardAds("", () => { callBack?.Invoke(); },sendAdUploadCb);
            }
        }
    }

    /// <summary>
    /// 检查资源是否足够
    /// </summary>
    /// <param name="reward"></param>
    /// <returns></returns>
    public static bool CheckRewardIsEnough(Reward reward, bool isTips = false, int cnt = 1, bool isJump = false)
    {
        if (reward == null) return false;
        bool isEnough = Lodash.CheckRewardIsEnough(reward, cnt);
        if (isTips && !isEnough)
        {
            //AudioManager.Instance.PlayWrongTips();
            if (reward.Type == RewardType.Item)
            {
                ToastManager.ShowLanguage(GlobalLanguageId.Shop_tips12, GetRewardName(reward));
            }
            else
            {
                ToastManager.ShowLanguage(GlobalLanguageId.Equip_15);
            }
        }

        if (!isEnough && isJump)
        {
            var vm = new JumpViewModel(reward);
            UIManager.Instance.OpenDialog<JumpView>(vm).Forget();
        }

        return isEnough;
    }

    public static bool CheckRewardIsEnough(List<Reward> reward, bool isTips = false, int cnt = 1, bool isJump = false)
    {
        bool isEnough = true;
        for (int i = 0; i < reward.Count; i++)
        {
            if (!CheckRewardIsEnough(reward[i], isTips, cnt))
            {
                isEnough = false;
                if (isJump)
                {
                    var vm = new JumpViewModel(reward[i]);
                    UIManager.Instance.OpenDialog<JumpView>(vm).Forget();
                }

                break;
            }
        }

        return isEnough;
    }

    public static bool CheckRewardIsEnough(ResourceData data, bool isTips = false, int cnt = 1)
    {
        return CheckRewardIsEnough(ResourceDataToReward(data), isTips);
    }

    public static void OpenCommonRewardView(List<Resource> rewards, Action callBack = null,
        TitleShowType showType = TitleShowType.Base,List<Resource> superRewardIdList=null,bool mergeSucceed = false)
    {
        var rewardVM = new ShopDrawRewardViewModel(rewards, 0, null, showType,false,superRewardIdList);
        rewardVM.CloseAction = callBack;
        rewardVM.mergeSucceedState = mergeSucceed;
        UIManager.Instance.OpenDialog<ShopDrawRewardView>(rewardVM, true).Forget();
    }

    public static void OpenCommonRewardView(List<Reward> rewards, Action callBack = null,
        TitleShowType showType = TitleShowType.Base,bool mergeSucceed = false)
    {
        OpenCommonRewardView(RewardsToResources(rewards), callBack, showType,mergeSucceed:mergeSucceed);
    }

    public static void OpenCommonRewardView(List<HeroEquip> rewards, Action callBack = null,
        TitleShowType showType = TitleShowType.Base,bool mergeSucceed = false)
    {
        OpenCommonRewardView(HeroEquipToResources(rewards), callBack, showType,mergeSucceed:mergeSucceed);
    }

    public static List<Resource> MergeLists(List<Resource> item1, List<Resource> item2)
    {
        var mergedList = new List<Resource>();
        foreach (var item in item1.Concat(item2).GroupBy(x => new { x.Type, x.Id,x.HeroEquip }))
        {
            var sumNum = item.Sum(x => x.Count);
            var res = new Resource();
            res.Type = item.Key.Type;
            res.Id = item.Key.Id;
            res.Count = sumNum;
            res.HeroEquip = item.Key.HeroEquip;
            mergedList.Add(res);
        }

        return mergedList;
    }

    public static List<Reward> MergeLists(List<Reward> item1, List<Reward> item2)
    {
        var mergedList = new List<Reward>();
        foreach (var item in item1.Concat(item2).GroupBy(x => new { x.Type, x.Id }))
        {
            var sumNum = item.Sum(x => x.Count);
            var res = new Reward(item.Key.Type, item.Key.Id, sumNum);
            mergedList.Add(res);
        }

        return mergedList;
    }

    public static List<Reward> GetItemJackpotList(Reward reward)
    {
        List<Reward> list = new();
        if (reward.Type == RewardType.Item)
        {
            var cfg = ConfigCenter.ItemCfgColl.GetDataById(reward.Id);
            if (cfg.Type == (int)GameConst.ItemType.EquipFragmentRandom && cfg is { JackpotId: { Count: > 0 } })
            {
                var jockCfg = ConfigCenter.JackpotCfgColl.GetDataById(cfg.JackpotId[0]);
                if (jockCfg is { RandomReward: { Count: > 0 } })
                {
                    foreach (var item in jockCfg.RandomReward)
                    {
                        list.Add(UIHelper.RandomRewardToReward(item));
                    }
                }
            }
        }

        return list;
    }

    public static bool GetItemIsLock(int type, int id)
    {
        if (type == (int)RewardType.Item)
        {
            var cfg = ConfigCenter.ItemCfgColl.GetDataById(id);
            if (cfg != null)
            {
                if (cfg.Type == (int)GameConst.ItemType.EquipFragment)
                {
                    var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(cfg.TypeValue);
                    if (equipCfg != null)
                    {
                        return !DataCenter.mainStageData.IsPassChapter(equipCfg.Unlock);
                    }
                }
            }
        }
        else
        {
            return true;
        }

        return false;
    }

    public static bool GetItemIsLock(ResourceData reward)
    {
        return GetItemIsLock(reward.Type, reward.Id);
    }

    public static bool GetItemIsLock(Reward reward)
    {
        return GetItemIsLock((int)reward.Type, reward.Id);
    }


    public static string GetRefreshDayTime(long time)
    {
        return ServerTime.Instance.SecondsDHms(Math.Max(ServerTime.Instance.RemainTime(time), 0));
    }

    public static void SortReward(List<Resource> rewards)
    {
        SortList(rewards, (itemA, itemB) => RewardSortValue(itemB) > RewardSortValue(itemA));
    }

    public static int RewardSortValue(Resource resource)
    {
        int value = 0;
        if (resource.Type == (int)RewardType.Item)
        {
            var cfg = ConfigCenter.ItemCfgColl.GetDataById(resource.Id);
            if (cfg != null)
            {
                value += (99 - cfg.Type) * 1000000;

                value += cfg.Quality * 10000;

                value += 9999 - cfg.Id;
            }
        }
        else
        {
            //DHLog.Error($"奖励排序 存在未设置奖励优先级的物品 需要添加 Type={Rewa}");
        }

        return value;
    }

    public static bool IsDiamond(Reward reward)
    {
        if (reward == null) return false;
        return reward.Type == RewardType.Item && reward.Id == (int)GameConst.ItemIdCode.Stone;
    }

    public static void GetDescLinkInfo(ref string desc, SkillPreviewSkillType type, List<int> param)
    {
        if (param == null) return;
        switch (type)
        {
            case SkillPreviewSkillType.EquipModel:
            {

                for (int i = 0; i < param.Count; i++)
                {
                    var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(param[i]);
                    var modelCfgL = ConfigCenter.EquipModelLanguageCfgColl.GetDataById(param[i]);
                    if (modelCfg != null && desc.Contains("{equip" + (i + 1) + "}"))
                    {
                        var modelAttr = ConfigCenter.EquipAttrCfgColl.GetDataById(modelCfg.AttrType);
                        desc = desc.Replace("{equip" + (i + 1) + "}",
                            $"<color=#{modelAttr.EquipAttrColor}><u><link={(int)type}_{param[i]}>{modelCfgL.WeaponName}</link></u></color>");
                    }
                }

                break;
            }
            case SkillPreviewSkillType.HeroEquip:
            {

                for (int i = 0; i < param.Count; i++)
                {
                    var buffCfgL = ConfigCenter.HeroEquipBuffLanguageCfgColl.GetDataById(param[i]);
                    if (buffCfgL != null && desc.Contains("{buff" + (i + 1) + "}"))
                    {
                        desc = desc.Replace("{buff" + (i + 1) + "}",
                            $"<color=#358801><u><link={(int)type}_{param[i]}>{buffCfgL.Name}</link></u></color>");
                    }
                }

                break;
            }
        }
    }

    public static void OnClickDescLink(string clickInfo, Vector3 pos)
    {
        var infolist = clickInfo.Split("_");
        if (infolist.Length != 2) return;
        {
            int infoType;
            int infoId;
            int.TryParse(infolist[0], out infoType);
            int.TryParse(infolist[1], out infoId);
            switch ((SkillPreviewSkillType)infoType)
            {
                case SkillPreviewSkillType.EquipModel:
                {
                    var modelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(infoId);
                    if (modelCfg != null)
                    {
                        UIManager.Instance
                            .OpenDialog<EquipStateView>(new EquipStateViewModel(modelCfg.Equip, modelCfg.Id)).Forget();
                    }

                    break;
                }
                case SkillPreviewSkillType.HeroEquip:
                {
                    var buffCfgL = ConfigCenter.HeroEquipBuffLanguageCfgColl.GetDataById(infoId);
                    if (buffCfgL != null)
                    {
                        OpenCommonItemTipsVIew(buffCfgL.Dec, buffCfgL.Name, pos);
                    }

                    break;
                }
            }
        }
    }

    public static long ShowNowTime(string pos, long lastTime = 0)
    {
        long timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        DHLog.Debug($"currentUTC   {pos}  :{timeStamp - lastTime}");
        return timeStamp;
    }

    public static void ClearAndDispose<T>(this ObservableList<T> target)
    {
        ViewModelBaseOnDisposes(target);
        target.Clear();
    }

    public static string InsertLineBreaks(string input, int maxWidth)
    {
        string output = "";
        int charCount = 0;

        for (int i = 0; i < input.Length; i++)
        {
            output += input[i];
            charCount++;

            if (charCount >= maxWidth && i < input.Length - 1)
            {
                output += "\n";
                charCount = 0;
            }
        }

        return output;
    }

    #region 排行榜金色昵称判断
    public static bool IsGoldName(int VipStatus)
    {
        bool isGolden = false;
        if (VipStatus != 0)
        {
            if (VipStatus == 1)
            {
                var PermanentCardCfg= DataCenter.monthCardData.PermanentCardCfg;
                for (int i = 0; i < PermanentCardCfg.EffectId.Count; i++)
                {
                    if (PermanentCardCfg.EffectId[i] == (int)MonthCardEffectType.GoldenNickname)
                    {
                        isGolden = true;
                        break;
                    } 
                }
            }else if (VipStatus == 2)
            {
                var PlusCardCfg= DataCenter.monthCardData.PlusCardCfg;
                for (int i = 0; i < PlusCardCfg.EffectId.Count; i++)
                {
                    if (PlusCardCfg.EffectId[i] == (int)MonthCardEffectType.GoldenNickname)
                    {
                        isGolden = true;
                        break;
                    } 
                }
            }
            else if (VipStatus == 3)
            {
                isGolden = true;
            }
        }

        return isGolden;
    }
    #endregion
    public static List<Resource> HeroEquipToResources(List<HeroEquip> heroEquips)
    {
        List<Resource> rewards = new();
        foreach (var item in heroEquips)
        {
            var resource = new Resource();
            resource.Type = (int)RewardType.HeroEquip;
            resource.Id = item.Id;
            resource.Count = 1;
            resource.HeroEquip = new HeroEquip()
            {
                Id = item.Id,
                Uid = item.Uid,
                Lv = item.Lv,
                QuaId = item.QuaId
            };
            rewards.Add(resource);
        }

        return rewards;
    }

    public static HeroEquipData HeroEquipToHeroEquipData(HeroEquip heroEquip)
    {
        if (heroEquip.IsNull()) return null;
        return new HeroEquipData(heroEquip);
    }

    public static HeroEquip HeroEquipDataToHeroEquip(HeroEquipData heroEquipData)
    {
        if (heroEquipData==null || heroEquipData.IsNull()) return null;
        return new HeroEquip
        {
            Id = heroEquipData.Id,
            Uid = heroEquipData.Uid,
            QuaId = heroEquipData.QuaId,
            Lv = heroEquipData.Lv,
        };
    }

    public static HeroEquip HeroEquipDigestToHeroEquip(HeroEquipDigest heroEquipDigest)
    {
        if (heroEquipDigest == null || heroEquipDigest.IsNull()) return null;
        return new HeroEquip()
        {
            Id = heroEquipDigest.Id,
            Uid = 0,
            QuaId = heroEquipDigest.QuaId,
            Lv = heroEquipDigest.Lv,
        };
    }

    public static List<Reward> OverlayReward(List<Reward> rewards)
    {
        List<ResourceData> overlayRewards = new();
        for (int i = 0; i < rewards.Count; i++)
        {
            bool isOverlay = false;
            int count = overlayRewards.Count;
            for (int j = 0; j < count; j++)
            {
                if ((int)rewards[i].Type == overlayRewards[j].Type && rewards[i].Id == overlayRewards[j].Id)
                {
                    isOverlay = true;
                    overlayRewards[j].Count += rewards[i].Count;
                }
            }

            if (!isOverlay)
            {
                overlayRewards.Add(UIHelper.RewardToResourceData(rewards[i]));
            }
        }

        return overlayRewards.Select(item => new Reward((RewardType)item.Type, item.Id, item.Count)).ToList();
    }

    public static bool IsNull(this HeroEquip data)
    {
        return data == null || (data.Id == 0 && data.QuaId == 0 && data.Uid == 0 && data.Lv == 0);
    }
    
    public static void SetNull(this HeroEquip data)
    {
        if (data != null)
        {
            data.Uid = 0;
            data.Id = 0;
            data.QuaId = 0;
            data.Lv = 0;
        }
    }
    
    public static bool IsNull(this HeroEquipData data)
    {
        return data == null || (data.Id == 0 && data.QuaId == 0 && data.Uid == 0 && data.Lv == 0);
    }
    public static bool IsNull(this HeroEquipDigest data)
    {
        return data == null || (data.Id == 0 && data.QuaId == 0 && data.Lv == 0);
    }
    
    public static void SetNull(this HeroEquipData data)
    {
        if (data != null)
        {
            data.Uid = 0;
            data.Id = 0;
            data.QuaId = 0;
            data.Lv = 0;
        }
    }
    
    public static async UniTaskVoid FlyChip(GameObject moveParent,string iconPath,Vector3 startPos,Vector3 endPos,Action endCallback)
    {
        if(moveParent==null) return;
        GameObject obj = null;
        for (int i = 0; i < moveParent.transform.childCount; i++)
        {
            GameObject chilidObj = moveParent.transform.GetChild(i).gameObject;
            if (!chilidObj.activeSelf)
            {
                obj = chilidObj;
                break;
            }
        }
        if (obj == null) return;
        DhImage itemImg = obj.transform.GetComponent<DhImage>();
        itemImg.sprite = AssetsManager.LoadSpriteSync(iconPath);
        itemImg.gameObject.SetActive(true);
        obj.transform.position = startPos;
        await UniTask.Delay(100);
        obj.transform.DOMove(endPos, 0.1f);
        await UniTask.Delay(200);
        endCallback?.Invoke();
        if(obj!=null)
            obj.gameObject.SetActive(false);
    }
    
    public static List<StageRewardCfg> GetAllStageRewardList(ActivityStageType type)
    {
        var list = ConfigCenter.StageRewardCfgColl.DataItems;
        return list.ToList().FindAll(item=>item.Type ==(int)type);
    }

    public static Reward GetJackpotRewardByIndex(int jackpotId,int index)
    {
        var cfg = ConfigCenter.JackpotCfgColl.GetDataById(jackpotId);
        if (cfg != null)
        {
            if (cfg.RandomReward!=null && cfg.RandomReward.Count > index)
            {
                return UIHelper.RandomRewardToReward(cfg.RandomReward[index]);
            }
        }

        return null;
    }

    public static int GetJackpotWeight(int jackpotId)
    {
        int weight = 0;
        var cfg = ConfigCenter.JackpotCfgColl.GetDataById(jackpotId);
        if (cfg != null)
        {
            if (cfg.RandomReward!=null && cfg.RandomReward.Count > 0)
            {
                foreach (var item in cfg.RandomReward)
                {
                    weight += item.Weight;
                }
            }
        }
        return weight;
    }

    public static CommonTopViewModel GetTopModel(params GameConst.ItemIdCode[] args)
    {
        return new CommonTopViewModel(args.ToList());
    }

    //获取Define常量
    public static int GetDefinesInt(DefineCfgId id)
    {
        var defineCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)id);
        return defineCfg?.Content[0] ?? 0;
    }
    
    
    #region 英雄立绘初始化箱子
    /// <summary>
    /// 英雄立绘初始化箱子
    /// </summary>
    /// <param name="heroTr"></param>
    /// <param name="heroEquipId"></param>
    public static void InitHeroEquipBox(Transform heroTr,int heroEquipId,float localScale = 1f)
    {
        var boxTr = heroTr.Find("Box");
        if (boxTr != null)
        {
            boxTr.transform.localScale = Vector3.one*localScale;
            var BoxImage = boxTr.GetComponent<DhImage>();
            var iconPath = "";
            if (heroEquipId == 0)
            {
                iconPath = "icon[heroEquipPicture_common_01]";
            }
            else
            {
                var equipId = heroEquipId;
                var heroEquipCfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(equipId).Picture;
                iconPath = string.IsNullOrEmpty(heroEquipCfg) ? "icon[heroEquipPicture_common_01]" : $"icon[{heroEquipCfg}]";
            }
            AssetsManager.LoadAssetAsyncWithCallback(iconPath,
                (Sprite iconSprite) =>
                {
                    BoxImage.sprite = iconSprite;
                });
        }
    }

    public static string GetRoleTips(string tipsName)
    {
        return $"SaveInfo_{DataCenter.charcaterData.Digest.RoleId}_{tipsName}";
    }
    
    

    #endregion
}