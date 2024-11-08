---
--- 文件名称:  HorizontalMove
--- 创建者:    lizhenxiong
--- 创建时间:  2022/1/15 16:43
-------------------------------------------------------------------
--- 功能描述：兼容ScrollView Vertical的滑块移动组件
---
---

--模块
---@class HorizontalMove
local HorizontalMove = bpcClass("HorizontalMove")

--- 当前状态
local DragState = {
    --- 默认状态
    None = 1,
    --- 移动中
    Moving = 2,
}

--[[--
构造函数
]]
function HorizontalMove:ctor(horizontalMoveCS, ownerChatItem)
    self.OwnerChatItem = ownerChatItem
    --- 当前状态
    self.State = DragState.None
    --- 需要移动的对象
    ---@type UnityEngine.Transform
    self.RectTransform = horizontalMoveCS.RectTransform

    --- 起始位置
    self.StartPosition = nil

    --- 最后停止位置
    self.StopPosition = nil

    --- 最大可移动的距离
    self.MaxMovePosition = -120

    self.HorizontalMoveCS = horizontalMoveCS

    self.scrollView = self.RectTransform:GetComponentInParent("UnityEngine.UI.ScrollRect")
    self.checkParent = not bpcIsNull(self.scrollView)

    self:AddListener()
end

--- 开始拖拽
function HorizontalMove:OnBeginDrag(eventData)
    local curPos = eventData.position
    local pressPos = eventData.pressPosition
    local horizontal = math.abs(curPos.x - pressPos.x)
    local vertical = math.abs(curPos.y - pressPos.y)

    if self.checkParent and vertical > horizontal then
        self.scrollView:OnBeginDrag(eventData)
        self.scrollParent = true
    else
        --- 记录开始位置
        self.StartPosition = curPos
        self.State = DragState.Moving
        self.OwnerChatItem.State = self.State
    end
end

--- 拖拽中
function HorizontalMove:OnDrag(eventData)
    if self.checkParent and self.scrollParent then
        self.scrollView:OnDrag(eventData)
    else
        local position = self.RectTransform.localPosition
        --- 根据拖拽的位置偏移更新Item的位置
        local offSetX = position.x + eventData.delta.x
        --- 边界判断
        self.RectTransform.localPosition = DH.Vector2(math.clamp(offSetX, self.MaxMovePosition, 0), position.y)

        --- 状态判断如果是默认状态，则滑到左边
        if eventData:IsPointerMoving() then
            self.State = DragState.Moving
        else
            self.State = DragState.None
        end
        self.OwnerChatItem.State = self.State
    end
end

--- 结束拖拽
function HorizontalMove:OnEndDrag(eventData)
    if self.checkParent and self.scrollParent then
        self.scrollParent = false
        self.scrollView:OnEndDrag(eventData)
    else
        local position = self.RectTransform.localPosition
        --- 边界值判断
        if position.x < self.MaxMovePosition and position.x > 0 then
            self.RectTransform.localPosition = DH.Vector2(0, position.y)
        end


        self.StopPosition = eventData.position

        if self.StartPosition.x > self.StopPosition.x then
            self.RectTransform.anchoredPosition = DH.Vector2(self.MaxMovePosition, 0)
        else
            self.RectTransform.anchoredPosition = DH.Vector2(0, 0)
        end


        self.StopPosition = eventData.position
        self.State = DragState.None
        self.OwnerChatItem.State = self.State
    end
end

--- 注册拖拽事件的句柄
function HorizontalMove:AddListener()

    self.BeginDragHandle = function(eventData)
        self:OnBeginDrag(eventData)
    end
    self.HorizontalMoveCS.BeginDragHandle = self.BeginDragHandle

    self.DragHandle = function(eventData)
        self:OnDrag(eventData)
    end
    self.HorizontalMoveCS.DragHandle = self.DragHandle

    self.EndDragHandle = function(eventData)
        self:OnEndDrag(eventData)
    end
    self.HorizontalMoveCS.EndDragHandle = self.EndDragHandle
end

function HorizontalMove:RemoveListener()
    self.HorizontalMoveCS.BeginDragHandle = nil
    self.HorizontalMoveCS.DragHandle = nil
    self.HorizontalMoveCS.EndDragHandle = nil
end

function HorizontalMove:OnDestroy()
    self:RemoveListener()
end

return HorizontalMove