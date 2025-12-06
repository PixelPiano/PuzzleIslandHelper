local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local xnaColors = require("consts.xna_colors")
local brown = xnaColors.Brown
local lime = xnaColors.Lime
local black = xnaColors.Black
local libraryPressurePlateHint = {}

libraryPressurePlateHint.justification = { 0, 0 }

libraryPressurePlateHint.name = "PuzzleIslandHelper/LibraryPressurePlateHint"
libraryPressurePlateHint.depth = 1
libraryPressurePlateHint.placements =
{
    name = "Library Pressure Plate Hint",
    data = 
    {
        width = 8,
        height = 8,
        flag = "",
        sequence = "0,1,2",
        inputs = 3,
        boxWidth = 24,
        boxHeight = 16,
        xPadding = 1,
        yPadding = 1
    }
}
local function getRectangle(x, y,width, height, color)
    local rectangle = utils.rectangle(x,y,width, height)
    return drawableRectangle.fromRectangle("fill", rectangle, color, color)
end

local function getSize(entity)
    local width =  entity.xPadding + (entity.inputs * 2 + 1) * (entity.boxWidth + entity.xPadding)
    local height = entity.boxHeight + entity.yPadding * 2
    entity.width = width
    entity.height = height
    return {width, height}
end
function libraryPressurePlateHint.minimumSize(room, entity)
    return getSize(entity)
end
function libraryPressurePlateHint.maximumSize(room, entity)
    return getSize(entity)
end


function libraryPressurePlateHint.sprite(room, entity)
    local sprites = {}
    local size = getSize(entity)
    local boxWidth = entity.boxWidth
    local boxHeight = entity.boxHeight
    local xpad = entity.xPadding;
    local ypad = entity.yPadding
    table.insert(sprites,getRectangle(entity.x, entity.y, size[1],size[2],black))
    local dummy = true
    for x = xpad, size[1] - boxWidth - xpad, boxWidth + xpad do
        if dummy then
            table.insert(sprites,getRectangle(entity.x + x,entity.y + ypad,boxWidth,boxHeight,brown))
        else
            table.insert(sprites,getRectangle(entity.x + x,entity.y + ypad,boxWidth,boxHeight,lime))
        end
        dummy = not dummy
    end
    return sprites
end
function libraryPressurePlateHint.selection(room, entity)
    local size = getSize(entity)
    return utils.rectangle(entity.x, entity.y, size[1], size[2])
end

return libraryPressurePlateHint