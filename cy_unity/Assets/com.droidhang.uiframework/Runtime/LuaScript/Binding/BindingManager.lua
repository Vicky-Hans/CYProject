---
--- 文件名称:  BindingManager
--- 创建者:    yuancan.
--- 创建时间:  2021/7/14 6:26 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local Dictionary = require("Common/Dictionary")
local List = require("Common/List")

--模块
---@class BindingManager
local BindingManager = bpcClass("BindingManager")
local enableDebug = true

local split = function (input, delimiter)
    input = tostring(input)
    delimiter = tostring(delimiter)
    if (delimiter=='') then return false end
    local pos,arr = 0, {}
    -- for each divider found
    for st,sp in function() return string.find(input, delimiter, pos, true) end do
        table.insert(arr, string.sub(input, pos, st - 1))
        pos = sp + 1
    end
    table.insert(arr, string.sub(input, pos))
    return arr
end

function BindingManager:Init()
    if self.init then
        return
    end

    ---@type Dictionary
    self.dataContextTable = Dictionary:New("string","List")
    ---@type Dictionary
    self.dataContextNameTable = Dictionary:New("object","string")
    ---@type Dictionary
    self.sourceTable = Dictionary:New("string","object")
    ---@type Dictionary
    self.sourceNameTable = Dictionary:New("table","string")
    self.init = true
    ---@type table<DataContext,DataContext>
    self.dataContext = {}
end


---@param sourceName string 绑定对象的名称
function BindingManager:BindSource(sourceName)
    if self == nil then
        return
    end
    
    if not self.dataContextTable:ContainsKey(sourceName) then
        -- no context need to bind
        return
    end
    
    if not self.sourceTable:ContainsKey(sourceName) then
        -- no source
        return
    end

    if enableDebug then
        bpcPrintf("BindSource: %s ",sourceName)
    end
    
    local source = self.sourceTable[sourceName]
    local dataContextList = self.dataContextTable[sourceName]
    -- update all context source
    for k, v in ipairs(dataContextList) do
        v:SetSource(source)
    end
    
end

---@param sourceName string 解除绑定对象的名称
function BindingManager:UnbindSource(sourceName)
    if self == nil then
        return
    end
    
    local dataContextList = self.dataContextTable[sourceName]
    if dataContextList == nil then
        -- no context need to bind
        return
    end

    if enableDebug then
        bpcPrintf("UnbindSource: %s",sourceName)
    end

    -- update all context source
    -- unbind all context
    for k, v in ipairs(dataContextList) do
        v:SetSource(nil)
    end

end


---注册绑定上下文
---@param dataContext DataContext 绑定上下文对象
---@param sourceName string 需要绑定的ViewModel名称
function BindingManager:AddDataContext(dataContext, sourceName)
    if self == nil then
        return
    end
    assert(dataContext ~= nil,"[BindingManager:AddDataContext] data context is nil")
    assert(sourceName ~= nil,"[BindingManager:AddDataContext] source name is null or empty")
    
    if enableDebug then
        bpcPrintf("Add data context: %s , require source: %s",tostring(dataContext),sourceName)
    end

    if self.dataContextNameTable:ContainsKey(dataContext) then
        bpcPrintf("DataContext %s already registered",tostring(dataContext))
        return
    end
    
    local dataContextList = nil
    if not self.dataContextTable:ContainsKey(sourceName) then
        dataContextList = List:New()
        self.dataContextTable:Add(sourceName,dataContextList)
    else
        dataContextList = self.dataContextTable[sourceName]
    end
    
    dataContextList:Add(dataContext)

    self.dataContextNameTable:Add(dataContext,sourceName)
    
    local source = self.sourceTable[sourceName]
    if source ~= nil then
        dataContext:SetSource(source)
    end
end

