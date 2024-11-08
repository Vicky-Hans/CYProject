---
--- 文件名称:  LocaleConfigExample
--- 创建者:    yuancan
--- 创建时间:  2022/2/24 10:03 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

---@class LocaleConfigItem
---@field fontPath string
---@field fontSize number
---@field maxFontSize number
---@field minFontSize number
---@field useAbsoluteFontSize boolean
---@field overrideFontWeight boolean


--模块
---@type table<string,LocaleConfigItem>
local LocaleConfigExample = {
    --- 默认一般情况为拼UI时使用的配置信息
    --- 若UI参考图是按中文，则默认配置应该为中文下的配置，以此类推
    default = {
        fontPath = "",
        fontSize = 0,
        maxFontSize = 0,
        minFontSize = 0,
        useAbsoluteFontSize = false,
        overrideFontWeight = false,
    },
    en = {
        fontPath = "",
        fontSize = -2,
        maxFontSize = -2,
        minFontSize = -2,
        useAbsoluteFontSize = false,
        overrideFontWeight = false,
    }
}

return LocaleConfigExample