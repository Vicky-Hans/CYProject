local Event = class("Event")

local EXPORTED_METHODS = {
    "AddEventListener",
    "DispatchEvent",
    "RemoveEventListener",
    "RemoveEventListenersByTag",
    "RemoveEventListenersByEvent",
    "RemoveAllEventListeners",
    "HasEventListener",
    "DumpAllEventListeners",
}
local printInfo = bpcPrintf
local DEBUG = 0

function Event:init_()
    self.target_ = nil
    self.listeners_ = {}
    self.nextListenerHandleIndex_ = 0
end

local function setmethods(target, component, methods)
    for _, name in ipairs(methods) do
        local method = component[name]
        target[name] = function(__, ...)
            return method(component, ...)
        end
    end
end

local function unsetmethods(target, methods)
    for _, name in ipairs(methods) do
        target[name] = nil
    end
end

function Event:bind(target)
    self:init_()
    setmethods(target, self, EXPORTED_METHODS)
    self.target_ = target
end

function Event:unbind(target)
    unsetmethods(target, EXPORTED_METHODS)
    self:init_()
end

function Event:on(eventName, listener, tag, priority)
    assert(type(eventName) == "string" and eventName ~= "",
        "Event:addEventListener() - invalid eventName")
    eventName = string.upper(eventName)
    if self.listeners_[eventName] == nil then
        self.listeners_[eventName] = {}
    end

    self.nextListenerHandleIndex_ = self.nextListenerHandleIndex_ + 1
    local handle = tostring(self.nextListenerHandleIndex_)
    tag = tag or ""
    priority = priority or 0
    self.listeners_[eventName][handle] = {listener, tag, priority}

    if DEBUG > 1 then
        printInfo("%s [Event] addEventListener() - event: %s, handle: %s, tag: \"%s\"",
                  tostring(self.target_), eventName, handle, tostring(tag))
    end

    return self.target_, handle
end

Event.AddEventListener = Event.on

function Event:DispatchEvent(event)
    event.name = string.upper(tostring(event.name))
    local eventName = event.name
    if DEBUG > 1 then
        printInfo("%s [Event] dispatchEvent() - event %s", tostring(self.target_), eventName)
    end

    if self.listeners_[eventName] == nil then return end
    event.target = self.target_
    event.stop_ = false
    event.stop = function(self)
        self.stop_ = true
    end

    --根据优先级排序
    local listenerList = {}
    for handle, listener in pairs(self.listeners_[eventName]) do
        local info = {handle=handle, listener=listener}
        table.insert(listenerList, info)
    end
    table.sort(listenerList, function( a, b )
        local priA = a.listener[3]
        local priB = b.listener[3]
        return priA < priB
    end)

    for i, info in ipairs(listenerList) do
        local handle = info.handle
        local listener = info.listener
        if DEBUG > 1 then
            printInfo("%s [Event] dispatchEvent() - dispatching event %s to listener %s", tostring(self.target_), eventName, handle)
        end
        -- listener[1] = listener
        -- listener[2] = tag
        event.tag = listener[2]
        listener[1](event)
        if event.stop_ then
            if DEBUG > 1 then
                printInfo("%s [Event] dispatchEvent() - break dispatching for event %s", tostring(self.target_), eventName)
            end
            break
        end
    end

    return self.target_
end

function Event:RemoveEventListener(handleToRemove)
    for eventName, listenersForEvent in pairs(self.listeners_) do
        for handle, _ in pairs(listenersForEvent) do
            if handle == handleToRemove then
                listenersForEvent[handle] = nil
                if DEBUG > 1 then
                    printInfo("%s [Event] removeEventListener() - remove listener [%s] for event %s", tostring(self.target_), handle, eventName)
                end
                return self.target_
            end
        end
    end

    return self.target_
end

function Event:RemoveEventListenersByTag(tagToRemove)
    for eventName, listenersForEvent in pairs(self.listeners_) do
        for handle, listener in pairs(listenersForEvent) do
            -- listener[1] = listener
            -- listener[2] = tag
            if listener[2] == tagToRemove then
                listenersForEvent[handle] = nil
                if DEBUG > 1 then
                    printInfo("%s [Event] removeEventListener() - remove listener [%s] for event %s", tostring(self.target_), handle, eventName)
                end
            end
        end
    end

    return self.target_
end

function Event:RemoveEventListenersByEvent(eventName)
    self.listeners_[string.upper(eventName)] = nil
    if DEBUG > 1 then
        printInfo("%s [Event] removeAllEventListenersForEvent() - remove all listeners for event %s", tostring(self.target_), eventName)
    end
    return self.target_
end

function Event:RemoveAllEventListeners()
    self.listeners_ = {}
    if DEBUG > 1 then
        printInfo("%s [Event] removeAllEventListeners() - remove all listeners", tostring(self.target_))
    end
    return self.target_
end

function Event:HasEventListener(eventName)
    eventName = string.upper(tostring(eventName))
    local t = self.listeners_[eventName]
    for _, __ in pairs(t) do
        return true
    end
    return false
end

function Event:DumpAllEventListeners()
    print("---- Event:dumpAllEventListeners() ----")
    for name, listeners in pairs(self.listeners_) do
        printf("-- event: %s", name)
        for handle, listener in pairs(listeners) do
            printf("--     listener: %s, handle: %s", tostring(listener[1]), tostring(handle))
        end
    end
    return self.target_
end

return Event
