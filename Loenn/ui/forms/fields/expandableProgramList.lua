local uiElements = require("ui.elements")

local contextMenu = require("ui.context_menu")
local mods = require("mods")
local expandableGrid = mods.requireFromPlugin("ui.widgets.expandableGrid")
local utils = require("utils")
local programList = {}

programList.fieldType = "PuzzleIslandHelper.programList"

programList._MT = {}
programList._MT.__index = {}

function programList._MT.__index:setValue(value)
    self.currentValue = value
end

function programList._MT.__index:getValue()
    return self.currentValue
end
local validProgramTypes =
{
    "text","invalid","info","wave","access","fountain","life","pipe"
}

function programList._MT.__index:validateType(v)
    for i = 1, #programList do
        if validProgramTypes[i] == v then return true end
    end
    return false
end
function programList._MT.__index:fieldValid()
    for i,v in ipairs(self:getValue()) do
        if not self:validateType(v) then
            return false
        end
    end
    return true
end

local function getLabelString(names,maxLen)
    local sep = ""
    local out = ""
    for i,v in ipairs(names) do
        out = out .. sep .. (v or "")
        sep = ", "
        if i == maxLen then
            local rem = #names - maxLen
            if rem> 0 then
                out = out .. sep .. string.format("... %s more",rem)
            end
            return out
        end
    end
    return out
end

function programList.getElement(name, value, options)
    local formField = {}

    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160
    local innerMaxWidth = options.innerMaxWidth or options.innerWidth or 160
    local innerMinWidth = options.innerMinWidth or options.innerWidth or 160
    formField.allowEmpty = options.allowEmpty
    local maxLen = options.maxLen or 1
    local label = uiElements.label(options.displayName or name)
    local button = uiElements.button(getLabelString(value,maxLen),function () end):with({
        minWidth = minWidth,
        maxWidth = maxWidth
    })
    formField.programs = {}
    for i,v in ipairs(value) do
        local program = {
            
        }
        formField.programs[i] = v
    end
    local type = ""
    
    local row= uiElements.Row({"Type", type, label, field, label, field})
    local buttonContext =  contextMenu.addContextMenu(button,function ()
        return expandableGrid.getGrid(
            formField.programs, --list of elements to spawn with
            3, --# of elements per row
            {minWidth=((innerMinWidth-25)/2), maxWidth=((innerMaxWidth-25)/2)}, --button options
            function () --callback function (+ button)
                table.insert(value,"")

                local innerButton = uiElements.button("",buttonPressed(formField,#value)):with({
                    minWidth = innerMinWidth,
                    maxWidth = innerMaxWidth
                })
                table.insert(formField.programs, innerButton)
                formField.button.label.text = getLabelString(value,maxLen)
                return innerButton
            end,
            function (idx) --callback function (- button)
                table.remove(value)
                table.remove(formField.programs)
                formField.button.label.text = getLabelString(value,maxLen)
            end
        )
    end,{
        shouldShowMenu = function () return true end,
        mode = "focused"
    })

    if options.tooltipText then
        label.interactive = 1
        label.tooltipText = options.tooltipText
    end

    label.centerVertically = true

    formField.label = label
    formField.button = button
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.maxLen = maxLen
    formField.width = 2
    formField.elements = {
        label, buttonContext
    }

    return setmetatable(formField, programList._MT)
end

return programList