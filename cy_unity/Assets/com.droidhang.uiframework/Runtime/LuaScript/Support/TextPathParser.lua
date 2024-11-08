---
--- 文件名称:  TextPathParser
--- 创建者:    yuancan.
--- 创建时间:  2021/7/21 11:47 AM
-------------------------------------------------------------------
--- 功能描述：
---
---

require("Common/System")
local Path = require("Support/Path")
local MemberNode = require("Support/MemberNode")
--模块
---@class TextPathParser
local TextPathParser = bpcClass("TextPathParser")
local enableDebug = DataBindingDebug

--[[--
构造函数
]]
---@param text string
function TextPathParser:ctor(text)
    assert(text~= nil and #text > 0,"[TextPathParser.ctor] invalid argument text")
    ---@type string
    ---remove all white space
    self.text = string.gsub(text," ","")
    ---@type number
    self.total = #text
    ---@type number
    self.pos = 0
    ---@type Path
    self.path = nil
end

---@public
---@return string
function TextPathParser:Current()
    return string.sub(self.text,self.pos,self.pos)
end

---@public
---@return boolean
function TextPathParser:MoveNext()
    self.pos = self.pos + 1
    if self.pos <= self.total then
        return true
    end
    return false
end

---@public
function TextPathParser:Reset()
    self.pos = 0
end

---@protected
---@return boolean
function TextPathParser:IsEof()
    return self.pos > self.total
end

---@public
---@return Path
function TextPathParser:Parse()
    if self.path ~= nil then
        return self.path
    end
    
    self.path = Path()
    self:MoveNext()

    repeat
        self:SkipWhiteSpaceAndCharacters('.')
        if self:Current() == '[' then
            self:ReadIndex()
            self:SkipWhiteSpace()
            if self:Current() ~= ']' then
                error("[TextPathParser.Parse] error parsing indexer with text "..self.text)
            end

            if self:MoveNext() then
                if self:Current() ~= '.' then
                    error("[TextPathParser.Parse] error parsing indexer with text "..self.text)
                end
            end
        elseif self:IsCurrentLetter() or self:Current() == '_' then
            self:ParseMemberName()
            local current = self:Current()
            if not self:IsEof() and current ~= '.' and current ~= '[' and current ~= ' ' then
                error("[TextPathParser.Parse] error parsing indexer with text "..self.text.." unexpected string "..current)
            end
        else
            error("[TextPathParser.Parse] error parsing indexer with text "..self.text)
        end
    until(self:IsEof()) -- end do while
    
    return self.path
end

---@protected
function TextPathParser:ReadIndex()
    if not self:MoveNext() then
        error("[TextPathParser.Parse] error parsing indexer with text "..self.text)
    end
    
    local char = self:Current()
    local ch = string.byte(char)
    if char == '\'' or char == '\"' then
        local index = self:ReadQuoteString()
        self.path:AppendIndexed(index)
        self:MoveNext()
        return
    end

    if ch >= string.byte('0') and ch <= string.byte('9') then
        local index = self:ReadUnsignedInteger()
        self.path:AppendIndexed(index)
        return
    end

    error("[TextPathParser.ReadIndex] error parsing indexer with text "..self.text)
end

---@private
function TextPathParser:ParseMemberName()
    local buf = ""
    repeat
        local char = self:Current()
        if not self:IsLetterOrDigit(char) and char ~= '_' then
            break
        end
        
        buf = buf..char
    until(not self:MoveNext())

    if #buf == 0 then
        error("[TextPathParser.ParseMemberName] error parsing indexer with text "..self.text)
    end

    if enableDebug then
        bpcPrintf("[ChainedSource MemberName]%s",buf)
    end
    
    self.path:Append(MemberNode(buf))
end
---@private
---@return number
function TextPathParser:ReadUnsignedInteger()
    local buf = ""
    repeat
        local char = self:Current()
        --- not a number
        if char < '0' or char > '9' then
            break
        end

        buf = buf..char
    until(not self:MoveNext())

    if #buf == 0 then
        error("[TextPathParser.ReadUnsignedInteger] error parsing indexer with text "..self.text)
    end
    
    if enableDebug then
        bpcPrintf("[ChainedSource [index]]%s",buf)
    end
    
    return tonumber(buf)
end

---@private
---@return string
function TextPathParser:ReadQuoteString()
    local char = self:Current()
    if char ~= '\'' and char ~= '\"' then
        error("[TextPathParser.ReadQuoteString] error parsing indexer with text "..self.text)
    end

    if not self:MoveNext() then
        error("[TextPathParser.ReadQuoteString] error parsing indexer with text "..self.text)
    end
    
    local buf = ""
    repeat
        char = self:Current()
        if not self:IsLetterOrDigit(char) and char ~= '_' and char ~= '-' then
            break
        end
        
        buf = buf..char
    until(not self:MoveNext())

    if #buf == 0 or (char ~= '\'' and char ~= '\"') then
        error("[TextPathParser.ReadQuoteString] error parsing indexer with text "..self.text)
    end

    if enableDebug then
        bpcPrintf("[ChainedSource ReadQuoteString]%s",buf)
    end
    
    return buf
end

function TextPathParser:SkipWhiteSpace()
    while(self:Current() == ' ') do
        if not self:MoveNext() then
            break
        end
    end
end

function TextPathParser:IsWhiteSpaceOrCharacter(ch,...)
    if ch == ' ' then
        return true
    end

    local args = {...}
    for k, v in pairs(args) do
        if v == ch then
            return true
        end
    end

    return false
end

function TextPathParser:SkipWhiteSpaceAndCharacters(...)
    while(self:IsWhiteSpaceOrCharacter(self:Current(),...)) do
        local result = self:MoveNext()
        if not result then
            break
        end
    end
end

---@private
---@return boolean
function TextPathParser:IsCurrentLetter()
    local char = self:Current()
    local ch = string.byte(char) | 0x20
    return ch >= string.byte('a') and ch <= string.byte('z')
end

---@private
---@return boolean
function TextPathParser:IsLetterOrDigit(char)
    local ch = string.byte(char)
    if ch >= string.byte('0') and ch <= string.byte('9') then
        return true
    end
    
    ch = string.byte(char) | 0x20
    return ch >= string.byte('a') and ch <= string.byte('z')
end


return TextPathParser