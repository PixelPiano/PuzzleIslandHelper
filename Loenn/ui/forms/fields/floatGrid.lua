local uiElements = require("ui.elements")
local grid = require("ui.widgets.grid")
local contextMenu = require("ui.context_menu")
local rotationGrid = {}
local configs = require("configs")
local logging = require("logging")

rotationGrid.fieldType = "PuzzleIslandHelper.rotationGrid"

rotationGrid._MT = {}
rotationGrid._MT.__index = {}

-- Use currentText field if possible, needed to "delay" the value for fieldValid
function rotationGrid._MT.__index:getCurrentText()
    return self.currentText or self.field.text or ""
end

function rotationGrid._MT.__index:fieldValid()
   local v= self:fieldsValid()
   return v[1] and v[2] and v[3] and v[4] and v[5]
end
function rotationGrid._MT.__index:validateIdx(v)
    return type(v) == "number" and v >= self.minValue and v <= self.maxValue
end

function rotationGrid._MT.__index:fieldsValid()
    return { self:validateIdx(self:getValue()[1]), self:validateIdx(self:getValue()[2]), self:validateIdx(self:getValue()
    [3]),self:validateIdx(self:getValue()[4]),self:validateIdx(self:getValue()[5]) }
end

function rotationGrid._MT.__index:setValue(value)
    self.currentTexts = {
        tostring(value[1]),tostring(value[2]),tostring(value[3]),tostring(value[4]),tostring(value[5])
    }
    self.yaw:setText(self.currentTexts[1])
    self.pitch:setText(self.currentTexts[2])
    self.roll:setText(self.currentTexts[3])
    self.mult:setText(self.currentTexts[4])
    self.speed:setText(self.currentTexts[5])
    local seperator = ""
    local text = ""
    if self.yaw ~= nil then
        text = text .. "Yaw Rate: "..self.yaw
        seperator = ", "
    end
    if self.pitch ~= nil then
        text = text .. seperator .. "Pitch Rate: "..self.pitch
        seperator = ", "
    end
    if self.roll ~= nil then
        text = text .. seperator .. "Roll Rate: "..self.roll
        seperator = ", "
    end
    if self.mult ~= nil then
        text = text .. seperator .. "Mult: "..self.mult
        seperator = ", "
    end
    if self.speed ~= nil then
        text = text .. seperator .. "Speed: "..self.speed
    end
    self.currentText = text
    self.field:setText(self.currentText)
    self.currentValue = value
end
function rotationGrid._MT.__index:getValue()
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
            formField.yaw.style = invalidStyle
        else
            formField.yaw.style = nil
        end
        formField.validVisuals[1] = valid[1]
        formField.yaw:repaint()
    end
    if validVisuals[2] ~= valid[2] then
        if not valid[2] then
            formField.pitch.style = invalidStyle
        else
            formField.pitch.style = nil
        end

        formField.validVisuals[2] = valid[2]
        formField.pitch:repaint()
    end
    if validVisuals[3] ~= valid[3] then
        if not valid[3] then
            formField.roll.style = invalidStyle
        else
            formField.roll.style = nil
        end
        formField.validVisuals[3] = valid[3]
        formField.roll:repaint()
    end
    if validVisuals[4] ~= valid[4] then
        if not valid[4] then
            formField.mult.style = invalidStyle
        else
            formField.mult.style = nil
        end
        formField.validVisuals[4] = valid[4]
        formField.mult:repaint()
    end
    if validVisuals[5] ~= valid[5] then
        if not valid[5] then
            formField.speed.style = invalidStyle
        else
            formField.speed.style = nil
        end
        formField.validVisuals[5] = valid[5]
        formField.speed:repaint()
    end
end
local function fieldChanged(formField,col)
    return function(element, new, old)
        formField.currentValue[col] = #new > 0 and tonumber(new)
        local valid = formField:fieldsValid()
        updateFieldStyle(formField, valid)
        formField:notifyFieldChanged()
    end
end
local function overUpdateFieldStyle(formField, valid)
    local validVisuals = formField.overValidVisuals
    if validVisuals ~= valid then
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
function rotationGrid.getElement(name, value, options)
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
    local button = uiElements.button("Edit",overFieldChanged(formField)):with({
        minWidth = minWidth,
        maxWidth = maxWidth
    })
    local rand = uiElements.button("Randomize",overFieldChanged(formField)):with({
        minWidth = minWidth,
        maxWidth = maxWidth
    })
    local yaw = uiElements.field(tostring(value[1]), fieldChanged(formField,1)):with({
        minWidth = nMinWidth,
        maxWidth = nMaxWidth,
    })
    local pitch = uiElements.field(tostring(value[2]), fieldChanged(formField,2)):with({
        minWidth = nMinWidth,
        maxWidth = nMaxWidth
    })
    local roll = uiElements.field(tostring(value[3]),fieldChanged(formField,3)):with({
        minWidth = nMinWidth,
        maxWidth = nMaxWidth
    })
    local mult = uiElements.field(tostring(value[4]), fieldChanged(formField,4)):with({
        minWidth = nMinWidth,
        maxWidth = nMaxWidth,
    })
    local speed = uiElements.field(tostring(value[5]), fieldChanged(formField,5)):with({
        minWidth = nMinWidth,
        maxWidth = nMaxWidth,
    })
    if editable == false then
        yaw:setEnabled(false)
        pitch:setEnabled(false)
        roll:setEnabled(false)
        mult:setEnabled(false)
        speed:setEnabled(false)
    end

    yaw:setPlaceholder(tostring(value[1] or 0))
    pitch:setPlaceholder(tostring(value[2] or 0))
    roll:setPlaceholder(tostring(value[3] or 0))
    mult:setPlaceholder(tostring(value[4] or 0))
    speed:setPlaceholder(tostring(value[5] or 0))
    local YAW = uiElements.label("Yaw Rate")
    local PITCH = uiElements.label("Pitch Rate")
    local ROLL = uiElements.label("Roll Rate")
    local MULT = uiElements.label("Mult")
    local SPEED = uiElements.label("Speed")
    local function getRandom(min, max)
        return min
    end
    local randContext = contextMenu.addContextMenu(
        rand,
        function ()
            value[1] = getRandom(0, 30)
            value[2] = getRandom(0,30)
            value[3] = getRandom(0,30)
            value[4] = getRandom(-5, 5)
            value[5] = getRandom(0, 10)
            fieldChanged(formField,1)
            fieldChanged(formField,2)
            fieldChanged(formField,3)
            fieldChanged(formField,4)
            fieldChanged(formField,5)
        end,
        {
            shouldShowMenu = false,
            mode = "focused"
        }
    )
    local buttonContext = contextMenu.addContextMenu(
        button,
        function ()
            return grid.getGrid({
                YAW, PITCH, ROLL,MULT,SPEED,
                yaw,pitch,roll,mult,speed
            },5)
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
    formField.yaw = yaw
    formField.pitch = pitch
    formField.roll = roll
    formField.mult = mult
    formField.speed = speed
    formField.button = button
    formField.rand = rand
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.valueTransformer = valueTransformer
    formField.validVisuals = {true, true, true, true, true}
    formField.overValidVisuals = true
    formField.width = 2
    formField.elements = {
        label, buttonContext, randContext
    }

    formField = setmetatable(formField, rotationGrid._MT)

    return formField
end



return rotationGrid