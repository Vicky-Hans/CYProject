---
--- 文件名称:  RoleModel
--- 创建者:    yuancan.
--- 创建时间:  2021/9/23 11:28 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

local ObservableObject = require("Binding/ObservableObject")

---
--模块
---@class ProtoDataModel:ObservableObject
local ProtoDataModel = bpcClass("ProtoDataModel", ObservableObject)

function ProtoDataModel:ctor(pbData)
    self.super.ctor(self)

    if pbData == nil then
        return
    end

    self:CopyPbData(pbData)
end

function ProtoDataModel:CopyPbData(pbData)
    for k, v in pairs(pbData) do
        local valueType = type(v)
        if valueType == "function" then
            --- not support
        elseif valueType == "table" then
            self[k] = ObservableObject:Create(v)
        else
            self[k] = v
        end
    end
end

function ProtoDataModel:Wrap()
    local wrap = ObservableObject()
    wrap.data = self
    return wrap
end

return ProtoDataModel