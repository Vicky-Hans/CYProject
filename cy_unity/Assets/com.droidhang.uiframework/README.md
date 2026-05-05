# DroidHang UIFramework

Unity 内部使用的 UGUI MVVM 框架。提供 View/ViewModel 基类、表达式数据绑定、可观察集合、命令、循环列表、Tab、本地化钩子等。

## 入门指引

第一次接触框架，先看这 5 个文件：

1. `Runtime/Mvvm/View/BaseView.cs` —— 所有界面 prefab 的基类，管 Canvas、显示/隐藏、绑定上下文、生命周期。
2. `Runtime/Mvvm/ViewModels/ViewModelBase.cs` —— ViewModel 基类，集成 `ObservableObject` 与 Messenger。
3. `Runtime/Core/Observables/ObservableObject.cs` —— 属性变更通知的原子能力（`INotifyPropertyChanged`）。
4. `Runtime/Binding/Contexts/BindingContext.cs` —— 一个 View 上的所有绑定都挂在这里，由 `BindingContextLifecycle` 跟随 Unity 生命周期。
5. `Runtime/Binding/Builder/BindingSet.cs` —— 用代码方式声明 View ↔ VM 绑定的入口（`view.CreateBindingSet<…>()` 拿到的就是它）。

## 目录速览

```text
Runtime/
  Core/                 框架内核：通知、消息、服务容器、应用上下文
    Observables/        属性变更与可观察集合（ObservableObject / ObservableList / ObservableDictionary 等）
    Messaging/          Messenger / Subject / 主题订阅
    Services/           IServiceContainer 与 ServiceBundle
    AppContext/         Application/Player Context（namespace: DH.UIFramework.AppContext）

  Binding/              数据绑定子系统（命名空间多在 DH.UIFramework.* 下）
    Binding.cs          Binding 主实现
    Contexts/           BindingContext / BindingContextLifecycle
    Builder/            BindingSet / BindingBuilder 等代码式绑定 API
    Binders/            Binder / StandardBinder
    Converters/         BindingConverter 公共契约
    Parameters/         绑定参数封装（ParameterWrapCommand 等）
    Paths/              绑定表达式路径解析
    Proxy/              源/目标代理（Sources/Targets）
      Sources/Object    属性/字段链式代理
      Sources/Expressions Linq Expression 解析
      Sources/Text      文本/字面量代理
      Targets/UGUI      UGUI 目标代理（事件/属性）
      Targets/UIElements UIElements 目标代理
      Targets/Universal 反射通用代理
    Reflection/         Proxy*Info 反射缓存
    Registry/           KeyValueRegistry 通用注册表

  Mvvm/                 MVVM 三件套：View / ViewModel / Interaction
    View/               BaseView*（含 partial）/ BaseItemView / SubViewBinder / Converter
    ViewModels/         ViewModelBase / IViewModel / 集合双向链 / 内置弹窗 VM
    Interactivity/      InteractionRequest / Notification / Action

  Components/           可独立使用的 UI 组件（不依赖具体业务）
    CircularScrollView/ 循环列表（含 GroupExpand / FlipPage 等扩展）
    TabView/            页签组件 TabGroupView
    Sorting/            Canvas/Renderer 排序层修正
    Clipper/            Clipper2D / Clipper3D / ClipperMaterialManager
    TMP/                TMP_TextEventHandler / TMP_SystemFontAsset / NativeOsFontGenerator
    Misc/               HoldButton / NonDrawingGraphic / OnTransformDimensionChanged / BackgroundSizeFitter
    Collection/         IndexedEnumerableCollectionBase / SimpleCollectionBinder / StaticItemsBindComponent / IScrollItemAnimation / ScrollRectExtend

  Commands/             ICommand / CommandBase / AsyncCommand / SimpleCommand / RelayCommand / CompositeCommand
  Attributes/           AutoNotifyAttribute / CommandAttribute（用于代码生成）
  Diagnostics/          DiagnosticsExtensions / ViewModelTracker
  Utilities/            通用工具：WeakDictionary / Variable / VariableArray / DataConverter / UnityExtension /
                        MonoBehaviourExtension / ScriptReference / DragEventTriggerListener / ObservableSingleton /
                        UnityProxyRegister / BpcRotate / ResourceManager（DownLoadImageFromUrl 等）

  LuaScript/            框架的 Lua 接入层（149 个 .lua）。仅在使用 Lua 接入路径时才需要，C# MVVM 用法不依赖此层。

Editor/
  CodeGen/              AssetConfig（旧版 Lua 绑定生成器源码已移除）
  Templates/            ViewTemplate.txt / ViewTemplate.cs
  Windows/              LuaFileNameWindow
  Inspectors/           UICircularScrollView / GroupExpand / FlipPage 自定义 Inspector
  Drawers/              Variable / VariableArray / ScriptReference 的 PropertyDrawer
  Settings/             LuaSettings
  Packaging/            发布/导出 unitypackage 的 Editor 工具（asmdef: DH.UIFramework.Packaging.Editor）
```

## 命名空间约定

- 主程序集 asmdef：`DH.UIFramework.MVVM`（位于 `Runtime/`）。
- Editor 发布工具 asmdef：`DH.UIFramework.Packaging.Editor`（位于 `Editor/Packaging/`）。
- 业务代码引用框架时常用以下 using：
  - `DH.UIFramework`（BaseView、BaseItemView、ScriptReference、各类组件）
  - `DH.UIFramework.ViewModels`（ViewModelBase）
  - `DH.UIFramework.Observables`（ObservableObject、ObservableList、ObservableDictionary）
  - `DH.UIFramework.Contexts`（BindingContext / IBindingContext / BindingContextLifecycle）
  - `DH.UIFramework.AppContext`（Context、ApplicationContext、PlayerContext —— **应用级别**上下文，与 Binding 上下文不同）
  - `DH.UIFramework.Builder`（BindingSet 等）
  - `DH.UIFramework.Messaging`（Messenger、PropertyChangedMessage）

## 迁移说明（相对旧目录结构）

- **应用上下文命名空间**：`Context` / `ApplicationContext` / `PlayerContext` 已从 `DH.UIFramework.Contexts` 迁至 **`DH.UIFramework.AppContext`**。绑定相关类型（`BindingContext`、`IBindingContext` 等）仍在 **`DH.UIFramework.Contexts`**。
- **发布工具路径**：原 `PackageImport/` 已合并为 **`Editor/Packaging/`**；`PackageImporter` 中忽略的导出目录已同步为 `Editor/Packaging`。
- **组件脚本**：`BackgroundSizeFitter` 类名已修正（原拼写 `BackgroudSizeFitter`），文件位于 `Runtime/Components/Misc/BackgroundSizeFitter.cs`。
- **重复 TypeExtensions**：已删除未使用的 `Runtime/Support/TypeExtensions.cs`（与 `Binding/TypeExtensions.cs` 重名且未被引用）。

## 关于 LuaScript 目录

`Runtime/LuaScript/` 是为 Lua 侧准备的等价 UI 框架接入层。

- 如果你的项目纯 C# 接入框架，**完全不需要看这个目录**。
- 如果你的项目使用 Lua 业务，再走 `LuaScript/UI/UIManager.lua` 等入口。
- 该目录与 C# 框架解耦，编辑这些文件不会影响 C# 程序集编译。
