using DH.Data;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

namespace Editor.CommonTools
{
    public class HierarchyContextMenuExtension
    {
        [MenuItem("GameObject/SetButtonTextColor/GreenBtnTextColor", false, 10)]
        private static void CustomMenuItem()
        {
            SetSelectColor(DhHexColor.GreenBtnTextColor);
        }
    
        [MenuItem("GameObject/SetButtonTextColor/GrayBtnTextColor", false, 10)]
        private static void CustomMenuItem1()
        {
            SetSelectColor(DhHexColor.GrayBtnTextColor);
        }
    
        [MenuItem("GameObject/SetButtonTextColor/BlueBtnGoOnTextColor", false, 10)]
        private static void CustomMenuItem2()
        {
            SetSelectColor(DhHexColor.BlueBtnGoOnTextColor);
        }

    
        [MenuItem("GameObject/SetButtonTextColor/YellowBtnTextColor", false, 10)]
        private static void CustomMenuItem3()
        {
            SetSelectColor(DhHexColor.YellowBtnTextColor);
        }
    
        [MenuItem("GameObject/SetButtonTextColor/RedBtnTextColor", false, 10)]
        private static void CustomMenuItem4()
        {
            SetSelectColor(DhHexColor.RedBtnTextColor);
        }
    
        [MenuItem("GameObject/SetButtonTextColor/PurpleBtnTextColor", false, 10)]
        private static void CustomMenuItem5()
        {
            SetSelectColor(DhHexColor.PurpleBtnTextColor);
        }

        [MenuItem("GameObject/SetButtonTextColor/BlueBtnConfirmTextColor", false, 9)]
        private static void CustomMenuItem6()
        {
            SetSelectColor(DhHexColor.BlueBtnConfirmTextColor);
        }
        private static void SetSelectColor(string color)
        {
            var selectList = Selection.gameObjects;
            if (selectList is { Length: > 0 })
            {
                foreach (var item in selectList)
                {
                    var text = item.GetComponent<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.color = UIHelper.HexColorStrToColor(color);
                    }
                    var text1 = item.GetComponent<TextMeshPro>();
                    if (text1 != null)
                    {
                        text1.color = UIHelper.HexColorStrToColor(color);
                    }
                
                    var text2 = item.GetComponent<Text>();
                    if (text2 != null)
                    {
                        text2.color = UIHelper.HexColorStrToColor(color);
                    }
                }
            }
        }
    }
}