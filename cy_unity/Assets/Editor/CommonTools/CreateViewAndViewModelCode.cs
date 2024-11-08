using System;
using System.Collections.Generic;
using System.IO;
using DG.DemiEditor;
using DH.UIFramework;
using Extend;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Tool
{
    public class CreateViewAndViewModelCode : EditorWindow
    {
        public enum  EInheritType
        {
            BaseView,
            BaseItemView
        }
        
        private EInheritType inheritType = EInheritType.BaseView;
        private const int WindowWidth = 400;
        private const int WindowHeight = 600;
        private string functionName;
        private string templatePath = "Assets\\Scripts\\Game\\UI\\";
        private string outputFolderPath =  "Assets\\Scripts\\Game\\UI\\";
        private bool isFullScreen;
        private string templateFolderPath = "Assets\\Editor\\CommonTools\\TempLate\\";

        private GameObject prefab;
        

        // private MonoBehaviour targetScript;
        private string propertyStart = "//##property_start##//";
        private string propertyBindingStart = "//##binding_start##//";
        private string viewScrollviewCellStart = "//##scrollview_cell_start##//";
        private string btnCallbackStart = "//##btn_callback_start##//";
        private string exportTag = "_ex";
        Dictionary<string, string> viewPorpertyDic = new Dictionary<string, string>();
        
        [MenuItem("DH Tools/生成 View 和 ViewModel")]
        public static void ShowWindow()
        {
            CreateViewAndViewModelCode tempWindow = GetWindow<CreateViewAndViewModelCode>();
            tempWindow.position = new Rect(100, 100, WindowWidth, WindowHeight);
        }
        
        private void OnGUI()
        {
            GUILayout.Label("生成 View和ViewModel", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("FunctionName", functionName);
            // templatePath = EditorGUILayout.TextField("Template Folder Path", templatePath);
            outputFolderPath = EditorGUILayout.TextField("Output Folder Path", outputFolderPath);
            
            inheritType = (EInheritType)EditorGUILayout.EnumPopup("继承类型", inheritType);
            isFullScreen = EditorGUILayout.Toggle("isFullScreen", isFullScreen);
            prefab = EditorGUILayout.ObjectField("绑定节点", prefab, typeof(GameObject),
                    false) as GameObject;
            // targetScript = EditorGUILayout.ObjectField("查找预制体使用指定脚本", targetScript, typeof(MonoBehaviour), false) as MonoBehaviour;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("生成"))
            {
                viewPorpertyDic.Clear();
                outputFolderPath = outputFolderPath.EndsWith("\\") ? outputFolderPath : $"{outputFolderPath}\\";
                
                if (prefab == null)
                {
                    Debug.LogError("请选择预制体");
                } else if (outputFolderPath.IndexOf(templatePath, StringComparison.Ordinal) == -1)
                {
                    Debug.LogError($"路径不对，请检查路径。 路径必须在 {templatePath} 下");
                }
                else if (LoadScript() != null)
                {
                    Debug.LogError(" 脚本已存在 请删除后重试");
                }
                else 
                {
                    // 检查是否有文件夹
                    CheckOrCreateFolder();
                    
                    GenerateClasses(functionName, templateFolderPath, outputFolderPath);
                    AssetDatabase.Refresh();
                    Debug.Log($"Generate successfully. viewPorpertyDic Count Is {viewPorpertyDic.Count}");
                    SaveDictionary();   
                }
            }

            if (GUILayout.Button("挂载 脚本 绑定 属性 "))
            {
                LoadDictionary();
                AttachScriptToPrefab();
                BindPropterToNode();
                
                // 标记当前对象为已修改
                EditorUtility.SetDirty(prefab);
                // 保存修改
                AssetDatabase.SaveAssets();

                // 刷新编辑器界面
                Repaint();
                
                AssetDatabase.Refresh();
            }

            EditorGUILayout.EndHorizontal();
            
            ListenerPrefabChange();
        }

        private void ListenerPrefabChange()
        {
            
            // 检查对象变化
            if (EditorGUI.EndChangeCheck())
            {
                // 对象发生变化时的处理逻辑
                if(prefab != null)
                {
                    var prefabName = prefab.name;
                    if (prefabName.EndsWith("View"))
                    {
                        functionName = prefabName.Substring(0, prefabName.Length - 4);
                    }
                    else
                    {
                        functionName = prefabName;
                    }
                }
                else
                {
                    functionName = "";
                }
            }
        }
        private void CheckOrCreateFolder()
        {
            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
                Debug.Log("文件夹已创建：" + outputFolderPath);
            }
        }

        private void GenerateClasses(string tempFunctionName, string templatePath,
            string outputPath)
        {
            // 读取模板文件内容
            string templateViewFilePath = Path.Combine(templatePath, "TemplateView.cs");
            string templateViewModelFilePath = Path.Combine(templatePath, "TemplateViewModel.cs");
            string templateViewContent = File.ReadAllText(templateViewFilePath);
            string templateViewModelContent = File.ReadAllText(templateViewModelFilePath);

            var newViewName = tempFunctionName + "View";
            var newViewModelName = tempFunctionName + "ViewModel";
            // 替换模板中的占位符
            string viewClassCode = templateViewContent.Replace("TemplateView", newViewName);
            viewClassCode = viewClassCode.Replace("TempLateViewModel", newViewModelName);
            viewClassCode = viewClassCode.Replace("BaseView", inheritType.ToString());
            
            var repStr = $"{isFullScreen}".ToLower();
            viewClassCode = viewClassCode.Replace("public override bool FullScreen => true;",
                $"public override bool FullScreen => {repStr};");
            string viewModelClassCode =
                templateViewModelContent.Replace("TempLateViewModel", newViewModelName);

            // 生成 View 类代码文件
            string viewClassFilePath = Path.Combine(outputPath, newViewName + ".cs");
            File.WriteAllText(viewClassFilePath, viewClassCode);

            // 生成 ViewModel 类代码文件
            string viewModelClassFilePath = Path.Combine(outputPath, newViewModelName + ".cs");
            File.WriteAllText(viewModelClassFilePath, viewModelClassCode);
            AssetDatabase.Refresh();
            Debug.Log("Classes generated successfully.");

            if (prefab == null)
            {
                Debug.LogError("Prefab is not assigned.");
                return;
            }

            Transform parentTransform = prefab.transform;
            List<Transform> allChildNodes = new List<Transform>();
            allChildNodes.Clear();

            // 递归遍历所有子节点，并将其添加到列表中
            TraverseHierarchy(parentTransform, ref allChildNodes);
            // 生成属性
            CreateProperty(ref allChildNodes);

            Debug.Log("Property generated successfully.");
        }

        private void CreateProperty(ref List<Transform> childNodeList)
        {
            var replaceViewStr = "\n";
            var replaceViewBindingStr = "\n";
            var viewScrollviewCellStr = "\n";
            var replaceViewModelStr = "\n";
            var replaceViewModelBtnCallbackStr = "\n";
            Debug.Log($"节点个数 {childNodeList.Count}");
            // 遍历列表，为每个节点生成属性
            foreach (Transform child in childNodeList)
            {
                // 获取节点名称
                string nameStr = Path.GetFileNameWithoutExtension(child.name);
                // 判断是否需要导出
                var tempStr = nameStr.ToLower();
                if (tempStr.IndexOf(exportTag, StringComparison.Ordinal) == -1) continue;

                var count = nameStr.Length;
                var tempName = nameStr.Substring(0, count - 3);
                child.name = tempName;
                
                AddPropertyToStr(child, ref replaceViewStr, ref replaceViewBindingStr,
                    ref replaceViewModelStr, ref replaceViewModelBtnCallbackStr,
                    ref viewScrollviewCellStr);
                
                // 标记当前对象为已修改
                EditorUtility.SetDirty(child);
            }
            // 保存修改
            AssetDatabase.SaveAssets();

            // 刷新编辑器界面
            Repaint();

            outputFolderPath = outputFolderPath.EndsWith("\\") ? outputFolderPath : $"{outputFolderPath}\\";
            // view 代码 写入
            var newViewName = functionName + "View";
            string viewClassFilePath = Path.Combine(outputFolderPath, newViewName + ".cs");
            string newViewScript = File.ReadAllText(viewClassFilePath);
            // 替换属性
            newViewScript = newViewScript.Replace(propertyStart, replaceViewStr);
            newViewScript = newViewScript.Replace(propertyBindingStart, replaceViewBindingStr);
            newViewScript = newViewScript.Replace(viewScrollviewCellStart, viewScrollviewCellStr);
            // 替换 绑定
            File.WriteAllText(viewClassFilePath, newViewScript);
            // viewModel 代码 写入
            var newViewModelName = functionName + "ViewModel";
            string viewModelClassFilePath =
                Path.Combine(outputFolderPath, newViewModelName + ".cs");
            string newViewModelScript = File.ReadAllText(viewModelClassFilePath);
            newViewModelScript = newViewModelScript.Replace(propertyStart, replaceViewModelStr);
            newViewModelScript =
                newViewModelScript.Replace(btnCallbackStart, replaceViewModelBtnCallbackStr);
            File.WriteAllText(viewModelClassFilePath, newViewModelScript);
        }

        private void TraverseHierarchy(Transform parent, ref List<Transform> tempList)
        {
            // 遍历子节点
            Transform childs = parent.GetComponentInChildren<Transform>(true);
            foreach (Transform child in childs)
            {
                tempList.Add(child);
                // 递归调用，遍历子节点的子节点
                TraverseHierarchy(child, ref tempList);
            }
            
        }

        private string GetNodePath(Transform targetNode)
        {
            string path = targetNode.name;
            Transform parent = targetNode.parent;
            while (parent.parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }

        private MonoScript LoadScript()
        {
            outputFolderPath = outputFolderPath.EndsWith("\\") ? outputFolderPath : $"{outputFolderPath}\\";
            string scriptPath = $"{outputFolderPath}{functionName}View.cs"; // 替换为您的脚本路径

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (script != null)
            {
                Debug.Log("Script loaded: " + script.name);
                // 在这里可以对加载的脚本对象进行操作，例如获取类型信息、实例化对象等
                return script;
            }
            else
            {
                Debug.LogError("Failed to load script at path: " + scriptPath);
                return null;
            }
        }

        private Component GetCreateComponent()
        {
            MonoScript scripts = LoadScript();
            if (prefab == null || scripts == null)
            {
                Debug.LogWarning("Prefab and script name must be assigned.");
                return null;
            }

            Type scriptType = scripts.GetClass();
            if (scriptType == null)
            {
                Debug.LogWarning("Invalid script name: " + scripts);
                return null;
            }

            Undo.RecordObject(prefab, "Add Script Component");
            if (!prefab.TryGetComponent(scriptType, out var component))
            {
                return null;
            }

            return component;
        }

        /// <summary>
        /// 挂脚本到预制体
        /// </summary>
        private void AttachScriptToPrefab()
        {
            Component component = GetCreateComponent();
            if (component == null)
            {
                MonoScript scripts = LoadScript();
                if (prefab == null || scripts == null)
                {
                    Debug.LogWarning("Prefab and script name must be assigned.");
                    return;
                }

                Type scriptType = scripts.GetClass();
                if (scriptType == null)
                {
                    Debug.LogWarning("Invalid script name: " + scripts);
                    return;
                }

                Undo.RecordObject(prefab, "Add Script Component");
                component = prefab.AddComponent(scriptType);
            }

            if (component != null)
            {
                Debug.Log("Script attached to the prefab successfully.");
            }
            else
            {
                Debug.Log("Failed to attach script to the prefab.");
            }
            
        }

        private void AddPropertyToStr(Transform childNode, ref string viewPorperty,
            ref string viewBind, ref string vmPropterty, ref string vmBtnCallback,
            ref string viewScrollViewCell)
        {
            
            CheckEx(childNode, ref viewPorperty, ref viewBind, ref vmPropterty, ref vmBtnCallback, ref viewScrollViewCell);
            EditorUtility.SetDirty(childNode);
            
        }

        /// <summary>
        /// 脚本里面写入数据
        /// </summary>
        /// <param name="childNode"></param>
        /// <param name="viewPorperty"></param>
        /// <param name="viewBind"></param>
        /// <param name="vmPropterty"></param>
        /// <param name="vmBtnCallback"></param>
        /// <param name="viewScrollViewCell"></param>
        private void CheckEx(Transform childNode, ref string viewPorperty, ref string viewBind,
            ref string vmPropterty, ref string vmBtnCallback, ref string viewScrollViewCell)
        {
            string nameStr = Path.GetFileNameWithoutExtension(childNode.name);
            var tempPropertyViewName = nameStr.Substring(0, 1).ToLower() + nameStr.Substring(1);
            var tempPropertyVmName = nameStr.Substring(0, 1).ToUpper() + nameStr.Substring(1);
            // var toolTips = $"[Tooltip(\"{GetNodePath(childNode)}\")]\n\t\t";
            var toolTips = "";
            viewPorpertyDic.Add(tempPropertyViewName, GetNodePath(childNode));
            
            if (childNode.gameObject.TryGetComponent(out ScrollRectExtend _))
            {
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public ScrollRectExtend {tempPropertyViewName};\n";
                viewPorperty =
                    $"{viewPorperty}\t\t[AssetPath]public string {tempPropertyViewName}Cell;\n";
                viewScrollViewCell =
                    $"{viewScrollViewCell}\t\t\t{tempPropertyViewName}.PrefabPath = {tempPropertyViewName}Cell;\n";
                vmPropterty =
                    $"{vmPropterty}\t\t //[AutoNotify] private ObservableList<int> {tempPropertyViewName}List = new();\n";
                viewBind =
                    $"{viewBind}\t\t\t//bindingSet.Bind({tempPropertyViewName}).For(v => v.Collection).To(vm => vm.{tempPropertyVmName}List);\n";
            }
            else if (childNode.gameObject.TryGetComponent(out UICircularScrollView _))
            {
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public UICircularScrollView {tempPropertyViewName};\n";
                viewPorperty =
                    $"{viewPorperty}\t\t[AssetPath]public string {tempPropertyViewName}Cell;\n";
                viewScrollViewCell =
                    $"{viewScrollViewCell}\t\t\t{tempPropertyViewName}.PrefabPath = {tempPropertyViewName}Cell;\n";
                vmPropterty =
                    $"{vmPropterty}\t\t //[AutoNotify] private ObservableList<int> {tempPropertyViewName}List = new();\n";
                viewBind =
                    $"{viewBind}\t\t\t//bindingSet.Bind({tempPropertyViewName}).For(v => v.Collection).To(vm => vm.{tempPropertyVmName}List);\n";
            }
            else if (childNode.gameObject.TryGetComponent(out BaseView outType))
            {
                viewPorperty = $"{viewPorperty}\t\t{toolTips}public {outType.GetType().Name} {tempPropertyViewName};\n";
                vmPropterty = $"{vmPropterty}\t\t[AutoNotify] private {outType.GetType().Name}Model {tempPropertyViewName}Vm;\n";
                viewBind = $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}.BindingContext).For(v => v.DataContext).To(vm => vm.{tempPropertyVmName}Vm);\n";

            }
            else if (childNode.gameObject.TryGetComponent(out DhButton _))
            {
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public DhButton {tempPropertyViewName};\n";
                vmBtnCallback =
                    $"{vmBtnCallback}\t\t[Command]\n\t\tprivate void OnClick{tempPropertyVmName}()\n\t\t{{}}\n";
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.onClick).To(vm => vm.OnClick{tempPropertyVmName}Command);\n";
            }
            else if (childNode.gameObject.TryGetComponent(out Button _))
            {
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public Button {tempPropertyViewName};\n";
                vmBtnCallback =
                    $"{vmBtnCallback}\t\t[Command]\n\t\tprivate void OnClick{tempPropertyVmName}()\n\t\t{{}}\n";
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.onClick).To(vm => vm.OnClick{tempPropertyVmName}Command);\n";
            }
            else if (childNode.gameObject.TryGetComponent(out DhText _))
            {
                viewPorperty = $"{viewPorperty}\t\t{toolTips}public DhText {tempPropertyViewName};\n";
                vmPropterty =
                    $"{vmPropterty}\t\t[AutoNotify] private string {tempPropertyViewName}Str;\n";
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.text).To(vm => vm.{tempPropertyVmName}Str);\n";

            }
            else if (childNode.gameObject.TryGetComponent(out TextMeshProUGUI _))
            {
                // 这里查看节点属性
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public TextMeshProUGUI {tempPropertyViewName};\n";
                vmPropterty =
                    $"{vmPropterty}\t\t[AutoNotify] private string {tempPropertyViewName}Str;\n";
                // bindingSet.Bind(Text_1).For(v => v.text).To(vm => vm.TextStr);
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.text).To(vm => vm.{tempPropertyVmName}Str);\n";

            }
            else if (childNode.gameObject.TryGetComponent(out Text _))
            {
                viewPorperty = $"{viewPorperty}\t\t{toolTips}public Text {tempPropertyViewName};\n";
                vmPropterty =
                    $"{vmPropterty}\t\t[AutoNotify] private string {tempPropertyViewName}Str;\n";
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.text).To(vm => vm.{tempPropertyVmName}Str);\n";

            }
            else if (childNode.gameObject.TryGetComponent(out DhImage _))
            {
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public DhImage {tempPropertyViewName};\n";
                vmPropterty =
                    $"{vmPropterty}\t\t[AutoNotify] private string {tempPropertyViewName}Path;\n";
                // bindingSet.Bind(ImageI).For(v => v.sprite).To(vm => vm.iconPath).WithConversion(this);
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.sprite).To(vm => vm.{tempPropertyVmName}Path).WithConversion(this);\n";
            }
            else if (childNode.gameObject.TryGetComponent(out Image _))
            {
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public Image {tempPropertyViewName};\n";
                vmPropterty =
                    $"{vmPropterty}\t\t[AutoNotify] private string {tempPropertyViewName}Path;\n";
                // bindingSet.Bind(ImageI).For(v => v.sprite).To(vm => vm.iconPath).WithConversion(this);
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.sprite).To(vm => vm.{tempPropertyVmName}Path).WithConversion(this);\n";
            }
            else if (childNode.gameObject.TryGetComponent(out Slider _))
            {
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public Slider {tempPropertyViewName};\n";
                vmPropterty =
                    $"{vmPropterty}\t\t[AutoNotify] private float {tempPropertyViewName}Value;\n";
                // bindingSet.Bind(SliderI).For(v => v.value).To(vm => vm.iconPath).WithConversion(this);
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.value).To(vm => vm.{tempPropertyVmName}Value);\n";
            }
            else if (childNode.gameObject.TryGetComponent(out StaticItemsBindComponent _))
            {
                viewPorperty = $"{viewPorperty}\t\t{toolTips}public StaticItemsBindComponent {tempPropertyViewName};\n";
                vmPropterty = $"{vmPropterty}\t\tpublic Func<object, object> Get{tempPropertyVmName}CellCallback => Get{tempPropertyVmName}CellCallbackByIndex;\n";
                vmPropterty = $"{vmPropterty}\t\t//[AutoNotify] private ObservableDictionary<int,int> {tempPropertyViewName}Dictionary = new();\n";
                vmBtnCallback = $"{vmBtnCallback}\t\tprivate object Get{tempPropertyVmName}CellCallbackByIndex(object index)\n\t\t{{\n\t\t\treturn null;\n\t\t}}\n";
                viewBind = $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.Get{tempPropertyVmName}CellCallback);\n";
                viewBind = $"{viewBind}\t\t\t//bindingSet.Bind({tempPropertyViewName}).For(v => v.Collection).To(vm => vm.{tempPropertyVmName}Dictionary);\n";
            } 
            else 
            {
                
                viewPorperty =
                    $"{viewPorperty}\t\t{toolTips}public GameObject {tempPropertyViewName};\n";
                vmPropterty =
                    $"{vmPropterty}\t\t[AutoNotify] private bool isShow{tempPropertyVmName};\n";
                // bindingSet.Bind(SliderI).For(v => v.value).To(vm => vm.iconPath).WithConversion(this);
                viewBind =
                    $"{viewBind}\t\t\tbindingSet.Bind({tempPropertyViewName}).For(v => v.activeSelf).To(vm => vm.IsShow{tempPropertyVmName});\n";
                
            }

        }
        private void BindPropterToNode()
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null");
                return;
            }

            Component component = GetCreateComponent();
            if (component == null)
            {
                Debug.LogError("Prefab not found the script!");
                return;
            }

            // 获取脚本类型
            Type scriptType = component.GetType();
            if (!prefab.TryGetComponent(scriptType, out Component scriptInstance))
            {
                Debug.LogError("Prefab not found the script!");
                return;
            }

            SerializedObject serializedObject = new SerializedObject (scriptInstance); // 创建 SerializedObject
            foreach (var item in viewPorpertyDic)
            {
                SerializedProperty property = serializedObject.FindProperty(item.Key); // 获取字段的 SerializedProperty

                Transform targetObject = prefab.transform.Find(item.Value); // 在场景中查找目标对象

                if (property != null && targetObject != null)
                {
                    property.SetValue(targetObject.gameObject); // 设置引用对象
                    serializedObject.ApplyModifiedProperties(); // 应用修改
                }
            }
            Debug.Log("Properties bound to nodes for prefab: " + prefab.name);
        }

        private string bindKey = "bind_Dictionary";
        private void LoadDictionary()
        {
            viewPorpertyDic.Clear();

            int count = EditorPrefs.GetInt($"{bindKey}Count");

            for (int i = 0; i < count; i++)
            {
                string key = EditorPrefs.GetString($"{bindKey}Key" + i);
                string value = EditorPrefs.GetString($"{bindKey}Value" + i);
                viewPorpertyDic.Add(key, value);
            }
        }

        private void SaveDictionary()
        {
            EditorPrefs.SetInt($"{bindKey}Count", viewPorpertyDic.Count);
            int i = 0;
            foreach (var kvp in viewPorpertyDic)
            {
                string key = kvp.Key;
                string value = kvp.Value;
                EditorPrefs.SetString($"{bindKey}Key" + i,key);
                EditorPrefs.SetString($"{bindKey}Value" + i,value);
                i++;
            }
        }
        private void OnEnable()
        {
            LoadDictionary();
        }

        private void OnDisable()
        {
            SaveDictionary();
        }
    }

}