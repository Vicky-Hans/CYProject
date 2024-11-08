---
--- 文件名称:  DefaultDynamicViewFactory
--- 创建者:    yuancan.
--- 创建时间:  2021/7/19 3:46 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local LuaBindingTargetWrapper = CS.UIFramework.LuaBindingTargetWrapper
local PrefabManagerLua = CS.DH.UIFramework.PrefabManagerLua
local List = require("Common/List")

--模块
---@class DefaultDynamicViewFactory
local DefaultDynamicViewFactory = bpcClass("DefaultDynamicViewFactory")

--[[--
构造函数
@param #table self
@param #table t 初始化参数
]]
function DefaultDynamicViewFactory:ctor(pool)
    self.pool = pool
    -- 0 horizontal 1 vertical
    self.config = {}
    self.DirectionType = {
        Horizontal = 0,
        Vertical = 1,
    }
    self.direction = self.DirectionType.Vertical
    ---@type List<int>
    self.indexList = List:New("int")
    ---@type List<GameOject>
    self.cachedView = List:New("GameObject")
    self.dirtyFlag = false
    ---@type Transform
    self.content = nil
    ---@type GameObject
    self.viewTemplate = nil
    self.prefabPrepare = false
    ---@type function callback function for owner
    self.onPrefabPrepared = nil
end

---@return DefaultDynamicViewFactory
function DefaultDynamicViewFactory:WithHorizontal()
    self.direction = self.DirectionType.Horizontal
    return self
end

---@param binding DynamicCollectionBinding
---@return DefaultDynamicViewFactory
function DefaultDynamicViewFactory:Init(binding, config)
    self.scrollRect = self:GetComponentInParent(self.content, "ScrollRect")
    self.onScrollChanged = function(value)
        self:OnScrollChanged(value)
    end
    self.dirtyFlag = false
    self.scrollRect.onValueChanged:AddListener(self.onScrollChanged)
    self.binding = binding
    self.config = config
    return self
end

---@pivate
function DefaultDynamicViewFactory:GetComponentInParent(target, type)
    return target:GetComponentInParent(type) --调用静态函数避免多次与lua的交互
end

---@public
function DefaultDynamicViewFactory:Release()
    if not bpcIsNull(self.scrollRect) then
        self.scrollRect.onValueChanged:RemoveListener(self.onScrollChanged)
    end

    self.cachedView:ForEach(function(item)
        CS.UnityEngine.GameObject.Destroy(item)
    end)
    self.binding = nil
    self.cachedView = nil
    self.scrollRect = nil
    self.indexList = nil
end

function DefaultDynamicViewFactory:PrepareTemplate()
    PrefabManagerLua.PreparePrefab(self.viewTemplate, function(prefab)
        self.viewTemplate = prefab
        self.prefabPrepare = true
        --- callback for update ui
        if (self.onPrefabPrepared) then
            self.onPrefabPrepared()
        end
    end)
end

---@pivate
function DefaultDynamicViewFactory:LateUpdate()
    if not self.prefabPrepare then
        return
    end

    if self.dirtyFlag then
        self:UpdateView()
        self.dirtyFlag = false
    end
end

---@pivate
function DefaultDynamicViewFactory:OnScrollChanged(value)
    self.dirtyFlag = true
end

---@public
function DefaultDynamicViewFactory:UpdateView()
    if self.binding == nil or not self.binding:IsBound() then
        return
    end

    self.binding:UpdateItems()
    self:RefreshView()
end

---@public
function DefaultDynamicViewFactory:GetDynamicList()
    self.indexList:Clear()
    if bpcIsNull(self.scrollRect) then
        return self.indexList
    end

    -- get rect
    local rect = self.scrollRect.viewport.rect
    local start
    local endIndex

    if self.binding:Count() == 0 then
        return self.indexList
    end

    if self.direction == self.DirectionType.Vertical then
        local t = self.content.anchoredPosition.y
        -- calculate visible range
        start = math.floor(t / self.config.itemSize)
        endIndex = math.ceil((t + rect.height) / self.config.itemSize)
    else
        local t = -self.content.anchoredPosition.x
        -- calculate visible range
        start = math.floor(t / self.config.itemSize)
        endIndex = math.ceil((t + rect.width) / self.config.itemSize)
    end

    -- clamp index 
    local maxIndex = self.binding:Count() - 1
    start = math.clamp(start - 1, 0, maxIndex)
    endIndex = math.clamp(endIndex, 0, maxIndex)

    self.startIndex = start

    -- convert c# index to lua index
    for index = start + 1, endIndex + 1 do
        if self.binding.collectionView ~= nil then
            self.indexList:Add(self.binding.collectionView.indexedList[index])
        else
            self.indexList:Add(index)
        end
    end

    return self.indexList
end

---@public
function DefaultDynamicViewFactory:RefreshView()
    if bpcIsNull(self.content) then
        return
    end
    -- calculate content rectangle size
    local size = self.binding:Count() * self.config.itemSize
    local newContentSize
    if self.direction == self.DirectionType.Vertical then
        newContentSize = CS.UnityEngine.Vector2(0, size)
    else
        newContentSize = CS.UnityEngine.Vector2(size, 0)
    end

    if self.content.sizeDelta ~= newContentSize then
        self.content.sizeDelta = newContentSize
    end

    -- get content rectangle
    local rect = self.content.rect
    -- position view item
    self.binding.bindingDictionary:ForEach(function(key, item)
        -- lua index start with 1
        -- need to resume to index start with 0 to adapt c# layout code
        local index = item.sortedIndex - 1 + self.startIndex

        if self.direction == self.DirectionType.Vertical then
            x = rect.width / 2
            y = -index * self.config.itemSize - self.config.itemSize / 2
            width = rect.width
            height = self.config.itemSize
        else
            x = index * self.config.itemSize + self.config.itemSize / 2
            y = rect.height / 2
            width = self.config.itemSize
            height = rect.height
        end

        local rt = item.view.transform
        local newPos = CS.UnityEngine.Vector2(x, y)
        if newPos ~= rt.anchoredPosition then
            rt.anchoredPosition = newPos
        end

        local newSize = CS.UnityEngine.Vector2(width, height)
        if newSize ~= rt.sizeDelta then
            rt.sizeDelta = newSize
        end
    end) -- end of bindingDictionary foreach
end

function DefaultDynamicViewFactory:ReleaseViewItem(view)
    -- already destroyed
    if bpcIsNull(view) then
        return
    end

    if not bpcIsNull(self.pool) then
        self.pool:ReleaseViewItem(view)
    else
        self.cachedView:Add(view)
        if view.activeSelf then
            view:SetActive(false)
        end
    end
end

function DefaultDynamicViewFactory:CreateViewItem()
    local newItem
    if not bpcIsNull(self.pool) then
        newItem = self.pool:GetViewItem()
    else
        if self.cachedView:Count() == 0 then
            -- no valid view item in cached pool
            -- create a new view item
            newItem = CS.UnityEngine.GameObject.Instantiate(self.viewTemplate)
        else
            -- get a view item from pool
            newItem = self.cachedView[1]
            self.cachedView:RemoveAt(1)
        end

        if not newItem.activeSelf then
            newItem:SetActive(true)
        end
    end -- end of view item creating

    LuaBindingTargetWrapper.SetParent(newItem, self.content, false)

    return newItem
end

function math.clamp(value, min, max)
    assert(value ~= nil and min ~= nil and max ~= nil, "[math.clamp] with nil arguments")
    if value < min then
        return min
    end

    if value > max then
        return max
    end

    return value
end

return DefaultDynamicViewFactory