---
--- 文件名称:  Singleton
--- 创建者:    yuancan.
--- 创建时间:  2021/8/23 3:09 PM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")

--模块
---@class Singleton
local Singleton = bpcClass("Singleton")

function Singleton:Instance()
    if rawget(self,"instance") == nil then
        rawset(self,"instance",self())
    end
    return self.instance
end

return Singleton