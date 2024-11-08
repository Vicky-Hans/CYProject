---
--- 文件名称:  BulletChatViewFactory
--- 创建者:    nieshihai
--- 创建时间:  2022/2/28 10:55
-------------------------------------------------------------------
--- 功能描述：
--- 弹幕功能，使用对应的 BulletChatView C#类

require("Common/System")
local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper

--模块
---@class BulletChatViewFactory
local BulletChatViewFactory = bpcClass("BulletChatViewFactory")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function BulletChatViewFactory:ctor(content, prefabTemplate)
    ---@type Transform
    self.content = content
    self.prefabTemplate = prefabTemplate
    self.prefabPrepare = false
    ---@type CircularDynamicCollectionBinding
    self.binding = nil
end

---@param binding CircularDynamicCollectionBinding
---@return BulletChatViewFactory
function BulletChatViewFactory:Init(binding)
    self.BulletChatView = self.content:GetComponentInParent("DH.UIFramework.BulletChatView")
    self.binding = binding
    return self
end

---@public
function BulletChatViewFactory:Release()
    self.content = nil
    self.binding = nil
    self.BulletChatView = nil
end

function BulletChatViewFactory:OnTemplatePrepared(prefab)
    self.prefabPrepare = true
    self.BulletChatView.m_CellGameObject = prefab
    self.BulletChatView.m_CellParentTrans = self.content

    local refreshItemCallback = function(cell, index)
        self:RefreshItemCallBack(cell, index)
    end
    
    local removeItemCallBack = function(cell, index)
        self:OnRemoveItemCallBack(cell)
    end

    local getItemAdaptingSize = function(index)
        return self:GetItemAdaptingSize(index)
    end
    
    local onItemPauseCallBack = function(index)
        self:OnItemPauseStateCallBack(index, true)
    end
    
    local onItemResumeCallBack = function(index)
        self:OnItemPauseStateCallBack(index, false)
    end
    
    self.BulletChatView.m_RefreshItemCallBack = refreshItemCallback
    self.BulletChatView.m_RemoveItemCallBack = removeItemCallBack
    self.BulletChatView.m_GetItemAdaptingSize = getItemAdaptingSize
    self.BulletChatView.m_OnItemPauseCallBack = onItemPauseCallBack
    self.BulletChatView.m_OnItemResumeCallBack = onItemResumeCallBack
    self.BulletChatView:Init()

    self:UpdateView()
end

function BulletChatViewFactory:PrepareTemplate()
    if type(self.prefabTemplate) == 'userdata' then
        self:OnTemplatePrepared(self.prefabTemplate)
    else
        PrefabManagerLua.PreparePrefab(self.prefabTemplate, function(prefab)
            self:OnTemplatePrepared(prefab)
        end)
    end
end

---@public
function BulletChatViewFactory:UpdateView(bJumpStart)
    if self.binding == nil or not self.binding:IsBound() then
        return
    end

    if not self.prefabPrepare then
        return
    end

    local count = self.binding:Count()
    self.BulletChatView:ShowList(count)
end

---@public
function BulletChatViewFactory:ClearList()
    if  not bpcIsNull(self.BulletChatView) then
        self.BulletChatView:Clear()
    end
end

---@private
function BulletChatViewFactory:OnRemoveItemCallBack(cell)
    self.binding:UnbindItem(cell)
end

---@private
--- 点击当前的暂停
function BulletChatViewFactory:OnItemPauseStateCallBack(index, pause)
    local source = self.binding:GetItemSource(index)
    
    if source and source.OnItemPause then
        source:OnItemPause(pause)
    end
end

---@private
function BulletChatViewFactory:RefreshItemCallBack(cell, index)
    self.binding:BindItem(cell, index)
end

---@private
function BulletChatViewFactory:GetItemAdaptingSize(index)
    return self.binding:GetItemSize(index, false)
end

function BulletChatViewFactory:AddItems(curCount)
    self:UpdateView()
end

function BulletChatViewFactory:InsertItems(index, insertCount)
    self:UpdateView()
end

return BulletChatViewFactory