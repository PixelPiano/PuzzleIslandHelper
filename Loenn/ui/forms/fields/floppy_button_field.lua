local uiElements = require("ui.elements")
local serialize = require("utils.serialize")
local mods = require("mods")
local DataEditor = mods.requireFromPlugin("ui.widgets.floppy_editor")
local contextMenu = require("ui.context_menu")
local configs = require("configs")

local buttonField = {}

buttonField.fieldType = "FloppyHelper.buttonStringField"

buttonField._MT = {}
buttonField._MT.__index = {}

local invalidStyle = {
    normalBorder = {0.65, 0.2, 0.2, 0.9, 2.0},
    focusedBorder = {0.9, 0.2, 0.2, 1.0, 2.0}
}

function buttonField._MT.__index:setValue(value)
    self.currentText = self.displayTransformer(value)
    self.field:setText(self.currentText)
    self.currentValue = value
end

function buttonField._MT.__index:getValue()
    return self.currentValue
end

function buttonField._MT.__index:getCurrentText()
    return self.name
end

function buttonField._MT.__index:fieldValid()
    return self.validator(self:getValue(), self:getCurrentText())
end

local function updateFieldStyle(formField, wasValid, valid)
    if wasValid ~= valid then
        if valid then
            -- Reset to default
            formField.field.style = nil
        else
            formField.field.style = invalidStyle
        end

        formField.field:repaint()
    end
end

local function fieldChanged(formField)
    return function(element, new, old)
        local wasValid = formField:fieldValid()

        formField.currentValue = formField.valueTransformer(new)
        formField.currentText = new

        local valid = formField:fieldValid()

        updateFieldStyle(formField, wasValid, valid)
        formField:notifyFieldChanged()
    end
end

local function shouldShowMenu(self, x, y, button, istouch)   
    local menuButton = configs.editor.contextMenuButton
    local actionButton = configs.editor.toolActionButton

    if button == menuButton then
        return true
    elseif button == actionButton then
        return x > self.screenX and x < (self.screenX + self.width) and y > self.screenY and y < (self.screenY + self.height)
    end

    return false
end


function buttonField.getElement(name, value, options)
    local formField = {}

    local validator = function(v)
        return true
    end

    local valueTransformer = options.valueTransformer or function(v)
        local success, result = serialize.unserialize(v, true, 3)

        if success and result and type(result) == "table" then
            if #result == 1 then
                local data = result[1]
                if data["_fromLayer"] == "tilesFg" and data["tiles"] then
                    return string.gsub(data["tiles"] or "", "\\n", "\n")
                end
            end
        end
        return v
    end

    local displayTransformer = options.displayTransformer or function(v)
        return v
    end

    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160
    local editable = options.editable
 
    local label = uiElements.label(options.displayName or name)
    local field = uiElements.field(displayTransformer(value), fieldChanged(formField)):with({
        minWidth = minWidth,
        maxWidth = maxWidth
    }):hook({
        -- thanks to microlith57!!!
        update = function(orig, self, dt)
            orig(self, dt)
            if self.focused then
              self.label.text = self.text
            else
              self.label.text = "..."
            end
          end
      })


    if editable == false then
        field:setEnabled(false)
    end

    field:setPlaceholder(displayTransformer("0"))

    local button = uiElements.button("Edit", function () end)

    button.__formButtonInfo = {
        text = name,
        minWidth = minWidth,
        maxWidth = maxWidth
    }
    local buttonWithContext = contextMenu.addContextMenu(
        button,
        function()
            local editOptions = {
                width = 32,
                height = 32,
                callback = function(data)
                    local value = DataEditor.getFieldFromMatrix(data)
                    formField:setValue(value or "")
                end
            }
            return DataEditor.getDataEditor(formField, editOptions)
        end,
        {
            shouldShowMenu = shouldShowMenu,
            mode = "focused"
        }
    )

    if options.tooltipText then
        field.interactive = 1
        field.tooltipText = options.tooltipText
    end

    button.centerVertically = true

    formField.button = button
    formField.label = label
    formField.field = field
    formField.name = name
    formField.initialValue = value
    formField.currentValue = value
    formField.validator = validator
    formField.valueTransformer = valueTransformer
    formField.displayTransformer = displayTransformer
    formField.width = 4
    formField.elements = {
        label, field, buttonWithContext
    }

    return setmetatable(formField, buttonField._MT)
end

return buttonField