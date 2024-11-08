---
--- 文件名称:  BpcDebug
--- 创建者:    nieshihai
--- 创建时间:  2021/12/08 10:15
-------------------------------------------------------------------
--- 功能描述：
--- 调试性bug
---

local dhLog = CS.DHFramework.DHLog

---@class BpcDebug
local BpcDebug = bpcClass("BpcDebug")
local debugLevel = 1;  --- 1 = log, 2 = error, 3 = exception
local tab = "    ";

local function GetLogInfo(fmt, ...)
    local p = { ... }
    local info = fmt
    if #p > 0 then

        for i, v in ipairs(p) do
            p[i] = tostring(v)
        end

        info = string.format(fmt, ...)
    end
    local stack = debug.traceback("", 3)
    return info..stack
end

function BpcDebug.Init(level)
    debugLevel = level;
    if debugLevel > 1 then
        return
    end
end

function BpcDebug.Debug(fmt, ...)
    if debugLevel > 1 then
        return
    end
    local info = GetLogInfo(fmt, ...)

    if dhLog.Debug then
        dhLog.Debug(info)
    end
end

function BpcDebug.Warning(fmt, ...)
    local info = GetLogInfo(fmt, ...)

    dhLog.Warning(info)
end

function BpcDebug.Error(fmt, ...)
    local info = GetLogInfo(fmt, ...)
    
    dhLog.Error(info)
end

function BpcDebug.PrintTable(tbl, prefix)
    if debugLevel < 1 then
        return
    end

    if not tbl then
        return
    end

    local prefix = prefix or ""
    if type(tbl) ~= "table" then
        BpcDebug.Debug("%s%s", prefix, tbl)
    else
        BpcDebug.Debug("%s{", prefix)
        local prefix2 = prefix .. BpcDebug.tab
        for k, v in pairs(tbl) do
            if type(v) ~= "table" then
                BpcDebug.Debug("%s%s = %s", prefix2, k, v)
            else
                BpcDebug.Debug("%s%s = ", prefix2, k)
                BpcDebug.PrintTable(v, prefix2)
            end
        end
        
        BpcDebug.Debug("%s}", prefix)
    end
end

function BpcDebug.ColorDebug(...)
    local list = {...}
    local text
    local color = #list
    local words = {}

    for i = 1 , #list do
        --words = words.."  "..tostring(list[i] == nil or list[i] == "" and "nil" or list[i])
        local curLine = tostring(list[i] == nil or list[i] == "" and "nil" or list[i])
        table.insert(words,curLine)
        table.insert(words,tab)
    end

    local print = table.concat(words)

    if color == 1 then
        text = string.format("<color=#FFFFFF>  %s</color>", print)
    elseif color == 2 then
        text = string.format("<color=#EA00FF>  %s</color>", print)
    elseif color == 3 then
        text = string.format("<color=#CAFFB1>  %s</color>", print)
    elseif color == 4 then
        text = string.format("<color=#7373FF>  %s</color>", print)
    elseif color == 5 then
        text = string.format("<color=#FF7092>  %s</color>", print)
    else
        text = string.format("<color=#FBCE52>  %s</color>", print)
    end

    BpcDebug.Debug(text)
end

return BpcDebug
