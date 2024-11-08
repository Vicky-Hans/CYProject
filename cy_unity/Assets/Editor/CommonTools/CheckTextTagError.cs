using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace DH.Tool
{
    public class CheckTextTagErrorWindow : EditorWindow
    {

        private CheckTextTagError tempCheck; 
        private Vector2 scrollPos;
        private Vector2 scrollPos1;
        private string tempDesc;
        private bool isPassSet;
        // 将名为"My Window"的菜单项添加到 Window 菜单
        [MenuItem("DH Tools/检查Text异常Tag")]
        public static void ShowWindow()
        {
            
            GetWindow(typeof(CheckTextTagErrorWindow));
        }

        private void OnEnable()
        {
            if (tempCheck == null)
            {
                tempCheck = new(true); 
            }
            tempCheck.GetJsonToFile();
            tempCheck.CheckTMPComponentTag();
        }

        private void OnDestroy()
        {
            tempCheck.SaveJsonToFile();
            tempCheck = null;
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("列表",GUILayout.Height(50)))
            {
                isPassSet = false;
                tempCheck.CheckTMPComponentTag();
            }
            
            if (GUILayout.Button("白名单",GUILayout.Height(50)))
            {
                isPassSet = true;
                tempCheck.CheckTMPComponentTag();
            }
            EditorGUILayout.EndHorizontal();
            if (!isPassSet)
            {
                if (GUILayout.Button("刷新",GUILayout.Height(50)))
                {
                    tempCheck.CheckTMPComponentTag();
                }

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                foreach (var item in tempCheck.ItemList)
                {
                    if (!tempCheck.CheckNoneChild(item.Value)) continue;
                    EditorGUILayout.BeginHorizontal();
          
                    EditorGUILayout.ObjectField(string.Empty,item.Key, typeof(GameObject), true,GUILayout.Width(200),GUILayout.Height(30));
                
                    
                    GUILayout.Label($"未标记数量 ：{item.Value.Count}", GUILayout.Width(150),GUILayout.Height(30));
                    EditorGUILayout.EndHorizontal();
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        name = tempCheck.GetparentPath(item.Value[i].transform);
                        if (tempCheck.IsWhiteList(name)) continue;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(50);
             
                        if (GUILayout.Button("选中节点",GUILayout.Width(60),GUILayout.Height(30)))
                        {
                            int pos1 = i;
                            SelectGame(item.Value[pos1],name);
                        }
                        
                        if (GUILayout.Button("白名单",GUILayout.Width(50),GUILayout.Height(30)))
                        {
                            tempCheck.AddWhiteList(name);
                            tempCheck.SaveJsonToFile();
                        }
                
                        GUILayout.Label($"ChildiName:{name}", GUILayout.Width(800),GUILayout.Height(30));
               
                        EditorGUILayout.EndHorizontal();
                    }
                }
     
                EditorGUILayout.EndScrollView();
            }
            else
            {
                scrollPos1 = EditorGUILayout.BeginScrollView(scrollPos1);
                for (int i = 0; i < tempCheck.WhiteList.Count; i++)
                {
                    var key = tempCheck.WhiteList.Keys.ToList()[i];
                    if (tempCheck.WhiteList[key])
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Del",GUILayout.Width(50),GUILayout.Height(30)))
                        {
                            tempCheck.DelWhiteList(key);
                            tempCheck.SaveJsonToFile();
                        }
                        GUILayout.Label($"白名单：{key}", GUILayout.Width(800),GUILayout.Height(30));
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }


        }

        private void SelectGame(GameObject gameObject,string tempNamename)
        {
            int index = tempNamename.IndexOf("/", StringComparison.Ordinal);
            var name1 = tempNamename.Substring( 0,index+1);
            tempNamename = tempNamename.Replace(name1, "");
            if (gameObject==null || Selection.activeGameObject==null)
            {
                return;
            }

            if (gameObject != null)
            {
                var parent = tempCheck.GetTopParent(Selection.activeGameObject);
                var text = parent?.Find(tempNamename)?.GetComponent<TextMeshProUGUI>();
                Debug.Log($"选中节点名字{Selection.activeGameObject.transform.name} 父节点名字 {parent?.name}   子物体路径：{tempNamename}");

                if (text!=null && !text.CompareTag("FontText"))
                {
                    Selection.activeGameObject = text.gameObject;
                    return;
                }
                var text1 = parent?.Find(tempNamename)?.GetComponent<TextMeshPro>();
                if (text1!=null && !text1.CompareTag("FontText"))
                {
                    Selection.activeGameObject = text1.gameObject;
                    return;
                }
            }
        }
    }

    public class CheckTextTagError
    {
        private static readonly string WorkFolder = Environment.CurrentDirectory;
        private static readonly string ProjectPath = Application.dataPath;

        public Dictionary<GameObject, List<GameObject>> ItemList = new();
        
        public Dictionary<string,bool> WhiteList = new();

        private bool isWindow;

        public CheckTextTagError(bool isWindow)
        {
            this.isWindow = isWindow;
        }

        public void CheckTMPComponentTag()
        {
            ItemList.Clear();
            string path = $"{WorkFolder}/Assets/GameAssets/Prefabs/UI"; //预制体路径
            List<string> prefabPaths = GetPrefabPaths(path);
            foreach (var prefabPath in prefabPaths)
            {
                string assetPath = "Assets" + prefabPath.Substring(ProjectPath.Length);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    var name = CheckTMPTag(prefab);
                    if(name!=null && name.Count>0) ItemList.Add(prefab,name);
                }
            }
        }

        public List<string> GetPrefabPaths(string path)
        {
            List<string> prefabPaths = new List<string>();
            string[] files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                prefabPaths.Add(file);
            }
            return prefabPaths;
        }

        public List<GameObject> CheckTMPTag(GameObject prefab)
        {
            List<GameObject> tagErrorList = new();
            TextMeshProUGUI[] tmpUGUIComponents =
                prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmpUGUIComponent in tmpUGUIComponents)
            {
                if (!tmpUGUIComponent.CompareTag("FontText"))
                {
                    if (isWindow)
                    {
                        tagErrorList.Add(tmpUGUIComponent.gameObject);
                    }
                    else
                    {
                        var name = GetparentPath(tmpUGUIComponent.gameObject.transform);
                        if (IsWhiteList(name)) continue;
                        Debug.LogError($"文本没加 tag 请检查 预制体 {prefab.name} 的 节点 {tmpUGUIComponent.gameObject.name}");
                    }
                }
            }

            TextMeshPro[] tmpComponents = prefab.GetComponentsInChildren<TextMeshPro>(true);
            foreach (var tmpComponent in tmpComponents)
            {
                if (!tmpComponent.CompareTag("FontText"))
                {
                    if (isWindow)
                    {
                        tagErrorList.Add(tmpComponent.gameObject);
                    }
                    else
                    {
                        var name = GetparentPath(tmpComponent.gameObject.transform);
                        if (IsWhiteList(name)) continue;
                        Debug.LogError($"文本没加 tag 请检查 预制体 {prefab.name} 的 节点 {tmpComponent.gameObject.name}");
                    }
                }
            }

            return tagErrorList;
        }
        
        //获得子节点完整路径
        public void GetPath(Transform currentTransform, StringBuilder path=null)
        {
            // 如果当前不是根节点，则添加名称并递归父节点
            if (currentTransform.parent != null)
            {
                GetPath(currentTransform.parent, path);
                path?.Append("/").Append(currentTransform.name);

            }
            else // 如果当前是根节点，则只添加自己的名称
            {
                path?.Append(currentTransform.name);
            }
        }
        //获得子节点完整路径
        public Transform GetTopParent(GameObject currentTransform)
        {
            // 如果当前不是根节点，则添加名称并递归父节点
            if (currentTransform.transform.parent != null && currentTransform.transform.parent.name !=  "Canvas (Environment)")
            {
                return GetTopParent(currentTransform.transform.parent.gameObject);
            }
            else // 如果当前是根节点，则只添加自己的名称
            {
                return currentTransform.transform;
            }
        }

        public string GetparentPath(Transform currentTransform)
        {
            StringBuilder str = new StringBuilder();
            GetPath(currentTransform, str);
            return str.ToString();
        }

        public void AddWhiteList(string path)
        {
            if (!WhiteList.ContainsKey(path))
            {
                WhiteList.Add(path,true);
            }
            else
            {
                WhiteList[path] = true;
            }
        }
        
        public void DelWhiteList(string path)
        {
            if (WhiteList.ContainsKey(path))
            {
                WhiteList[path] = false;
            }
        }
        
        public bool IsWhiteList(string path)
        {
            if (WhiteList.ContainsKey(path))
            {
                return WhiteList[path];
            }

            return  false;
        }

        public bool CheckNoneChild(List<GameObject> childList)
        {
            for (int i = 0; i < childList.Count; i++)
            {
                string name = GetparentPath(childList[i].transform);
                if (!WhiteList.ContainsKey(name) || (WhiteList.ContainsKey(name) && !WhiteList[name]))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 保存json为文件
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="filePath"></param>
        public void SaveJsonToFile()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in WhiteList)
            {
                if (item.Value)
                {
                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.Append(item.Key);
                    }
                    else
                    {
                        stringBuilder.Append(",").Append(item.Key);//可优化为使用换行隔开，可在保存的信息时避免冲突shi
                    }
                }
            }

            string json = stringBuilder.ToString();
            File.WriteAllText(WorkFolder+"/Tools/TagWhiteList.json",json);
        }

        public void GetJsonToFile()
        {
            WhiteList.Clear();
            if(!File.Exists(WorkFolder+"/Tools/TagWhiteList.json")) return;
            string json = File.ReadAllText(WorkFolder+"/Tools/TagWhiteList.json");
            var NameList = json.Split(',');
            for (int i = 0; i < NameList.Length; i++)
            {
                if (NameList[i] != "")
                {
                    WhiteList.Add(NameList[i],true);
                }
            }
        }
        
    }

}