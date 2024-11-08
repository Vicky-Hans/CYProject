---
--- 文件名称:  ServerDataBinding
--- 创建者:    nieshihai
--- 创建时间:  2021/10/12 17:39
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local TextPathParser = require("Support/TextPathParser")
local ChainedSource = require("Binding/ChainedSource")
local BindingManager = require("Binding/BindingManager")

--模块
---@class ServerDataBinding
local ServerDataBinding = bpcClass("ServerDataBinding")

--[[--
构造函数
]]
---@param dataContext DataContext
function ServerDataBinding:ctor(dataContext)
    assert(dataContext ~= nil, "[ServerDataBinding]Build dataContext is nil")
    self.dataContext = dataContext
    ---@type any
    self.source = nil
    ---@type string
    self.sourcePath = "" --如 'currentRole.name'
    ---@type string
    self.targetPath = ""
    ---@type string
    self.sourcePropertyName = "" --如 'name'
    
    self.OnSourcePropertyChanged = function(sender, propertyName, oldValue, newValue)
        assert(self:IsBound())
        if sender ~= self.source then
            error("Invalid sender " .. tostring(self.source))
            return
        end

        if propertyName ~= nil and propertyName ~= self.sourcePropertyName then
            return
        end

        self:UpdateTarget()
    end
end

function ServerDataBinding:IsBound()
    return self.source ~= nil
end

---@param sourcePath string
---@return ServerDataBinding
function ServerDataBinding:To(sourcePath)
    assert(sourcePath ~= nil)
    self.sourcePath = sourcePath
    self.sourcePropertyName = BindingManager:GetPropertyPath(sourcePath)
    ---@type TextPathParser
    local parser = TextPathParser(sourcePath)
    ---@type Path
    local path = parser:Parse()
    ---@type PathToken
    self.token = path:AsPathToken()
    
    return self
end

---@param callback function
function ServerDataBinding:OnCallback(callback)
    self.callback = callback
    self.dataContext:AddBinding(self)
end

-- @type #ObservableObject source
function ServerDataBinding:Bind(source)
    if source == nil then
        error("source is null")
    end

    if self:IsBound() then
        self:Unbind()
    end

    --- chained source binding
    if self.token:HasNext() then
        self.source = ChainedSource(source, self.token)
    else
        local bindingSource = source
        if bindingSource == nil then
            error("binding source is null")
            return
        end

        self.source = bindingSource
    end

    self.source:subscribe(self.sourcePropertyName, self.OnSourcePropertyChanged)
    self:UpdateTarget()
end

function ServerDataBinding:Unbind()
    if not self:IsBound() then
        return
    end

    self.source:unsubscribe(self.sourcePropertyName, self.OnSourcePropertyChanged)
    self.source = nil
end

function ServerDataBinding:UpdateTarget()
    if not self:IsBound() then
        return
    end

    local value
    if self.source.__classname == "ChainedSource" then
        value = self.source:GetValue()
    else
        value = self.source[self.sourcePropertyName]
    end

    self.callback(value)
end

return ServerDataBinding