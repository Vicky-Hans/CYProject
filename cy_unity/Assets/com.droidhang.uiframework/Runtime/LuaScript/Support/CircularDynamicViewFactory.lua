---
--- 文件名称:  CircularDynamicViewFactory
--- 创建者:    nieshihai
--- 创建时间:  2021/9/16 10:55
-------------------------------------------------------------------
--- 功能描述：
--- 默认模式的循环列表，只有循环的功能，使用对应的 UICircularScrollView C#类
--- ScrollView和item的pivot必须用左上角的方式

require("Common/System")
local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
--模块
---@class CircularDynamicViewFactory
local CircularDynamicViewFactory = bpcClass("CircularDynamicViewFactory")

---@alias Handler fun(type: table):userdata
---@param prefabTemplate string|userdata|Handler @prefab，当是Handler的时候需要根据source返回对应的GameObject
function CircularDynamicViewFactory:ctor(content, prefabTemplate)
    ---@type userdata @生成的子item的parent transform
    self.content = content
    self.prefabTemplate = prefabTemplate
    self.prefabPrepare = false
    ---@type function callback function for owner
    self.onPrefabPrepared = nil
    ---@type CircularDynamicCollectionBinding
    self.binding = nil
end

---@param binding CircularDynamicCollectionBinding
---@return CircularDynamicViewFactory
function CircularDynamicViewFactory:Init(binding)
    self.CircularScrollView = self.content:GetComponentInParent("DH.UIFramework.UICircularScrollView")
    self.binding = binding
    self.scrollViewDirection = self.CircularScrollView.m_Direction
    return self
end

---@public
function CircularDynamicViewFactory:Release()
    self.content = nil
    self.binding = nil
    self.CircularScrollView = nil
    self.onPrefabPrepared = nil
end

function CircularDynamicViewFactory:OnTemplatePrepared(prefab)
    self.prefabPrepare = true

    if type(prefab) ~= "function" then
        self.CircularScrollView.m_CellGameObject = prefab
    end

    local refreshItemCallback = function(cell, index)
        self:RefreshItemCallBack(cell, index)
    end

    local removeItemCallBack = function(cell, index)
        self:OnRemoveItemCallBack(cell)
    end

    local getItemAdaptingSize = function(index)
        return self:GetItemAdaptingSize(index)
    end

    self.CircularScrollView:Init()

    if type(prefab) == "function" then
        local getPrefabTemplate = function(index)
            return self:GetItemPrefab(index)
        end

        self.CircularScrollView.m_FuncGetPrefabTemplate = getPrefabTemplate
    end
    self.CircularScrollView.m_RefreshItemCallBack = refreshItemCallback
    self.CircularScrollView.m_RemoveItemCallBack = removeItemCallBack
    if self.CircularScrollView.m_AdaptingSize then
        self.sourceItemSizeChanged = function(index, size)
            if not bpcIsNull(self.CircularScrollView) then
                self.CircularScrollView:OneCellSizeChanged(index, size)
            end
        end

        self.CircularScrollView.m_GetItemAdaptingSize = getItemAdaptingSize
    end

    self:UpdateView()
end

function CircularDynamicViewFactory:CheckPrefabTemplate(prefab, preparedFunc)
    if type(prefab) == 'userdata' then
        preparedFunc(prefab)
    else
        PrefabManagerLua.PreparePrefab(prefab, preparedFunc)
    end
end

function CircularDynamicViewFactory:PrepareTemplate()
    local templateType = type(self.prefabTemplate)
    if templateType == 'userdata' or templateType == "function" then
        self:OnTemplatePrepared(self.prefabTemplate)
    else
        PrefabManagerLua.PreparePrefab(self.prefabTemplate, function(prefab)
            self:OnTemplatePrepared(prefab)
        end)
    end
end

---@public
function CircularDynamicViewFactory:UpdateView(bJumpStart)
    if self.binding == nil or not self.binding:IsBound() then
        return
    end

    if not self.prefabPrepare then
        return
    end

    local count = self.binding:Count()
    if bJumpStart == nil then
        self.CircularScrollView:ShowList(count, true)
    else
        self.CircularScrollView:ShowList(count, bJumpStart)
    end
end

---@public
function CircularDynamicViewFactory:ClearList()
    if not bpcIsNull(self.CircularScrollView) then
        self.CircularScrollView:Clear()
    end
end

---@private
function CircularDynamicViewFactory:OnRemoveItemCallBack(cell)
    self.binding:UnbindItem(cell)
end

---@private
function CircularDynamicViewFactory:RefreshItemCallBack(cell, index)
    self.binding:BindItem(cell, index)
end

---@private
function CircularDynamicViewFactory:GetItemAdaptingSize(index)
    local size, itemSource = self.binding:GetItemSize(index, self.scrollViewDirection == CS.DH.UIFramework.e_Direction.Vertical)

    if self.sourceItemSizeChanged ~= nil then
        itemSource.circularItemIdx = index
        itemSource.onItemSizeChangedCallback = self.sourceItemSizeChanged
    end

    return size
end

---@private
--- 根据source来获取不同的prefab，需要有prefabHandler
function CircularDynamicViewFactory:GetItemPrefab(index)
    local temType = type(self.prefabTemplate)
    if temType == "function" then
        local source = self.binding:GetItemSource(index)
        return self.prefabTemplate(source)
    elseif temType == 'userdata' then
        return self.prefabTemplate
    end

    return nil
end

--- 以下函数是在不使用DynamicCollectionView时调用 begin
function CircularDynamicViewFactory:AddItems(curCount)
    if self.CircularScrollView ~= nil then
        self.CircularScrollView:AddItems(curCount)
    end
end

function CircularDynamicViewFactory:InsertItems(index, insertCount)
    if self.CircularScrollView ~= nil then
        self.CircularScrollView:InsertItems(index, insertCount)
    end
end
--- 以上函数是在不使用DynamicCollectionView时调用 end

return CircularDynamicViewFactory