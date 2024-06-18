local uiElements = require("ui.elements")
local contextMenu = require("ui.context_menu")
local randomField = {}
local configs = require("configs")

randomField.fieldType = "PuzzleIslandHelper.randomValue"

randomField._MT = {}
randomField._MT.__index = {}

-- Use currentText field if possible, needed to "delay" the value for fieldValid
function randomField._MT.__index:getCurrentText()
    return self.currentText or self.field.text or ""
end

function randomField._MT.__index:fieldValid()
   local v= self:fieldsValid()
   return v[1]
end
function randomField._MT.__index:validateIdx(v)
    return true
end
local function validateType(v)
    local lower = string.lower(v)
    for i = 1, #validrandomFieldTypes do
        if lower == validrandomFieldTypes[i] then
            return true
        end
    end
    return false
end
function randomField._MT.__index:fieldsValid()
    return {validateType(self:getValue()[1] or ""), true, true}
end
local function listToString(ls, sep)
    local innerSep = ''
    local res = ''
    if type(ls) ~= 'table' then
        return ls
    end
    for _,v in ipairs(ls) do
        res = res .. innerSep .. (v or "")
        innerSep = sep or ','
    end

    return res
end
function randomField._MT.__index:setValue(value)
    self.currentTexts = {
        tostring(value[1]),tostring(value[2]),tostring(value[3])
    }
    self.type:setText(self.currentTexts[1])
    self.tab:setText(self.currentTexts[2])
    self.window:setText(self.currentTexts[3])
    local text = "randomField: "..self.type
    if self.tab ~= nil then
        text = text .. ", Tab: "..self.tab
    end
    if self.window ~= nil then
        text = text .. ", Window: "..self.window
    end
    self.currentText = text
    self.field:setText(self.currentText)
    self.currentValue = value
end
function randomField._MT.__index:getValue()
    return self.currentValue
end

local invalidStyle = {
    normalBorder = {0.65, 0.2, 0.2, 0.9, 2.0},
    focusedBorder = {0.9, 0.2, 0.2, 1.0, 2.0}
}
local function shouldShowMenu(element, x, y, button)
    local menuButton = configs.editor.contextMenuButton
    local actionButton = configs.editor.toolActionButton
    if button == menuButton or button == actionButton then
        return true
    end
    return false
end

local function updateFieldStyle(formField, valid)
    -- Make sure the textbox visual style matches the input validity
    local validVisuals = formField.validVisuals
    if validVisuals[1]~= valid[1] then
        if not valid[1] then
            formField.type.style = invalidStyle
        else
            formField.type.style = nil
        end

        formField.validVisuals[1] = valid[1]

        formField.type:repaint()
    end
    if validVisuals[2] ~= valid[2] then
        if not valid[2] then
            formField.tab.style = invalidStyle
        else
            formField.tab.style = nil
        end

        formField.validVisuals[2] = valid[2]
        formField.tab:repaint()
    end
    if validVisuals[3] ~= valid[3] then
        if not valid[3] then
            formField.window.style = invalidStyle
        else
            formField.window.style = nil
        end
        formField.validVisuals[3] = valid[3]
        formField.window:repaint()
    end
end
local function fieldChanged(formField,col)
    return function(element, new, old)
        formField.currentValue[col] = #new>0 and new
        formField.button:setText(listToString(formField.currentValue, ", "))
        local valid = formField:fieldsValid()
        updateFieldStyle(formField, valid)
        formField:notifyFieldChanged()
    end
end
local function overUpdateFieldStyle(formField,valid)
    local validVisuals = formField.overValidVisuals
    if validVisuals ~=valid then
        if not valid then
            formField.button.style = invalidStyle
        else
            formField.button.style = nil
        end
        formField.overValidVisuals = valid
        formField.button:repaint()
    end
end
local function overFieldChanged(formField)
    return function(element, new, old)
        local valid = formField:fieldValid()
        overUpdateFieldStyle(formField,valid)
        formField:notifyFieldChanged()
    end
end
local function getLabel(value)
    local text = "Empty"
    if value[1] ~= nil then
        text = "" .. value[1]
    end
    return text
end
function randomField.getElement(name, value, options)
    local formField = {}

    local valueTransformer = options.valueTransformer or function(v)
        return v
    end

    local minWidth = options.minWidth or options.width or 30
    local maxWidth = options.maxWidth or options.width or 30
    local nMinWidth = options.nMinWidth or options.nWidth or 80
    local nMaxWidth = options.nMaxWidth or options.nWidth or 80
    formField.minValue = options.minValue or -math.huge
    formField.maxValue = options.maxValue or math.huge
    local editable = options.editable

    local label = uiElements.label(options.displayName or name)
    local button = uiElements.button("Randomize",overFieldChanged(formField)):with({
        minWidth = minWidth,
        maxWidth = maxWidth
    })
    local type = uiElements.field(value[1], fieldChanged(formField,1)):with({
        minWidth = nMinWidth,
        maxWidth = nMaxWidth,
    })
    if editable == false then
        type:setEnabled(false)
    end

    type:setPlaceholder(tostring(value[1] or 0))
    tab:setPlaceholder(tostring(value[2] or 0))
    window:setPlaceholder(tostring(value[3] or 0))
    local TYPE = uiElements.label("Type")
    local TAB = uiElements.label("Tab")
    local WINDOW = uiElements.label("Window")
    local buttonContext = contextMenu.addContextMenu(
        button,
        function ()
            return grid.getGrid({
                TYPE,TAB,WINDOW,
                type,tab,window
            },3)
        end,
        {
            shouldShowMenu = shouldShowMenu,
            mode = "focused"
        }
    )
    if options.tooltipText then
        label.interactive = 1
        label.tooltipText = options.tooltipText
    end

    label.centerVertically = true

    formField.label = label
    formField.type = type
    formField.tab = tab
    formField.window = window
    formField.button = button
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.valueTransformer = valueTransformer
    formField.validVisuals = {true,true,true}
    formField.overValidVisuals = true
    formField.width = 2
    formField.elements = {
        label, buttonContext
    }

    formField = setmetatable(formField, randomField._MT)

    return formField
end



return randomField