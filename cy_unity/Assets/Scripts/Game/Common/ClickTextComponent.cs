using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace DH.Game.UIViews
{
    public class ClickTextComponent : MonoBehaviour, IPointerClickHandler
    {

        public Action<string> OnClickCallback;
        public TextMeshProUGUI text;
        private Button button;
        public bool isClickPos=false;
        public Action<string,Vector3> ClickCallback { get; set; }
        private void Awake()
        {
            button = GetComponent<Button>();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, eventData.pressEventCamera);
            if(linkIndex < 0 || linkIndex >= text.textInfo.linkInfo.Length) return;
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            // DHLog.Debug($"linkIndex == {linkInfo.GetLinkID()}  str = {linkInfo.GetLinkText()}");
            if (isClickPos)
            {
                Vector3 worldPos = AppGlobal.Instance.UICamera.ScreenToWorldPoint(eventData.position);
                ClickCallback?.Invoke(linkInfo.GetLinkID(),worldPos);
            }
            else
            {
                ClickCallback?.Invoke(linkInfo.GetLinkID(),gameObject.transform.position);
            }



        }
    }
}