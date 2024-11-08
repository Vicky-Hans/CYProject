using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DH.Game.UI.Control
{
    public class TabControl : MonoBehaviour
    {
        [Serializable]
        public class TabItem
        {
            public RectTransform tabItem;
            public TextMeshProUGUI textObj;
            public Vector2 normalSize;
            public Vector2 selectSize;
            public Button selectButton;
            public GameObject[] views;
            public bool defaultItem;

            internal bool selected;
        }
        
        [Serializable]
        public class SelectEvent : UnityEvent<int> { }

        public TabItem[] tabItems;
        public SelectEvent selectEvent;
        private TabItem current;

        private void Start()
        {
            int index = 0;
            foreach (var item in tabItems)
            {
                item.selectButton.onClick.AddListener(()=>OnSelectItem(item));
                if (item.defaultItem)
                {
                    current = item;
                    item.selected = true;
                    foreach (var view in item.views)
                    {
                        view.SetActive(true);
                    } 
                    item.textObj.gameObject.SetActive(true);
                    item.tabItem.sizeDelta = item.selectSize;
                    selectEvent.Invoke(index);
                }
                else
                {
                    item.selected = false;
                    foreach (var view in item.views)
                    {
                        view.SetActive(false);
                    }

                    item.tabItem.sizeDelta = item.normalSize;
                    item.textObj.gameObject.SetActive(false);
                }

                index++;
            }
        }

        private void OnSelectItem(TabItem item)
        {
            current.selected = false;
            foreach (var view in current.views)
            {
                view.SetActive(false);
            }
            current.tabItem.sizeDelta = item.normalSize;
            current.textObj.gameObject.SetActive(false);

            current = item;
            current.selected = true;
            foreach (var view in current.views)
            {
                view.SetActive(true);
            }
            current.tabItem.sizeDelta = item.selectSize;
            current.textObj.gameObject.SetActive(true);
            
            selectEvent.Invoke(Array.IndexOf(tabItems,item));
        }
    }
}