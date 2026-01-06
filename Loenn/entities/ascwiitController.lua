local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")

local ascwiitController= {}
ascwiitController.justification = { 0, 0 }

ascwiitController.name = "PuzzleIslandHelper/AscwiitController"

ascwiitController.depth = 1

ascwiitController.texture = "objects/PuzzleIslandHelper/ascwiit/controller"
ascwiitController.nodeLimits = {4,4}
ascwiitController.nodeLineRenderType = "fan"
ascwiitController.nodeVisibility = "always"
local fill = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
local border = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
local dirs = {"U","D","L","R"}
ascwiitController.placements =
{
    {
        name = "Ascwiit Controller",
        data = 
        {
            disableFlag = "",
            groupID = "",
            steps = "Up, Down, Left",
            isEnd = false,
            isStart = false,
            endDirection = "Right",
            removeAscwiitsIfWrongWay = true,
            removeAscwiitsIfDisabled = true,
        }
    }
}
local function getText(text, position, width, height)
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, position.x, position.y, width or 8, height or 8, font, 1)
                end)
            end)
    return result
end
local function getRectangle(room, entity, position, brighter)
    local colors = getColors(0.3, 0.8, brighter)
    local rectangle = utils.rectangle(position.x, position.y, 8, 8)
    local rect = drawableRectangle.fromRectangle("bordered", rectangle, colors[1], colors[2])
    return rect
end
function ascwiitController.nodeSprite(room, entity, node, index, viewport)
    local sprites = {}
    local pos = {x = node.x, y = node.y}
    local text = getText(dirs[index], pos, 8, 8)
    local rect = getRectangle(room, entity, pos, false)
    table.insert(sprites,rect)
    table.insert(sprites,text)
    return sprites
end
function ascwiitController.selection(room, entity)
    local offset = 0
    local size = 8
    local main = utils.rectangle(entity.x + offset, entity.y + offset, 32, 32)
    local nodes = {}

    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            nodes[i] = utils.rectangle(node.x + offset, node.y + offset, size, size)
        end
    end

    return main, nodes
end
return ascwiitController