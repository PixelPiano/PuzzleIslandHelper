local uiElements= require("ui.elements")
local contextMenu = require("ui.context_menu")
local mods = require("mods")
local expandableGrid = mods.requireFromPlugin("ui.widgets.expandableGrid")
local grid = require("ui.widgets.grid")
local objectField = {}
objectField.fieldType = "PuzzleIslandHelper.objectList"
objectField._MT = {}
objectField._MT.__index = {}
local invalidStyle = {
    normalBorder = {0.65, 0.2, 0.2, 0.9, 2.0},
    focusedBorder = {0.9, 0.2, 0.2, 1.0, 2.0}
}
--change this to be some way to turn a list of objects into a string
local function getLabelString(objects)
    return tostring(#objects).. " Objects"
end
--change this if desired to some way to turn an object into a string
local function objectText(object)
    return tostring(object[1])..", "..tostring(object[2])..", "..tostring(object[3])
end
function objectField._MT.__index:getValue()
    return self.currentValue
end
function objectField._MT.__index:getCurrentText()
    return self.currentText
end
function objectField._MT.__index:getObjectField(objNum, field)
    return self:getValue()[objNum][field]
end
function objectField._MT.__index:getFieldText(objectNum, field)
    return self.currentText and self.currentText[objectNum] and self.currentText[field] or self:getObjectField(objectNum,field)
end
function objectField._MT.__index:partValid(objNum, field)
    return self.validators[field](self.currentValue[objNum][field])
end
function objectField._MT.__index:objectValid(objNum)
    for i=1,self.num,1 do
        if not self:partValid(objNum,i) then return false end
    end
    return true
end
function objectField._MT.__index:fieldValid()
    for i=1,#self.currentValue do
        if not self:objectValid(i) then return false end
    end
    return true
end
local function updateFieldStyle(formfield,objNum, field)
    local validVisuals = true
    local valid = true
    if objNum and field then
        validVisuals = formfield.validVisuals[objNum][field]
        valid = formfield:partValid(objNum,field)
        local needsChanged = validVisuals ~= valid
        if needsChanged then
            if not valid then
                formfield.objectParts[objNum][field].style = invalidStyle
            else
                formfield.objectParts[objNum][field].style = nil
            end
            formfield.validVisuals[objNum][field] = valid
        end
    end
    local objectVisuals =true 
    local objectValid = true
    if objNum then
        objectVisuals = formfield.objectVisuals[objNum]
        objectValid = valid and formfield:objectValid(objNum)
        local objectChanged = objectVisuals~=objectValid
        if objectChanged then
            if not objectValid then
                formfield.object[objNum].style = invalidStyle
            else
                formfield.object[objNum].style = nil
            end
            formfield.objectVisuals[objNum] = objectValid
        end
    end
    local fieldVisuals = formfield.fieldVisuals
    local fieldValid = objectValid and formfield:fieldValid()
    local fieldChanged = fieldVisuals~=fieldValid
    if fieldChanged then
        if not fieldValid then
            formfield.field.style = invalidStyle
        else
            formfield.field.style = nil
        end
        formfield.fieldVisuals = fieldValid
    end
end
local function partChanged(formField,objNum,field)
    return function (element, new, old)
        formField.currentValue[objNum][field] = new --not going to bother with value transformers.
        formField.currentText[objNum][field] = new
        formField.objects[objNum]:setText(objectText(formField.currentValue[objNum]))
        formField.field:setText(getLabelString(formField.currentValue))
        updateFieldStyle(formField,objNum,field)
        formField:notifyFieldChanged()
    end
end
function objectField.getElement(name,value,options)
    local formField = {}
    --change to be a list of validators
    --the first validator applies to the first element of every object, ect
    formField.validators = options.validators or {
        function (v) return true end,function (v) return true end, function (v) return true end
    }
    --customize here to make the the graphics look how you want
    local minWidth = options.minWidth or options.width or 160
    local maxWidth = options.maxWidth or options.width or 160
    local objectMinWidth = options.objectMinWidth or options.objectWidth or 100
    local objectMaxWidth = options.objectMaxWidth or options.objectWidth or 100
    local partMinWidth = options.partMinWidth or options.partWidth or 80
    local partMaxWidth = options.partMaxWidth or options.partWidth or 80
    formField.num = options.num or 3
    local label = uiElements.label(options.displayName or name)
    if options.tooltipText then
        label.interactive = 1
        label.tooltipText = options.tooltipText
    end
    label.centerVertically = true
    formField.label = label
    formField.currentValue = value
    formField.validVisuals = {}
    formField.objectVisuals = {}
    formField.formVisuals = true
    formField.objects = {}
    formField.objectParts = {}
    local objContexts = {}
    local labels = {uiElements.label("Field 1"),uiElements.label("Field 2"),uiElements.label("Field 3")} --change here to get different labels
    local function makeItem(newObj, idx)
        local parts = {}
        table.insert(formField.validVisuals,{})
        for i=1,formField.num,1 do
            table.insert(parts,uiElements.field(newObj[i],partChanged(formField,idx,i)):with({
                minWidth = partMinWidth,
                maxWidth = partMaxWidth
            }))
            table.insert(formField.validVisuals[#formField.validVisuals],true)
        end
        table.insert(formField.objectParts,parts)
        local obj = uiElements.field(objectText(newObj),function() end):with({
            minWidth = objectMinWidth,
            maxWidth = objectMaxWidth
        })
        table.insert(formField.objects,obj)
        table.insert(formField.objectVisuals,true)
        local gridLs = {}
        for i,v in ipairs(labels) do table.insert(gridLs,v) end
        for i,v in ipairs(parts) do table.insert(gridLs,v) end
        local objWithContext  = contextMenu.addContextMenu(obj,function ()
            return grid.getGrid(gridLs,formField.num)
        end,{
            shouldShowMenu = function (element, x, y, button) return true end,
            mode= "focused"
        })
        table.insert(objContexts,objWithContext)
        return objWithContext
    end
    for i,v in ipairs(value) do
        makeItem(v,i)
    end
    formField.field = uiElements.field(getLabelString(value),function () end):with({
        minWidth=minWidth,
        maxWidth=maxWidth
    })
    local defaultObj = {"Thing 1", "Thing 2", "Thing 3"} --change this
    local fieldWithConext = contextMenu.addContextMenu(formField.field,function ()
        return expandableGrid.getGrid(objContexts,4,{minWidth=((objectMinWidth-25)/2),maxWidth=((objectMaxWidth-25)/2)},
        function ()
            local index = #formField.currentValue +1
            local item = makeItem(defaultObj,index)
            formField.currentValue[index] = defaultObj
            formField.objects[index]:setText(objectText(formField.currentValue[index]))
            formField.field:setText(getLabelString(formField.currentValue))
            updateFieldStyle(formField,index)
            return item
        end,
        function (idx)
            table.remove(formField.validVisuals)
            table.remove(formField.objectVisuals)
            table.remove(formField.objectParts)
            table.remove(objContexts)
            table.remove(formField.objects)
            table.remove(formField.currentValue)
            formField.field:setText(getLabelString(formField.currentValue))
        end
    )
    end,{
        shouldShowMenu = function (element,x,y,button) return true end,
        mode = "focused"
    })
    formField.elements = {
        label, fieldWithConext
    }
    formField.width = 2
    formField.initialValue = value
    formField.currentValue = value
    formField.currentText = value
    formField.name= name
    return setmetatable(formField,objectField._MT)
end
return objectField