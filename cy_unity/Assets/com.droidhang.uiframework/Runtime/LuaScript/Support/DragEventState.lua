---
--- 文件名称:  DragEventDispatcher
--- 创建者:    weiyinlei
--- 创建时间:  2022/9/8 15:57
-------------------------------------------------------------------
--- 功能描述：
--- 目前只支持竖起方向的事件转发
---

--模块
---@class DragEventState
local DragEventState = bpcClass("DragEventState")

---ctor
---@param dragEventListener DH.UIFramework.DragEventTriggerListener
---@param selfScrollRect UnityEngine.UI.ScrollRect @本身的ScrollRect
function DragEventState:ctor(dragEventListener, selfScrollRect)
    if bpcIsNull(dragEventListener) and not bpcIsNull(selfScrollRect) then
        dragEventListener = selfScrollRect:GetComponent("DH.UIFramework.DragEventTriggerListener")
    end
    self.dragEventListener = dragEventListener
    self.targetScrollRect = selfScrollRect
    self:AddListener()
end

--- 注册拖拽事件的句柄
function DragEventState:AddListener()
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

function DragEventState:RemoveListener()
    self.dragEventListener.BeginDragHandle = nil
    self.dragEventListener.OnDragHandle = nil
    self.dragEventListener.EndDragHandle = nil
end

--- 开始拖拽
function DragEventState:OnBeginDrag(eventData)
    self.scrollingTarget = true
end

--- 拖拽中
function DragEventState:OnDrag(eventData)
    self.scrollingTarget = true
end

--- 结束拖拽
function DragEventState:OnEndDrag(eventData)
    if self.scrollingTarget then
        self.scrollingTarget = false
    end
end


function DragEventState:Release()
    self:RemoveListener()
end

return DragEventState