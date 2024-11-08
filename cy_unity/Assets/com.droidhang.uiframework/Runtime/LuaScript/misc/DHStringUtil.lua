---
--- 文件名称:  DHStringUtil
--- 创建者:    weiyinlei
--- 创建时间:  2022/6/15 10:27
-------------------------------------------------------------------
--- 功能描述：工具类(单例) 字符串处理
---
---

--模块
---@class DHStringUtil
local DHStringUtil = bpcClass("DHStringUtil")


---IsEmoji
---@param inputContent string
---@param byteIdx number
function DHStringUtil.IsEmoji(inputContent, byteIdx)
    local isEmoji = false

    local byte1 = string.byte(inputContent, byteIdx)--根据首字节的大小确定
    if byte1 > 239 then
        --11110xxx
        local byte2 = string.byte(inputContent, byteIdx + 1)
        local byte3 = string.byte(inputContent, byteIdx + 2)
        if byte1 == 0xf0 and byte2 == 0x9f and byte3 == 0x98 then
            isEmoji = true
        end
    end

    return isEmoji
end

---获取字符串的长度（任何单个字符长度都为1）
---@param inputStr string
function DHStringUtil.GetStringLength(inputStr)
    if not inputStr or type(inputStr) ~= "string" then
        return 0
    end

    local totalLength = #inputStr
    if totalLength <= 0 then
        return 0
    end

    local length = 0  -- 字符的个数
    local i = 1
    while true do
        local curByte = string.byte(inputStr, i)--根据首字节的大小确定
        local byteCount = 1
        if curByte > 239 then
            --11110xxx
            byteCount = 4  -- 4字节字符
        elseif curByte > 223 then
            --1110xxxx
            byteCount = 3  -- 3字节字符
        elseif curByte > 128 then
            --110xxxxx
            byteCount = 2  -- 双字节字符
        else
            byteCount = 1  -- 单字节字符
        end

        i = i + byteCount
        length = length + 1
        if i > totalLength then
            break
        end
    end

    return length
end

---限制字符到特定长度内
---@param inputStr string
function DHStringUtil.LimitStringToLength(inputStr, maxLen)
    if not inputStr or type(inputStr) ~= "string" then
        return ""
    end

    local totalLength = #inputStr
    if totalLength <= 0 then
        return ""
    end

    --- 表示不做限制就返回原字符串
    if maxLen == nil or maxLen <= 0 then
        return inputStr
    end

    local length = 0  -- 字符的个数
    local i = 1
    while true do
        local curByte = string.byte(inputStr, i)--根据首字节的大小确定
        local byteCount = 1
        if curByte > 239 then
            --11110xxx
            byteCount = 4  -- 4字节字符
        elseif curByte > 223 then
            --1110xxxx
            byteCount = 3  -- 3字节字符
        elseif curByte > 128 then
            --110xxxxx
            byteCount = 2  -- 双字节字符
        else
            byteCount = 1  -- 单字节字符
        end

        i = i + byteCount
        length = length + 1
        if i > totalLength or length >= maxLen then
            break
        end
    end

    if i > totalLength then
        return inputStr
    end

    return string.sub(inputStr, 1, i - 1)
end


function DHStringUtil:GetFilterPattern()
    if self.filterPattern == nil then
        local UChatEmojiFilter = require("UChatEmojiFilter")
        self.filterPattern = table.concat(UChatEmojiFilter[2], ',')
        self.filterPattern = string.format("[%s]", self.filterPattern)
    end
    return self.filterPattern
end

-- 过滤特殊Unicode 
function DHStringUtil.FilterPatternString(inputStr)
    if inputStr then
        return string.gsub(inputStr, self:GetFilterPattern(), '')
    end
    return ""
end

-- 计算展示内容
-- sounum:目标整数  toInteger:转化为整数的系数  decimalplaces:小数位数(10^x)  tmark:展示符号标记
local function ConversionToStr(sounum, toInteger, decimalplaces, tmark)
    local strxiaoshu = ""
    local bfoceshow = false
    while decimalplaces > 1 do
        local txiaoshu = math.floor(sounum * decimalplaces * toInteger) % 10
        bfoceshow = bfoceshow or (txiaoshu > 0)
        strxiaoshu = (bfoceshow and txiaoshu or "") .. strxiaoshu
        decimalplaces = math.floor(decimalplaces * 0.1)
    end
    if bfoceshow then
        strxiaoshu = "." .. strxiaoshu
    end
    return string.format("%d%s%s", math.floor(sounum * toInteger), strxiaoshu, tmark)
end

-- 将数字转化为展示字符串
function DHStringUtil.NumTosString(monnum)
    monnum = tonumber(monnum) or 0
    if monnum >= 100000000 then
        return ConversionToStr(monnum, 0.000001, 10, "M")
    elseif monnum >= 100000 then
            return ConversionToStr(monnum, 0.0001, 10, "W")
    elseif monnum >= 1000 then
        return ConversionToStr(monnum, 0.001, 10, "K")
    end
    return string.format("%d", monnum)
end

return DHStringUtil