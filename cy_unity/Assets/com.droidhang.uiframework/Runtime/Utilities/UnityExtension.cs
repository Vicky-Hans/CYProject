using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Binding
{
    public static class UnityExtension
    {
        private static readonly Stack<Transform>
            stack = new Stack<Transform>(50); // GetComponentInChildren查找不到disable的节点

        public static bool IsNull(this UnityEngine.Object o) // 或者名字叫IsDestroyed等等
        {
            return !o;
        }

        public static Component GetComponentInParent(this Component component, string type)
        {
            Transform curTrans = component.transform;
            while (curTrans != null)
            {
                var targetCom = curTrans.GetComponent(type);
                if (targetCom != null)
                {
                    return targetCom;
                }

                curTrans = curTrans.transform.parent;
            }

            return null;
        }

        public static UnityEngine.Object GetAutoInjectNode(this Transform trans, string fieldName, string path,
            string fieldType)
        {
            Transform nodeTrans = null;

            if (!string.IsNullOrEmpty(path))
            {
                nodeTrans = path.Equals(".") ? trans : trans.Find(path);
            }
            else
            {
                stack.Clear();
                stack.Push(trans);
                while (stack.Count > 0)
                {
                    var node = stack.Pop();
                    if (node.gameObject.name.Equals(fieldName))
                    {
                        nodeTrans = node;
                        break;
                    }

                    foreach (Transform i in node)
                    {
                        stack.Push(i);
                    }
                }

                stack.Clear();
            }

            if (!nodeTrans)
            {
                Debug.LogWarning($"Can't find {fieldName} with path {path}", trans);
            }

            UnityEngine.Object comp = null;

            if (nodeTrans != null)
            {
                if (fieldType.Equals("UnityEngine.Transform")) //UnityEngine.Canvas
                {
                    comp = nodeTrans.transform;
                }
                else if (fieldType.Equals("UnityEngine.RectTransform"))
                {
                    comp = nodeTrans.transform as RectTransform;
                }
                else if (fieldType.Equals("UnityEngine.GameObject"))
                {
                    comp = nodeTrans.gameObject;
                }
                else if (fieldType.Equals("UnityEngine.Camera"))
                {
                    comp = nodeTrans.GetComponent<Camera>();
                }
                else
                {
                    comp = nodeTrans.GetComponent(fieldType);
                }
            }

            return comp;
        }

        public static void Stretch(this Transform transform)
        {
            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                return;
            }

            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// RectTransform sizeDelta拓展函数
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="x">分量x</param>
        /// <param name="y">分量y</param>
        public static void SetSizeDelta(this RectTransform rectTransform, float x, float y)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = x;
            sizeDelta.y = y;
            rectTransform.sizeDelta = sizeDelta;
        }

        /// <summary>
        /// RectTransform anchoredPosition拓展函数
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="x">分量x</param>
        /// <param name="y">分量y</param>
        public static void SetAnchoredPosition(this RectTransform rectTransform, float x, float y)
        {
            Vector2 anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.x = x;
            anchoredPosition.y = y;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// RectTransform localPosition拓展函数
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="x">分量x</param>
        /// <param name="y">分量y</param>
        /// <param name="z">分量z</param>
        public static void SetLocalPosition(this RectTransform rectTransform, float x, float y, float z)
        {
            Vector3 localPosition = rectTransform.localPosition;
            localPosition.x = x;
            localPosition.y = y;
            localPosition.z = z;
            rectTransform.localPosition = localPosition;
        }

        /// <summary>
        /// RectTransform localScale拓展函数
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="x">分量x</param>
        /// <param name="y">分量y</param>
        /// <param name="z">分量z</param>
        public static void SetLocalScale(this RectTransform rectTransform, float x, float y, float z)
        {
            Vector3 localScale = rectTransform.localScale;
            localScale.x = x;
            localScale.y = y;
            localScale.z = z;
            rectTransform.localScale = localScale;
        }

        /// <summary>
        /// RectTransform pivot拓展函数
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="x">分量x</param>
        /// <param name="y">分量y</param>
        public static void SetPivot(this RectTransform rectTransform, float x, float y)
        {
            Vector2 pivot = rectTransform.pivot;
            pivot.x = x;
            pivot.y = y;
            rectTransform.pivot = pivot;
        }

        /// <summary>
        /// RectTransform anchorMin拓展函数
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="x">分量x</param>
        /// <param name="y">分量y</param>
        public static void SetAnchorMin(this RectTransform rectTransform, float x, float y)
        {
            Vector2 anchorMin = rectTransform.anchorMin;
            anchorMin.x = x;
            anchorMin.y = y;
            rectTransform.anchorMin = anchorMin;
        }

        /// <summary>
        /// RectTransform anchorMax拓展函数
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="x">分量x</param>
        /// <param name="y">分量y</param>
        public static void SetAnchorMax(this RectTransform rectTransform, float x, float y)
        {
            Vector2 anchorMax = rectTransform.anchorMax;
            anchorMax.x = x;
            anchorMax.y = y;
            rectTransform.anchorMax = anchorMax;
        }

    }
}