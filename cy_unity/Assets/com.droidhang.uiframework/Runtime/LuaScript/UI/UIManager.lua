---
--- 文件名称:  UIManager
--- 创建者:    nieshihai
--- 创建时间:  2021/10/27 09:49
-------------------------------------------------------------------
--- 功能描述：
--- Open和Close对应的界面
---

--模块
---@class UIManager
local UIManager = bpcClass("UIManager")
local UITree = require("UI/UITree")
local List = require("Common/List")
local Dictionary = require("Common/Dictionary")
local UILayerItem = require("UI/UILayerItem")
local LayerItemState = require("UI/LayerItemState")
---@public
--- 初始化，不用手动调用，会在UITree的Init里自动调用
function UIManager:Init(config,callback)
    -- 表里做反向索引
    for uiKey, value in pairs(config) do
        value.uiKey = uiKey
    end
    
    self.serialId = 0

    ---@type table<string, ConfigItem>
    self.config = config
    
    ---@type table<number, UILayerItem> | Dictionary @<serialId, UILayerItem> 当前打开的UI
    self.treeUIs = Dictionary:New(number, table)
    
    ---@type table<string,number> | Dictionary @<uiKey, serialId>，对非单例的UI，key是uiKey+serialId
    self.key2SerialId = Dictionary:New(string, number)
    
    ---@type UILayerItem[] | List @缓存之前加载过的UIView
    self.cacheUIViews = List:New("UILayerItem")

    ---@type number[] |List @按UI的打开顺序的serialId栈；在这个栈里的UI需要是可一步一步返回的；所以toastUI这种需要单独管理；
    self.uiSerialIdStack = List:New(number)

    ---@type number[] |List @正在关闭的界面的sid
    self.closingSerialIdList = List:New(number)
    
    self.finalCloseFunc = function()
        self.closingSerialIdList:ForEach(function(sid) 
            self:CloseBySid(sid)
        end)

        self.closingSerialIdList:Clear()
    end
    
    UITree:Init(callback)
end

---@public
--- 打开界面
---@param vmPath string @传给require的继承于UIBaseViewModel的ViewModel的脚本路径
---@return UIBaseViewModel @与vmPath对应的vm对象
function UIManager:OpenWithVM(vmPath, ...)
    local uiVMClass = require(vmPath)
    
    ---@type UIBaseViewModel
    local uiVM = uiVMClass(...)
    local uiKey = uiVM:GetUIConfigKey()
    local count = self.uiSerialIdStack:Count()
    local oldView = nil
    if count > 0 then
        local sid = self.uiSerialIdStack[count]
        oldView = self.treeUIs[sid]
    end

    if not self:Open(uiKey) then
        return
    end

    count = self.uiSerialIdStack:Count()

    --- 获得栈顶UIView，按理就是上面打开的
    if count > 0 then
        local topSid = self.uiSerialIdStack[count]
        local view =  self.treeUIs[topSid]
        view.oldView = oldView
        view:BindSource(uiVM)
        uiVM.uiSerialId = topSid
    end
    
    return uiVM
end

---@public
--- 使用UIConfig打开界面，多实例UI和自动构建VM的UI不要使用这个接口
---@param uiKey string|UIConfig @UIConfig里的key或者也可以直接传对应的table
function UIManager:Open(uiKey)
    if self.treeUIs == nil then
        error("You should call UIManager:Init first")
        return false
    end

    local key = uiKey
    local uiConfig = self.config[uiKey]

    --- 兼容直接传入配置属性的情况
    if type(uiKey) == "table" and uiKey.uiKey ~= nil then
        -- 这种情况就是外面直接把UIConfig里对应UI的config table传进来了
        key = uiKey.uiKey
        uiConfig = uiKey
    end

    if uiConfig == nil then
        error(string.format("Invalid ui key %s",key))
        return false
    end
    
    --- 不是单例的需要把对应的UI提到最上层,sid不变
    if not uiConfig.isMulti and self.key2SerialId:ContainsKey(key) then
        local lastSerialId = self.key2SerialId[key]

        local count = self.uiSerialIdStack:Count()

        if count > 0 then
            local topSid = self.uiSerialIdStack[count]

            if topSid == lastSerialId then
                return true --- 已经在栈顶了
            end
        end
        
        local curView = self.treeUIs[lastSerialId]
        if curView == nil then
            error("UI的堆栈有问题，key2SerialId与treeUIs没有对应上")
        end

        self.uiSerialIdStack:Remove(lastSerialId)
        self.uiSerialIdStack:Add(lastSerialId)
        
        return curView:OnRefocusUI()
    end
    
    return self:OpenUIFromConfig(uiConfig)
