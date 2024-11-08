---
--- 文件名称:  DragEventDispatcher
--- 创建者:    nieshihai
--- 创建时间:  2022/6/23 15:57
-------------------------------------------------------------------
--- 功能描述：
--- 目前只支持竖起方向的事件转发
---

local DragDirection = require("Support/DragDirection")

--模块
---@class DragEventDispatcher
local DragEventDispatcher = bpcClass("DragEventDispatcher")

---ctor
---@param dragEventListener DH.UIFramework.DragEventTriggerListener
---@param targetScrollRect UnityEngine.UI.ScrollRect @事件分发的目标
---@param targetDir DragDirection @在目标方向拖动时才转发
---@param selfScrollRect UnityEngine.UI.ScrollRect @本身的ScrollRect
function DragEventDispatcher:ctor(dragEventListener, targetScrollRect, targetDir, selfScrollRect, targetScrollRectPriority)
    if bpcIsNull(dragEventListener) and not bpcIsNull(selfScrollRect) then
        dragEventListener = selfScrollRect:GetComponent("DH.UIFramework.DragEventTriggerListener")
    end

    if targetDir == DragDirection.Horizontal then
        error("DragEventDispatcher 水平的转发暂时还未支持，请联系中台")    
    end
    
    self.dragEventListener = dragEventListener
    self.targetScrollRect = targetScrollRect
    self.targetDir = targetDir
    self.selfScrollRect = selfScrollRect
    self.targetScrollRectPriority = targetScrollRectPriority

    --- 自身没有ScrollRect或者自身的ScrollRect的拖动方向与转发的目标方向不一致，就可以直接转发
    self.canDirectDispatcher = bpcIsNull(self.selfScrollRect) or
            self.targetDir == DragDirection.Vertical and self.selfScrollRect.horizontal or
            self.targetDir == DragDirection.Horizontal and self.selfScrollRect.vertical
    
    self.targetValid = not bpcIsNull(self.dragEventListener) and not bpcIsNull(self.targetScrollRect)
    --- 只有有分发的目标才AddListener
    if self.targetValid then
        self:AddListener()
        self.targetScrollViewRect = targetScrollRect.viewport.rect
        self.targetScrollContentRt = targetScrollRect.content
    end

    if not bpcIsNull(selfScrollRect) then
        self.selfScrollViewRect = selfScrollRect.viewport.rect
        self.selfScrollContentRt = selfScrollRect.content
    end
end

--- 注册拖拽事件的句柄
function DragEventDispatcher:AddListener()
    self.beginDragHandle = function(eventData)
        self:OnBeginDrag(eventData)
    end
    self.dragEventListener.BeginDragHandle = self.beginDragHandle

    self.onDragHandle = function(eventData)
        self:OnDrag(eventData)
    end
    self.dragEventListener.OnDragHandle = self.onDragHandle

    self.endDragHandle = function(eventData)
        self:OnEndDrag(eventData)
    end
    self.dragEventListener.EndDragHandle = self.endDragHandle
end

function DragEventDispatcher:RemoveListener()
    self.dragEventListener.BeginDragHandle = nil
    self.dragEventListener.OnDragHandle = nil
    self.dragEventListener.EndDragHandle = nil
end

--- 开始拖拽
function DragEventDispatcher:OnBeginDrag(eventData)
    local curPos = eventData.position
    local pressPos = eventData.pressPosition
    local dragDir = self:GetCurDragDirection(curPos, pressPos)

    --- 拖动的方向与分发的目标相同
    if dragDir == self.targetDir then
        --- 自身都没有ScrollRect，直接分发
        if self.canDirectDispatcher then
            self:TargetBeginDrag(eventData)
        end
    end
    
    self.lastPos = curPos
end

--- 拖拽中
function DragEventDispatcher:OnDrag(eventData)
    if self.scrollingTarget then
        self.targetScrollRect:OnDrag(eventData)
    end
    
    local curPos = eventData.position
    local dragDir = self:GetCurDragDirection(curPos, self.lastPos)

    --- 说明目标方向与自己ScrollView的滑动方向相同，需要做特殊处理
    if not self.canDirectDispatcher then
        if dragDir == self.targetDir then
            local canDispatch = false
            
            if self.targetDir == DragDirection.Vertical and curPos.y > self.lastPos.y then
                --- 向上
   
                if self.targetScrollRectPriority then
                    canDispatch = not self:IsScrollRectAtEndPos(self.targetDir, self.targetScrollContentRt, self.targetScrollViewRect)
                    if canDispatch  then
                        self.selfScrollRect.enabled = false
                        self.selfScrollContentRt:SetAnchoredPosition(0, 0)
                    else
                        self.selfScrollRect.enabled = true
                    end
                else
                    canDispatch = self:IsScrollRectAtEndPos(self.targetDir, self.selfScrollContentRt, self.selfScrollViewRect)
                end
            elseif self.targetDir == DragDirection.Vertical and curPos.y < self.lastPos.y then
                --- 向下
                canDispatch = self:IsScrollRectAtBeginPos(self.targetDir, self.selfScrollContentRt)
            end

            if canDispatch then
                if self.scrollingTarget then
                    self.targetScrollRect:OnDrag(eventData)
                else
                    self:TargetBeginDrag(eventData)
                end
            else
                self:TargetScrollViewEndDrag(eventData)
            end
        end
    end
    
    self.lastPos = curPos
end

--- 结束拖拽
function DragEventDispatcher:OnEndDrag(eventData)
    self:TargetScrollViewEndDrag(eventData)
end

---IsScrollRectAtBeginPos
---@param dir DragDirection
---@param scrollContentRt UnityEngine.RectTransform
function DragEventDispatcher:IsScrollRectAtBeginPos(dir, scrollContentRt)
    local contentPos = scrollContentRt.anchoredPosition
    if dir == DragDirection.Horizontal then
        return contentPos.x <= 10 --- 容错
    end

    return contentPos.y <= 10 --- 容错
end

---IsScrollRectAtBeginPos
---@param dir DragDirection
---@param scrollContentRt UnityEngine.RectTransform @内容的RectTransform
---@param scrollViewRect UnityEngine.Rect @视窗的rect
function DragEventDispatcher:IsScrollRectAtEndPos(dir, scrollContentRt, scrollViewRect)
    local contentPos = scrollContentRt.anchoredPosition
    local contentRt = scrollContentRt.rect
    
    if dir == DragDirection.Horizontal then
        local viewWidth = scrollViewRect.width
        return contentRt.width + contentPos.x < viewWidth
    end

    local viewHeight = scrollViewRect.height
    return contentRt.height -contentPos.y <= viewHeight
end

function DragEventDispatcher:GetCurDragDirection(lastPos, curPos)
    local horizontal = math.abs(curPos.x - lastPos.x)
    local vertical = math.abs(curPos.y - lastPos.y)
    local dragDir = DragDirection.Horizontal
    if vertical > horizontal then
        dragDir = DragDirection.Vertical
    end
    
    return dragDir
end

function DragEventDispatcher:TargetBeginDrag(eventData)
    self.targetScrollRect:OnBeginDrag(eventData)
    self.scrollingTarget = true
end

function DragEventDispatcher:TargetScrollViewEndDrag(eventData)
    if self.scrollingTarget then
        self.scrollingTarget = false
        self.targetScrollRect:OnEndDrag(eventData)
    end
end

function DragEventDispatcher:Release()
    if self.targetValid then
        self:RemoveListener()
    end
end

return DragEventDispatcher