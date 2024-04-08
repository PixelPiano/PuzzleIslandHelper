local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")

local polyScreenShiftArea = {}
polyScreenShiftArea.depth = -10001
polyScreenShiftArea.name = "PuzzleIslandHelper/PolyScreenShiftArea"
polyScreenShiftArea.nodeLimits = {2,2}
polyScreenShiftArea.lineRenderType = "line"
polyScreenShiftArea.nodeVisibility = "always"
polyScreenShiftArea.canResize = {false, false}
polyScreenShiftArea.minimumSize = {8,8}
polyScreenShiftArea.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
polyScreenShiftArea.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
polyScreenShiftArea.placements = {
    name = "Polygon Screen Shift Area",
    data = {
        width = 8,
        height = 8
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
local function getColors(fillAmount, borderAmount, brighter)
    local colors = {}
    local addition = brighter and 0.2 or 0.0
    local mults = {fillAmount + addition, borderAmount + addition}
    local fill = {lightBlue[1] * mults[1], lightBlue[2] *mults[1], lightBlue[3] * mults[1], 0.6 + addition}
    local border ={lightBlue[1] * mults[2], lightBlue[2] * mults[2], lightBlue[3] * mults[2], 0.8 + addition}
    table.insert(colors, fill)
    table.insert(colors, border)
    return colors
end
local function getRectangle(room, entity, position, brighter)
    local colors = getColors(0.3, 0.8, brighter)
    local rectangle = utils.rectangle(position.x, position.y, 8, 8)
    local rect = drawableRectangle.fromRectangle("bordered", rectangle, colors[1], colors[2])
    return rect
end
local function getTriPoints(p1, p2, p3)
    return p1.x + 4, p1.y + 4, p2.x + 4, p2.y + 4, p3.x + 4, p3.y + 4
end

local function getTriangle(entity)
    local p1 = { x = entity.x or 0, y = entity.y or 0}
    local p2 = entity.nodes[1] or p1
    local p3 = entity.nodes[2] or p2
    local triangle =
    drawableFunction.fromFunction(function()
        drawing.callKeepOriginalColor(function()
        love.graphics.setColor({255 /255, 0/255, 0/255, 100/255})
        love.graphics.polygon("fill", getTriPoints(p1,p2,p3))
        end)
    end)
    return triangle
end

function polyScreenShiftArea.sprite(room, entity)
    local sprites = {}
    local pos = { x = entity.x, y = entity.y}
    local text = getText(0, pos, 8, 8)
    local rect = getRectangle(room, entity, pos, true)

    table.insert(sprites, getTriangle(entity))
    table.insert(sprites,rect)
    table.insert(sprites,text)
    return sprites
end
function polyScreenShiftArea.nodeSprite(room, entity, node, nodeIndex, viewport)
    local sprites = {}
    local pos = {x = node.x, y = node.y}
    local text = getText(nodeIndex, pos, 8, 8)
    local rect = getRectangle(room, entity, pos, false)
    table.insert(sprites,rect)
    table.insert(sprites,text)
    return sprites
end
function polyScreenShiftArea.selection(room, entity)
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

function polyScreenShiftArea.fieldInformation()
    return {
        bgTo = {
           options = fakeTilesHelper.getTilesOptions("tilesBg"),
           editable = false
        }
    }
end


return polyScreenShiftArea