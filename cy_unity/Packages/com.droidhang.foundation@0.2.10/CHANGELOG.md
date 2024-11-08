## DHFoundation
DHFoundation是一个统一基础库，包含基础辅助类以及基础内存管理库。

## 使用说明
引擎组内部使用

## 详细文档


## ChangeLog
#### 0.0.1
1. 增加基础辅助类DHUtility，包含Hash计算、Json、文件路径处理。
2. 增加TaskPool用于任务管理系统。
3. 增加ConcurrentHashSet线程安全的HashSet数据结构。
4. 增加基础框架GameFrameworkModule以及GameFrameworkAction等，游戏基础框架类。
5. 增加ReferencePool内存池。

#### 0.0.2
1. 修复EventInternal.TriggerEvent抛出异常的问题

#### 0.1.0
1. 优化GameFramework组件管理，用以适配已经存在的单例整合到GameFramework
1. 优化序列化GC
1. UnityThreadModule增加在主线程调用时，直接调用

#### 0.1.1
1. GameFrameworkEntry新增模块释放功能

#### 0.1.2
1. 增加GameModuleLateUpdate的支持

#### 0.1.3
1.新增对Header Json支持的接口

#### 0.2.0
1. GameFramework增加Unity主线程同步模块，用于BestHttp回调线程同步(不支持AOD项目)

#### 0.2.1
1. 线程同步模块增加可以传入ThreadId的构造函数

#### 0.2.2
1. 增加统一加密工具

### 0.2.3
1. 修改MD5转小写方式为ToLowerInvariant

### 0.2.4
1. 增加自定义PlayerPrefs接口，用于跨平台获取PlayerPrefs数据
2. 修改Android平台文件系统到Foundation插件中

### 0.2.5
1. 修复命名空间问题

### 0.2.6
1. 为androidFileStream 添加条件编译，方便移除非依赖包

### 0.2.7
1. 回退到0.2.5。0.2.6修改非必要，删除；

### 0.2.8
### 0.2.9
1. 修复IGameFrameworkComponent释放时未自动调用Shutdown函数的问题
2. 开放GameFrameworkEntry

### 0.2.10
1. 移除对Android JNI在非Android平台的依赖
