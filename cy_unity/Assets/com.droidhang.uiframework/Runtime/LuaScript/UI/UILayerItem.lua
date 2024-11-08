---
--- 文件名称:  UILayerItem
--- 创建者:    nieshihai
--- 创建时间:  2021/10/27 10:10
-------------------------------------------------------------------
--- 功能描述：
---
---

--模块
---@class UILayerItem
---@field oldView UILayerItem
local UILayerItem = bpcClass("UILayerItem")
local MenuManagerWrapper = CS.DH.UIFramework.MenuManagerWrapper
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
local LayerItemState = require("UI/LayerItemState")

---@public
function UILayerItem:Init(config, layer, sid)
    ---@type UIBaseViewModel
    self.uiViewModel = nil
    self.viewDataContext = nil
    self.serialId = sid
    ---@type ConfigItem
    self.config = config
    ---@type UILayer
    self.uiLayer = layer
    ---@type LayerItemState
    self.itemState = LayerItemState.None

    if not self.config.isMulti then
        self.uiKey = self.config.uiKey
    else
        self.uiKey = self.config.uiKey..sid
    end
end

---@public
---@return string @UI唯一的key
function UILayerItem:GetUIUniqueKey()
    return self.uiKey
end

---@public
--- 第一次加载该UI
function UILayerItem:LoadUI()
    self:SetLayerItemState(LayerItemState.Loading)
    local uiKey = self.config.uiKey
    self:LoadUIPrefab(uiKey, self.uiLayer:GetNextOrder(), nil)
    self.uiLayer:AddUI(uiKey)
end

---@public
--- 从所属的layer里关闭UI
function UILayerItem:UnLoadUI()
    if self.itemState == LayerItemState.Loading then
        error(self.uiKey.."正在loading， 不应该被unload，请检查逻辑")
    end
    
    self:SetLayerItemState(LayerItemState.Release)
    self:UnBindSource()
    self:UnLoadUIPrefab()
end

---@public
--- UI被遮挡
function UILayerItem:OnCover()
    self:SetLayerItemState(LayerItemState.OnCover)
    self:UnLoadUIPrefab()
end

---@public
--- 处于底层的UI被提到最高层
function UILayerItem:OnRefocusUI(callback)
    self:SetLayerItemState(LayerItemState.OnRefocus)
    self.uiLayer:Take2Top(self.config.uiKey)

    if self.panelObj == nil then
        self:SetLayerItemState(LayerItemState.Loading)
        local uiKey = self.config.uiKey
        self:LoadUIPrefab(uiKey, self.uiLayer:GetCurMaxOrder(), callback)
    else
        if callback then
            callback()
        end
    end

    return true
end

---@public
function UILayerItem:BindSource(vmSource)
    self.uiViewModel = vmSource
    self:BindUIViewModelToPanel()
end

---@private
function UILayerItem:UnBindSource()
    if self.uiViewModel ~= nil then
        self.uiViewModel:Release()

        if self.viewDataContext ~= nil then
            self.viewDataContext:SetSource(nil)
        end
    end

    self.uiViewModel = nil
    self.viewDataContext = nil
end

---@private
function UILayerItem:LoadUIPrefab(uiKey, order, callback)
    MenuManagerWrapper.LoadMenu(uiKey, self.uiLayer.layerRoot, order, self.config, function(panel)
        self:SetLayerItemState(LayerItemState.Open)
        self.panelObj = panel
        
        --- 此处会判断通用弹窗的动画
        local animator = panel:GetComponent(typeof(CS.UnityEngine.Animator))
        if not bpcIsNull(animator) then
            animator:SetBool("Open", true)
        end
        
        self:BindUIViewModelToPanel()

        if not self.config.transparent and self.oldView and self.oldView.config and not self.oldView.config.transparent then
            self.oldView:OnCover()
        end
        
        self.oldView = nil

        if callback then
            callback()
        end
    end)

    self.uiLayer:AddUI(uiKey)
end

---@private
--- 只是卸载Prefab，但不从所属的layer里移除
function UILayerItem:UnLoadUIPrefab()
    local uiKey = self:GetUIUniqueKey()
    MenuManagerWrapper.ReleaseMenu(uiKey)
    self.panelObj = nil
    self.viewDataContext = nil
end

---@private
function UILayerItem:BindUIViewModelToPanel()
    if self.viewDataContext ~= nil then
        return
    end
    
    if self.panelObj == nil or self.uiViewModel == nil then
        return
    end

    --- 只绑最外层的 LuaBehaviour,子物体下面的忽略，因为子物体下面的可能是通过最外层view去绑定的
    local luaTable = LuaBindingTargetWrapper.GetLuaTable(self.panelObj)
    if luaTable ~= nil then
        self.viewDataContext = luaTable["dataContext"]
        self.viewDataContext:SetSource(self.uiViewModel)
    end
end

---@private
function UILayerItem:SetLayerItemState(curState)
    self.itemState = curState
end

return UILayerItem