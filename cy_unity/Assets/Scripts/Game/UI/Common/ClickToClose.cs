using System;
using DH.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game
{
    public partial class ClickToClose : MonoBehaviour
    {
        /// <summary>
        /// 是否点击关闭
        /// </summary>
        [SerializeField]
        public bool closeToClose = false;
        /// <summary>
        /// 设置透明度来达到是否显示背景
        /// </summary>
        [SerializeField]
        public int bgAlpha = 0;


        public Action CloseCallback { get; set; }
        private Button closeBtn;
        private Image bgImg;
        

        private void Awake()
        {
            closeBtn = gameObject.GetComponent<Button>();
            if (closeToClose)
            {
                closeBtn.onClick.AddListener(OnClickToClose);
            }
            bgImg = gameObject.GetComponent<Image>();
            Color defaultColor = bgImg.color;
            defaultColor.a = bgAlpha/255f;
            bgImg.SetColor(defaultColor);
        }
        
        public void OnClickToClose()
        {
            var view = transform.parent.GetComponent<BaseView>();
            if (view)
            {
                CloseCallback?.Invoke();
                UIManager.Instance.CloseDialog(view.GetType());
                
            }
        }

        
        /// <summary>
        ///  获取/设置a值【0-1
        /// </summary>
        public float alpha
        {
            get
            {
                bgImg = gameObject.GetComponent<Image>();
                
                return bgImg.color.a;
            }
            set
            {
                bgImg = gameObject.GetComponent<Image>();
                Color defaultColor = bgImg.color;
                defaultColor.a = value;
                bgImg.SetColor(defaultColor);

            }
        }
    }
}