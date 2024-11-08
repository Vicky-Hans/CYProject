---
--- 文件名称:  CircularGroupFactory
--- 创建者:    nieshihai
--- 创建时间:  2021/9/24 18:21
-------------------------------------------------------------------
--- 功能描述：
--- 有分组扩展的循环列表。对应使用 GroupExpandCircularScrollView 类。
--- ScrollView和item的pivot必须用左上角的方式
--- 这个类就不继承 CircularDynamicViewFactory 了，因为使用方法以及函数内容基本完全不同

require("Common/System")
local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
--模块
---@class CircularGroupFactory
local CircularGroupFactory = bpcClass("CircularGroupFactory")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function CircularGroupFactory:ctor(content, prefabTemplate, expandButtonPrefabTemplate)
    ---@type Transform
    self.content = content
    self.prefabTemplate = prefabTemplate
    self.prefabPrepare = false
    self.expandButtonTemplate = expandButtonPrefabTemplate
    self.expandButtonPrefabPrepare = false
    ---@type CircularDynamicCollectionBinding
    self.binding = nil
end

---@param binding CircularDynamicCollectionBinding
---@return CircularDynamicViewFactory
function CircularGroupFactory:Init(binding)
    self.CircularScrollView = self.content:GetComponentInParent("DH.UIFramework.GroupExpandCircularScrollView")
    self.binding = binding
    return self
end

---@public
function CircularGroupFactory:Release()
    self.prefabTemplate = nil
    self.expandButtonTemplate = nil
    self.content = nil
    self.binding = nil
    self.CircularScrollView = nil
end

function CircularGroupFactory:PrepareTemplate()
    PrefabManagerLua.PreparePrefab(self.prefabTemplate, function(prefab)
        self.prefabTemplate = nil
        -- 初始化基本的prefab
        self.prefabPrepare = true
        self.CircularScrollView.m_CellGameObject = prefab
        self:CheckAllPrefabPrepared()
    end)

    PrefabManagerLua.PreparePrefab(self.expandButtonTemplate, function(prefab)
        self.expandButtonTemplate = nil
        self.expandButtonPrefabPrepare = true
        self.CircularScrollView.m_ExpandButton = prefab
        self:CheckAllPrefabPrepared()
    end)
end

---@private
function CircularGroupFactory:CheckAllPrefabPrepared()
    if self.prefabPrepare and self.expandButtonPrefabPrepare then
        local refreshItemCallback = function(expandButton, itemObj, buttonNum, itemNum)
            self:RefreshItemCallBack(expandButton, itemObj, buttonNum, itemNum)
        end

        local removeItemCallBack = function(cell, index)
            self:OnRemoveItemCallBack(cell) --调用super里的函数
        end

        self.CircularScrollView:Init()
        self.CircularScrollView.m_FuncCallBackFunc = refreshItemCallback
        self.CircularScrollView.m_RemoveItemCallBack = removeItemCallBack
        self:UpdateView()
    end
end

---@public
function CircularGroupFactory:UpdateView(bJumpStart)
    if self.binding == nil or not self.binding:IsBound() then
        return
    end

    if not self.prefabPrepare or not self.expandButtonPrefabPrepare then
        return
    end

    local count = self.binding:GroupCount()

    if count == 0 then
        self.CircularScrollView:Clear()
    else
        local countStr = ""
        local curIdx = 0
        
        self.binding:GroupForEach(function(groupItem)
            if curIdx == 0 then
                countStr = countStr..groupItem.indexedList:Count()
            else
                countStr = countStr.."|"..groupItem.indexedList:Count()
            end
            
            curIdx = curIdx + 1
        end)

        self.CircularScrollView:ShowList(countStr)
    end
end

---@public
function CircularGroupFactory:ClearList()
    if  not bpcIsNull(self.CircularScrollView) then
        self.CircularScrollView:Clear()
    end
end

---@private
function CircularGroupFactory:RefreshItemCallBack(expandButton, itemObj, buttonNum, itemNum)
    if itemNum > 0 then
        self.binding:BindItem(itemObj, self.binding:GetItemIndexInGroup(buttonNum, itemNum))
    else
        self.binding:BindGroupButtonItem(expandButton, buttonNum)
    end
end

---@private
function CircularGroupFactory:OnRemoveItemCallBack(cell)
    self.binding:UnbindItem(cell)
end

return CircularGroupFactory