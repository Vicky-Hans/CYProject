---
--- 文件名称:  CircularFlipFactory
--- 创建者:    nieshihai
--- 创建时间:  2021/9/22 16:26
-------------------------------------------------------------------
--- 功能描述：
--- 扩展Tip的循环列表。对应使用 FlipPageCircularScrollView 类。
--- ScrollView和item的pivot必须用左上角的方式

require("Common/System")
local CircularDynamicViewFactory = require("Support/CircularDynamicViewFactory")
---
--模块
---@class CircularFlipFactory:CircularDynamicViewFactory
local CircularFlipFactory = bpcClass("CircularFlipFactory", CircularDynamicViewFactory)

--[[--
构造函数
]]
function CircularFlipFactory:ctor(content, prefabTemplate, navNormalPrefabTemplate, navSelectedPrefabTemplate)
    CircularFlipFactory.super.ctor(self, content, prefabTemplate)
    self.navNormalPrefabTemplate = navNormalPrefabTemplate
    self.navSelectedPrefabTemplate = navSelectedPrefabTemplate
    self.navNormalObjPrepare = false
    self.navSelectedObjPrepare = false
end

---@param binding CircularDynamicCollectionBinding
---@return CircularFlipFactory
function CircularFlipFactory:Init(binding)
    self.CircularScrollView = self.content:GetComponentInParent("DH.UIFramework.FlipPageCircularScrollView")
    self.binding = binding
    return self
end

---@public
function CircularFlipFactory:Release()
    CircularFlipFactory.super.Release(self)
    self.navNormalPrefabTemplate = nil
    self.navSelectedPrefabTemplate = nil
end

function CircularFlipFactory:CheckPrefabTemplate(prefab, preparedFunc)
    if type(prefab) == 'userdata' then
        preparedFunc(prefab)
    else
        PrefabManagerLua.PreparePrefab(prefab, preparedFunc)
    end
end

function CircularFlipFactory:PrepareTemplate()
    local templateType = type(self.prefabTemplate)
    if templateType == 'userdata' or templateType == "function" then
        -- 初始化基本的prefab
        self.prefabPrepare = true
        if templateType == 'userdata' then
            self.CircularScrollView.m_CellGameObject = self.prefabTemplate
        end
        self:CheckAllPrefabPrepared()
    else
        self:CheckPrefabTemplate(self.prefabTemplate, function(prefab)
            -- 初始化基本的prefab
            self.prefabPrepare = true
            self.CircularScrollView.m_CellGameObject = prefab
            self:CheckAllPrefabPrepared()
        end)
    end

    if self.navNormalPrefabTemplate and self.navSelectedPrefabTemplate then
        self:CheckPrefabTemplate(self.navNormalPrefabTemplate, function(prefab)
            -- 初始化基本的prefab
            self.navNormalObjPrepare = true
            self.CircularScrollView.m_NavNormalPrefab = prefab
            self:CheckAllPrefabPrepared()
        end)
        
        self:CheckPrefabTemplate(self.navSelectedPrefabTemplate, function(prefab)
            -- 初始化基本的prefab
            self.navSelectedObjPrepare = true
            self.CircularScrollView.m_NavSelectedPrefab = prefab
            self:CheckAllPrefabPrepared()
        end)
    else
        self.navNormalObjPrepare = true
        self.navSelectedObjPrepare = true
        self:CheckAllPrefabPrepared()
    end
end

function CircularFlipFactory:IsAllPrefabPrepared()
    return self.prefabPrepare and self.navNormalObjPrepare and self.navSelectedObjPrepare
end

---@private
function CircularFlipFactory:CheckAllPrefabPrepared()
    if self:IsAllPrefabPrepared() then
        local refreshItemCallback = function(cell, index)
            self:RefreshItemCallBack(cell, index) --调用super里的函数
        end

        local removeItemCallBack = function(cell, index)
            self:OnRemoveItemCallBack(cell) --调用super里的函数
        end
        
        self.CircularScrollView:Init()

        if type(self.prefabTemplate) == "function" then
            local getPrefabTemplate = function(index)
                return self:GetItemPrefab(index)
            end

            self.CircularScrollView.m_FuncGetPrefabTemplate = getPrefabTemplate
        end
        
        self.CircularScrollView.m_RefreshItemCallBack = refreshItemCallback
        self.CircularScrollView.m_RemoveItemCallBack = removeItemCallBack
        self:UpdateView()
    end
end

---@public
function CircularFlipFactory:UpdateView(JumpStart)
    if not self:IsAllPrefabPrepared() then
        return
    end

    CircularFlipFactory.super.UpdateView(self, JumpStart)
end

function CircularFlipFactory:AddItems(curCount)
    self:UpdateView()
end

function CircularFlipFactory:InsertItems(index, insertCount)
    self:UpdateView()
end

return CircularFlipFactory