end

---@private
function UIManager:OpenUIFromConfig(uiConfig)
    self.serialId = self.serialId + 1
    local layer = UITree:GetLayer(uiConfig.layer)
    ---@type UILayerItem
    local view

    if self.cacheUIViews:Count() > 0 then
        view = self.cacheUIViews:RemoveLast()
    end

    if view == nil then
        view = UILayerItem()
    end

    view:Init(uiConfig, layer, self.serialId)
    view:LoadUI()
    self.key2SerialId:Add(view:GetUIUniqueKey(), self.serialId)
    
    self.treeUIs:Add(self.serialId, view)
    self.uiSerialIdStack:Add(self.serialId)
    
    return true
end

---@public
--- 关闭界面，与OpenWithVM配对
---@param uiVM UIBaseViewModel
function UIManager:CloseWithVM(uiVM)
    if self.closingSerialIdList:Contains(uiVM.uiSerialId) then
        return
    end
    
    self:BackToSidParent(uiVM.uiSerialId)
end

-- 关闭最上层的界面
function UIManager:Close()
    local count = self.uiSerialIdStack:Count()
    
    if count > 0 then
        local sid = self.uiSerialIdStack[count]

        if self.closingSerialIdList:Contains(sid) then
            return
        end

        self.closingSerialIdList:Add(sid)
        self:RefocusTopUI(self.finalCloseFunc)
    end
end

---@private
function UIManager:RefocusTopUI(callback)
    local count = self.uiSerialIdStack:Count()
    local bFind = false
    
    for i = count, 1, -1 do
        local sid = self.uiSerialIdStack[i]

        if not self.closingSerialIdList:Contains(sid) then
            local topView = self.treeUIs[sid]

            if topView then
                topView:OnRefocusUI(callback)
            end
            bFind = true
            break
        end
    end

    if not bFind and callback then
        callback()
    end
end

---@private
function UIManager:BackToSidParent(sid)
    local count = self.uiSerialIdStack:Count()
    for i = count, 1, -1 do
        local tmpSid = self.uiSerialIdStack[i]
        self.closingSerialIdList:Add(tmpSid)
        
        if tmpSid == sid then
            break
        end
    end
    
    self:RefocusTopUI(self.finalCloseFunc)
end

---@private
function UIManager:CloseBySid(sid)
    local view = self.treeUIs[sid]
    if view ~= nil then
        local uiKey = view:GetUIUniqueKey()
        self.key2SerialId:Remove(uiKey)
        view:UnLoadUI()
    end

    self.treeUIs:Remove(sid)
    self.uiSerialIdStack:Remove(sid)
    self.cacheUIViews:Add(view)
    
    return view
end

function UIManager:Release()
    local count = self.uiSerialIdStack:Count()
    while(count > 0) do
        local sid = self.uiSerialIdStack[count]
        self:CloseBySid(sid)
        count = self.uiSerialIdStack:Count()
    end
    
    self.key2SerialId:Clear()
    self.closingSerialIdList:Clear()
end

function UIManager:GetUICamera()
    return UITree.uiCamera
end

return UIManager