---
--- 文件名称:  CircularExpandTipFactory
--- 创建者:    nieshihai
--- 创建时间:  2021/9/22 16:26
-------------------------------------------------------------------
--- 功能描述：
--- 扩展Tip的循环列表。对应使用 ExpandTipsCircularScrollView 类。
--- ScrollView和item的pivot必须用左上角的方式

require("Common/System")
local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
local CircularDynamicViewFactory = require("Support/CircularDynamicViewFactory")
---
--模块
---@class CircularExpandTipFactory
local CircularExpandTipFactory = bpcClass("CircularExpandTipFactory", CircularDynamicViewFactory)

--[[--
构造函数
]]
function CircularExpandTipFactory:ctor(content, prefabTemplate, tipTemplate)
    CircularExpandTipFactory.super.ctor(self, content, prefabTemplate)
    self.tipTemplate = tipTemplate
    self.tipPrefabPrepare = false
    self.lastClickTipIdx = -1
end

---@param binding CircularDynamicCollectionBinding
---@return CircularExpandTipFactory
function CircularExpandTipFactory:Init(binding)
    self.CircularScrollView = self.content:GetComponentInParent("DH.UIFramework.ExpandTipsCircularScrollView")
    self.binding = binding
    self.tipObj = nil
    return self
end

---@public
function CircularExpandTipFactory:Release()
    CircularExpandTipFactory.super.Release(self)
    self.tipObj = nil
end

function CircularExpandTipFactory:PrepareTemplate()
    PrefabManagerLua.PreparePrefab(self.prefabTemplate, function(prefab)
        -- 初始化基本的prefab
        self.prefabPrepare = true
        self.CircularScrollView.m_CellGameObject = prefab
        self:CheckAllPrefabPrepared()
    end)
    
    PrefabManagerLua.PreparePrefab(self.tipTemplate, function(prefab)
        -- 初始化展开的tip Go
        self.tipPrefabPrepare = true
        self.tipObj = CS.UnityEngine.GameObject.Instantiate(prefab, self.content)
        self.tipObj:SetActive(false)
        self.CircularScrollView.m_ExpandTips = self.tipObj
        self:CheckAllPrefabPrepared()
    end)
end

---@private
function CircularExpandTipFactory:CheckAllPrefabPrepared()
    if self.prefabPrepare and self.tipPrefabPrepare then
        local refreshItemCallback = function(cell, index)
            self:RefreshItemCallBack(cell, index) --调用super里的函数
        end

        local removeItemCallBack = function(cell, index)
            self:OnRemoveItemCallBack(cell) --调用super里的函数
        end

        local onExpandTipClickCallBack = function(cell, index)
            self:ExpandTipClickCallBack(cell, index)
        end

        self.CircularScrollView:Init()
        self.CircularScrollView.m_RefreshItemCallBack = refreshItemCallback
        self.CircularScrollView.m_RemoveItemCallBack = removeItemCallBack
        self.CircularScrollView.m_FuncOnClickCallBack = onExpandTipClickCallBack
        self:UpdateView()
    end
end

---@public
function CircularExpandTipFactory:UpdateView(JumpStart)
    if not self.tipPrefabPrepare then
        return
    end

    self:UnBindTipItem()
    CircularExpandTipFactory.super.UpdateView(self, JumpStart)
end

---@private
function CircularExpandTipFactory:ExpandTipClickCallBack(cell, index)
    local bExpandTip = self.lastClickTipIdx ~= index
    self:UnBindTipItem()

    if (bExpandTip) then
        self.binding:BindItem(self.tipObj, index)
        self.lastClickTipIdx = index
    end
end

---@private
function CircularExpandTipFactory:UnBindTipItem()
    if (self.lastClickTipIdx > 0) then
        self.binding:UnbindItem(self.tipObj)
        self.lastClickTipIdx = -1
    end
end

return CircularExpandTipFactory