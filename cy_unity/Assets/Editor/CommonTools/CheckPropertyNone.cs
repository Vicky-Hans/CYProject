using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DG.DemiEditor;
using DH.UIFramework;
using TMPro;
using UIFramework.Binding;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class CheckPropertyNoneWindow: EditorWindow
{

    private CheckPropertyNone tempComp;
    private Vector2 scrollPos;
    private Vector2 scrollPos1;
    private string tempDesc;
    private bool IsPassSet;
    // 将名为"My Window"的菜单项添加到 Window 菜单
    [MenuItem("DH Tools/检查脚本属性是否绑定")]
    public static void ShowWindow()
    {
        var window = GetWindow(typeof(CheckPropertyNoneWindow));
        window.titleContent = new GUIContent("检查脚本属性是否绑定");
    }
    
    private void OnEnable()
    {
        if (tempComp == null)
        {
            tempComp = new(true);
        }
        tempComp.CheckAllPrefab();
    }

    private void OnDestroy()
    {
        tempComp = null;

    }
    
    void OnGUI()
    {
        if (GUILayout.Button("刷新",GUILayout.Height(50)))
        {
            tempComp.CheckAllPrefab();
        }
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        int pos = 0;
        foreach (var item in tempComp.ItemList)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(string.Empty,item.Key, typeof(GameObject), true,GUILayout.Width(200),GUILayout.Height(30));
            GUILayout.Label($"需要替换的数量 ：{item.Value.Count}", GUILayout.Width(150),GUILayout.Height(30));
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < item.Value.Count; i++)
            {
                if(item.Value[i].Node.IsNull()) continue;
                name = tempComp.GetparentPath(item.Value[i].Node.transform);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
         
                if (GUILayout.Button("选中节点",GUILayout.Width(60),GUILayout.Height(30)))
                {
                    int pos1 = i;
                    SelectGame(item.Value[pos1].Node,name);
                }
                
                GUILayout.Label($"以下字段属性为空：", GUILayout.Width(150),GUILayout.Height(30));
                for (int j = 0; j < item.Value[i].InfoList.Count; j++)
                {
                    GUILayout.Label($"{item.Value[i].InfoList[j]}", GUILayout.Width(100),GUILayout.Height(30));
                }
                EditorGUILayout.EndHorizontal();
              
            }
            pos++;
        }
        EditorGUILayout.EndScrollView();
    }
       
    private void SelectGame(GameObject gameObject,string name)
    {
        int index = name.IndexOf("/");
        var name1 = name.Substring( 0,index+1);
        if (name1 == "" || name.Length == 0)
        {
            
        }
        else
        {
            name = name.Replace(name1, "");
            if (gameObject==null || Selection.activeGameObject==null)
            {
                return;
            }
        }

     

        if (gameObject != null)
        {
            var parent = tempComp.GetTopParent(Selection.activeGameObject);
            var text = parent?.Find(name);
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
}

public class CheckPropertyNone
{
    public struct PrefabInfo
    {
        public GameObject Node;
        public List<string> InfoList;
    }
    private static readonly string WorkFolder = Environment.CurrentDirectory;
    private static readonly string ProjectPath = Application.dataPath;
    
    public Dictionary<GameObject, List<PrefabInfo>> ItemList = new();

    private bool isWindow;

    public CheckPropertyNone(bool isWindow)
    {
        this.isWindow = isWindow;
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
    
    public void CheckAllPrefab()
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
                var name = CheckPrefabBind(prefab);
                if(name.Count>0)
                    ItemList.Add(prefab,name);
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
    
   
    public List<PrefabInfo> CheckPrefabBind(GameObject prefab)
    {
        List<PrefabInfo> tagErrorList = new();
        Transform[] allNodes = prefab.GetComponentsInChildren<Transform>(true);
        foreach (Transform node in allNodes)
        {
            var list = CheckUnBindPro(node.gameObject);
            if (list.Count > 0)
            {
                var info = new PrefabInfo()
                {
                    Node = node.gameObject,
                    InfoList =  list,
                };
                if (isWindow)
                {
                    tagErrorList.Add(info);   
                }
                else
                {
                    foreach (var nameStr in list)
                    {
                        Debug.LogError($"预制体属性绑定的为空 请检查 预制体 {prefab.name} 的 节点 {nameStr}"); 
                    }
                }
            }
        }
        return tagErrorList;
    }

    public List<string> CheckUnBindPro(GameObject obj)
    {
        List<string> unBindName = new();
        Component[] scripts = obj.GetComponents(typeof(BaseView));
        foreach (Component component in scripts)
        {
            Type scriptType = component.GetType();
            FieldInfo[] fields = scriptType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            SerializedObject serializedObject = new SerializedObject (component);
            foreach (FieldInfo field in fields)
            {
                SerializedProperty property = serializedObject.FindProperty(field.Name); // 获取字段的 SerializedProperty
                if (property!=null && property.GetValue() == null && !property.isArray)
                {
                    unBindName.Add($"{field.Name}");
                }
                
            }
        }

        return unBindName;
    }
    
}