---取消注册绑定上下文
---@param dataContext DataContext 上下文对象
function BindingManager:RemoveDataContext(dataContext)
    if dataContext == nil or self == nil then
        return
    end

    if  self.dataContextNameTable == nil then
        return
    end

    if enableDebug then
        bpcPrintf("Remove DataContext %s",tostring(dataContext))
    end

    if not self.dataContextNameTable:ContainsKey(dataContext) then
        --error("[BindingManager:RemoveDataContext] DataContext "..tostring(dataContext.ownerView).." had been unregistered")
        return
    end
    
    local sourceName = self.dataContextNameTable[dataContext]

    if not self.dataContextTable:ContainsKey(sourceName) then
        --error("[BindingManager:RemoveDataContext] DataContext "..tostring(dataContext.ownerView).." had been unregistered")
        return
    end
    
    local dataContexts = self.dataContextTable[sourceName]
    
    dataContexts:Remove(dataContext)
    self.dataContextNameTable:Remove(dataContext)

    if dataContexts:Count() == 0 then
        self.dataContextTable:Remove(sourceName)
    end
    
    -- reset source
    dataContext:SetSource(nil)
end

---注册ViewModel
---@param  source any ViewModel对象
---@param  sourceName string ViewModel名称
function BindingManager:AddSource(source, sourceName)
    -- instance released ignore add source
    if self == nil then
        return
    end
    
    if sourceName == nil then
        error("[BindingManager:AddSource] invalid source name")
        return
    end

    if source == nil then
        error("[BindingManager:AddSource] invalid source object")
        return
    end

    if enableDebug then
        bpcPrintf("AddSource source name %s source is %s",sourceName,tostring(source))
    end

    if self.sourceTable:ContainsKey(sourceName) then
        error("[BindingManager:AddSource] Source already added "..sourceName)
        return
    end
    
    self.sourceTable:Add(sourceName,source)
    self.sourceNameTable:Add(source,sourceName)
    
    self:BindSource(sourceName)
end

---移除ViewModel
---@param source string ViewModel对象
function BindingManager:RemoveSource(source)
    -- instance released ignore remove source
    if self == nil or self.sourceNameTable == nil then
        return
    end
    
    if source == nil then
        error("[BindingManager:RemoveSource] invalid source object")
        return
    end

    if enableDebug then
        bpcPrintf("Remove source %s",tostring(source))
    end

    local sourceName = self.sourceNameTable[source]
    if sourceName == nil then
        return
    end
    
    self.sourceNameTable:Remove(source)
    self.sourceTable:Remove(sourceName)
    
    self:UnbindSource(sourceName)
end

function BindingManager:Clear()
    --- Unbind all data context
    self.dataContextNameTable:ForEach(function(key,value)
        if key == nil then
            print(value)
        end
        key:SetSource(nil)
    end)
    self.sourceNameTable:ForEach(function(key,value)
        if key then
            key:InternalRelease()
        end
    end)
    self.dataContextTable:Clear()
    self.dataContextNameTable:Clear()
    self.sourceTable:Clear()
    self.sourceNameTable:Clear()
end

---@param sourcePath string
---@param object any
---@return string
function BindingManager:GetBindingObject(object, sourcePath)
    if object == nil then
        error("[BindingManager:GetBindingObject] Binding object is nil")
        return
    end
    if sourcePath == nil then
        error("[BindingManager:GetBindingObject] Binding source path is nil")
    end

    local splitPath = split(sourcePath,".")
    if #splitPath == 1 then
        return object
    end
    
    local type = type(object)
    local length = #splitPath
    local nestedObj = object
    for i = 1, length - 1 do
        local propertyName = splitPath[i]
        if type == "table" then
            nestedObj = nestedObj[propertyName]
        else
            nestedObj = nestedObj[propertyName]
        end
    end
    
    return nestedObj
end

--- 多语言切换
function BindingManager:OnLocalize()
    for _, v in pairs(self.dataContext) do
        v:RefreshBindings()
    end
end

---@param sourcePath string
---@return string
function BindingManager:GetPropertyPath(sourcePath)
    if sourcePath == nil then
        error("[BindingManager:GetPropertyPath] Binding source path is nil")
    end

    local splitPath = split(sourcePath,".")
    if #splitPath == 1 then
        return sourcePath
    end
    local length = #splitPath
    return splitPath[length]
end

function BindingManager:Release()
    require("Support/SimpleCommandPool").Release()
    self:Clear()
    self.init = false
end

return BindingManager