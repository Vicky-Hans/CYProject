---
--- 文件名称:  SimpleCommandWrap
--- 创建者:    yuancan
--- 创建时间:  2022/7/15 3:32 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

--模块
---@class SimpleCommandWrap
---@field public serialId number  
local SimpleCommandWrap = bpcClass('SimpleCommandWrap')
local CSSimpleCommand = CS.DH.UIFramework.Commands.SimpleCommand

--[[--
构造函数
]]
function SimpleCommandWrap:ctor(...)
    self.action = nil
    self.csCommand = CSSimpleCommand(function()
        if self.action then
            self.action()
        end
    end)
end

function SimpleCommandWrap:SetAction(action)
    if action == nil then
        error("[SimpleCommandWrap] SetAction action is nil")
    end
    self.action = action
end

function SimpleCommandWrap:Clear()
    self.action = nil
end

function SimpleCommandWrap:Release()
    self.csCommand:Release()
    self.csCommand = nil
end

return SimpleCommandWrap