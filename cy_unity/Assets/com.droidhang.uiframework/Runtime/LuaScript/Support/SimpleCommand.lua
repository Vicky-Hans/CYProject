---
--- 文件名称:  SimpleCommand
--- 创建者:    yuancan
--- 创建时间:  2022/7/15 3:31 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

--模块
---@class SimpleCommand
local SimpleCommand = bpcClass("SimpleCommand")

function SimpleCommand:ctor(action)
    self.action = action
end

function SimpleCommand:Execute()
    if self.action then
        self.action()
    end
end

return SimpleCommand