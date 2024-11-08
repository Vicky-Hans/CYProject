using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class ChangeTextColor  : EditorWindow
{
    private static readonly string WorkFolder = Environment.CurrentDirectory;
    private static readonly string ProjectPath = Application.dataPath;
    
    private Dictionary<GameObject, List<GameObject>> ItemList = new();
    
    private Vector2 scrollPos;
    private Vector2 scrollPos1;
    private string tempDesc;
    private bool IsPassSet;
    // 将名为"My Window"的菜单项添加到 Window 菜单
    [MenuItem("DH Tools/白色替换")]
    public static void ShowWindow()
    {
        GetWindow(typeof(ChangeTextColor));
    }
    
    [MenuItem("DH Tools/清除缓存")]
    public static void DeleteAll()
    {
        DHUnityUtil.PlayerPrefs.DeleteAll();
    }
    
    private void OnEnable()
    {
        CheckTMPComponentTag();
    }

    private void OnDestroy()
    {
        
    }
    
    public void ChangeTMPComponentColor()
    {
        ItemList.Clear();
        string path = $"{WorkFolder}/Assets/GameAssets/Prefabs"; //预制体路径
        List<string> prefabPaths = GetPrefabPaths(path);
        // 设置进度条的初始值
        float progress = 0f;
        EditorUtility.DisplayProgressBar("Long Operation", "Processing...", progress);
        
        foreach (var prefabPath in prefabPaths)
        {
            string assetPath = "Assets" + prefabPath.Substring(ProjectPath.Length);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                // 更新进度条的值
                progress++;
                EditorUtility.DisplayProgressBar("替换全部白色字体颜色", $"正在替换 {prefab.name} 字体颜色", progress/prefabPaths.Count);
                ChangedTMPColor(prefab);
            }
        }
        // 完成操作后关闭进度条
        EditorUtility.ClearProgressBar();
    }
    
    private void ChangedTMPColor(GameObject prefab)
    {
        TextMeshProUGUI[] tmpUGUIComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmpUGUIComponent in tmpUGUIComponents)
        {
            if (tmpUGUIComponent.color == Color.white &&
                tmpUGUIComponent.colorGradient.bottomLeft==Color.white && 
                tmpUGUIComponent.colorGradient.bottomRight==Color.white&& 
                tmpUGUIComponent.colorGradient.topLeft==Color.white&& 
                tmpUGUIComponent.colorGradient.topRight==Color.white)
            {
                tmpUGUIComponent.color = UIHelper.HexColorStrToColor("#e7f0ff");
                EditorUtility.SetDirty(prefab);
            }
        }

        TextMeshPro[] tmpComponents = prefab.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var tmpComponent in tmpComponents)
        {
            if (tmpComponent.color == Color.white&&
                tmpComponent.colorGradient.bottomLeft==Color.white && 
                tmpComponent.colorGradient.bottomRight==Color.white&& 
                tmpComponent.colorGradient.topLeft==Color.white&& 
                tmpComponent.colorGradient.topRight==Color.white)
            {
                tmpComponent.color = UIHelper.HexColorStrToColor("#e7f0ff");
                EditorUtility.SetDirty(prefab);
            }
        }
    }
    
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("列表",GUILayout.Height(50)))
        {
            CheckTMPComponentTag();
        }
        // if (GUILayout.Button("一键替换",GUILayout.Height(50)))
        // {
        //     ChangeTMPComponentColor();
        // }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("刷新",GUILayout.Height(50)))
        {
            CheckTMPComponentTag();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        int pos = 0;
        foreach (var item in ItemList)
        {
            EditorGUILayout.BeginHorizontal();
      
            EditorGUILayout.ObjectField(string.Empty,item.Key, typeof(GameObject), true,GUILayout.Width(200),GUILayout.Height(30));
            
                
            GUILayout.Label($"纯白数量 ：{item.Value.Count}", GUILayout.Width(150),GUILayout.Height(30));
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < item.Value.Count; i++)
            {
                name = GetparentPath(item.Value[i].transform);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
         
                if (GUILayout.Button("选中节点",GUILayout.Width(60),GUILayout.Height(30)))
                {
                    int pos1 = i;
                    SelectGame(item.Value[pos1],name);
                }
                if (GUILayout.Button("设置白色",GUILayout.Width(60),GUILayout.Height(30)))
                {
                    int pos1 = i;
                    SelectWhite(item.Value[pos1],name);
                }
                GUILayout.Label($"ChildiName:{name}", GUILayout.Width(800),GUILayout.Height(30));
           
                EditorGUILayout.EndHorizontal();
            }
            pos++;
        }
 
        EditorGUILayout.EndScrollView();
    }
       
    private void SelectWhite(GameObject gameObject,string name)
    {
        int index = name.IndexOf("/");
        var name1 = name.Substring( 0,index+1);
        name = name.Replace(name1, "");
        if (gameObject==null || Selection.activeGameObject==null)
        {
            return;
        }

        if (gameObject != null)
        {
            var parent = GetTopParent(Selection.activeGameObject);
            var text = parent?.Find(name)?.GetComponent<TextMeshProUGUI>();
            Debug.Log($"选中节点名字{Selection.activeGameObject.transform.name} 父节点名字 {parent?.name}   子物体路径：{name}");

            if (text!=null)
            {
                text.color = UIHelper.HexColorStrToColor("#e7f0ff");
                Selection.activeGameObject = text.gameObject;
                EditorUtility.SetDirty(text);
                return;
            }
            
            var text1 = parent?.Find(name)?.GetComponent<TextMeshPro>();
            
            if (text1!=null)
            {
                text1.color = UIHelper.HexColorStrToColor("#e7f0ff");
                Selection.activeGameObject = text1.gameObject;
                EditorUtility.SetDirty(text1);
                return;
            }
            
            var text2 = parent?.Find(name)?.GetComponent<Text>();
            
            if (text2!=null)
            {
                text2.color = UIHelper.HexColorStrToColor("#e7f0ff");
                Selection.activeGameObject = text2.gameObject;
                EditorUtility.SetDirty(text2);
                return;
            }
        }
    }
       
    private void SelectGame(GameObject gameObject,string name)
    {
        int index = name.IndexOf("/");
        var name1 = name.Substring( 0,index+1);
        name = name.Replace(name1, "");
        if (gameObject==null || Selection.activeGameObject==null)
        {
            return;
        }

        if (gameObject != null)
        {
            var parent = GetTopParent(Selection.activeGameObject);
            var text = parent?.Find(name)?.GetComponent<TextMeshProUGUI>();
            Debug.Log($"选中节点名字{Selection.activeGameObject.transform.name} 父节点名字 {parent?.name}   子物体路径：{name}");

            if (text!=null)
            {
                Selection.activeGameObject = text.gameObject;
                return;
            }
            
            var text1 = parent?.Find(name)?.GetComponent<TextMeshPro>();
            
            if (text1!=null)
            {
                Selection.activeGameObject = text1.gameObject;
                return;
            }
            
            var text2 = parent?.Find(name)?.GetComponent<Text>();
            
            if (text2!=null)
            {
                Selection.activeGameObject = text2.gameObject;
                return;
            }
        }
    }
    
    //获得子节点完整路径
    private Transform GetTopParent(GameObject currentTransform)
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
       
    private string GetparentPath(Transform currentTransform)
    {
        StringBuilder str = new StringBuilder();
        GetPath(currentTransform, str);
        return str.ToString();
    }

    //获得子节点完整路径
    private void GetPath(Transform currentTransform, StringBuilder path=null)
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
    
    public void CheckTMPComponentTag()
    {
        ItemList.Clear();
        string path = $"{WorkFolder}/Assets/GameAssets/Prefabs"; //预制体路径
        List<string> prefabPaths = GetPrefabPaths(path);
        foreach (var prefabPath in prefabPaths)
        {
            string assetPath = "Assets" + prefabPath.Substring(ProjectPath.Length);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                var name = CheckTMPColor(prefab);
                if(name!=null && name.Count>0)
                    ItemList.Add(prefab,name);
            }
        }
    }
    
    private List<string> GetPrefabPaths(string path)
    {
        List<string> prefabPaths = new List<string>();
        string[] files = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            prefabPaths.Add(file);
        }
        return prefabPaths;
    }
    
    private List<GameObject> CheckTMPColor(GameObject prefab)
    {
        List<GameObject> tagErrorList = new();
        TextMeshProUGUI[] tmpUGUIComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmpUGUIComponent in tmpUGUIComponents)
        {
            if (tmpUGUIComponent.color == Color.white &&
                tmpUGUIComponent.colorGradient.bottomLeft==Color.white && 
                tmpUGUIComponent.colorGradient.bottomRight==Color.white&& 
                tmpUGUIComponent.colorGradient.topLeft==Color.white&& 
                tmpUGUIComponent.colorGradient.topRight==Color.white)
            {
                tagErrorList.Add(tmpUGUIComponent.gameObject);
            }
        }

        TextMeshPro[] tmpComponents = prefab.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var tmpComponent in tmpComponents)
        {
            if (tmpComponent.color == Color.white&&
                tmpComponent.colorGradient.bottomLeft==Color.white && 
                tmpComponent.colorGradient.bottomRight==Color.white&& 
                tmpComponent.colorGradient.topLeft==Color.white&& 
                tmpComponent.colorGradient.topRight==Color.white)
            {
                tagErrorList.Add(tmpComponent.gameObject);
            }
        }
        return tagErrorList;
    }
}
