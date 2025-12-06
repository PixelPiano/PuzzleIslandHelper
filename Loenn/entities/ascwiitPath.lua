local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")

local ascwiitPath = {}
ascwiitPath.depth = -10001
ascwiitPath.name = "PuzzleIslandHelper/AscwiitPath"
ascwiitPath.nodeLimits = {2,-1}
ascwiitPath.lineRenderType = "line"
ascwiitPath.nodeVisibility = "always"
ascwiitPath.canResize = {false, false}
ascwiitPath.minimumSize = {8,8}
ascwiitPath.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
ascwiitPath.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
ascwiitPath.placements = {
    name = "Ascwiit Path",
    data = {
        width = 8,
        height = 8,
        pathID = "",
        nodeRadius = 20,
        speedMult = 1,
        removeBirdAtEnd = false,
        flagOnBirdRemoved = "",
        naiveFly = false
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

function ascwiitPath.sprite(room, entity)
    local sprites = {}
    local pos = { x = entity.x, y = entity.y}
    local text = getText(0, pos, 8, 8)
    local rect = getRectangle(room, entity, pos, true)
    table.insert(sprites,rect)
    table.insert(sprites,text)
    return sprites
end
function ascwiitPath.nodeSprite(room, entity, node, nodeIndex, viewport)
    local sprites = {}
    local pos = {x = node.x, y = node.y}
    local text = getText(nodeIndex, pos, 8, 8)
    local rect = getRectangle(room, entity, pos, false)
    table.insert(sprites,rect)
    table.insert(sprites,text)
    return sprites
end
function ascwiitPath.selection(room, entity)
    local offset = 0
    local size = 8
    local main = utils.rectangle(entity.x + offset, entity.y + offset, size, size)
    local nodes = {}

    if entity.nodes then
        for i, node in ipairs(entity.nodes) do
            nodes[i] = utils.rectangle(node.x + offset, node.y + offset, size, size)
        end
    end

    return main, nodes
end


return ascwiitPath