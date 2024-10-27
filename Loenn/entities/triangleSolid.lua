local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")

local triSolid = {}
triSolid.depth = -10001
triSolid.name = "PuzzleIslandHelper/TriangleSolid"
triSolid.nodeLimits = {2,3}
triSolid.lineRenderType = "line"
triSolid.nodeVisibility = "always"
triSolid.canResize = {false, false}
triSolid.minimumSize = {8,8}
triSolid.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
triSolid.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
triSolid.placements = {
    name = "Triangle Solid",
    data = {
        width = 8,
        height = 8,
    }
}
function getText(text, position, width, height)
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, position.x, position.y, width or 8, height or 8, font, 1)
                end)
            end)
    return result
end
function getColors(fillAmount, borderAmount, brighter)
    local colors = {}
    local addition = brighter and 0.2 or 0.0
    local mults = {fillAmount + addition, borderAmount + addition}
    local fill = {lightBlue[1] * mults[1], lightBlue[2] *mults[1], lightBlue[3] * mults[1], 0.6 + addition}
    local border ={lightBlue[1] * mults[2], lightBlue[2] * mults[2], lightBlue[3] * mults[2], 0.8 + addition}
    table.insert(colors, fill)
    table.insert(colors, border)
    return colors
end
function getRectangle(room, entity, position, brighter)
    local colors = getColors(0.3, 0.8, brighter)
    local rectangle = utils.rectangle(position.x, position.y, 8, 8)
    local rect = drawableRectangle.fromRectangle("bordered", rectangle, colors[1], colors[2])
    return rect
end
function getIndexPositions( entity)
    local result = {}
    local entityPos = {x = entity.x, y = entity.y}
    local list = entity.indices or ""
    for token in list:gmatch("[^,%s%p]+") do -- Match any characters except ',', whitespace, and punctuation marks
        local num = tonumber(token) or 0
        if num <= 0 or num > #entity.nodes then
            table.insert(result, entityPos)
        else
            table.insert(result, entity.nodes[num])
        end
    end
    return result
end

function getTriangleSprites(entity)
    local lines = {}
    local ind = getIndexPositions(entity)
    if ind then 
        local maxColors = (#ind / 3)
        local count = 0
        for i = 3, #ind, 3 do
            local p1 = ind[i - 2]
            local p2 = ind[i-1]
            local p3 = ind[i]
            local color = getColor(maxColors, count)    
            count = count + 1
            if p1 and p2 and p3 then
                table.insert(lines,getTriangle(p1,p2,p3, color))
            else
                table.insert(lines,getFallbackLine(entity))
                return lines
            end
        end
    end
    return lines
end

function getTriPoints(p1, p2, p3)
    return p1.x + 4, p1.y + 4, p2.x + 4, p2.y + 4, p3.x + 4, p3.y + 4
end

function getTriangle(p1, p2, p3, color)
    local triangle =
    drawableFunction.fromFunction(function()
        drawing.callKeepOriginalColor(function()
        love.graphics.setColor({color.r /255, color.g/255, color.b/255, 100/255})
        love.graphics.polygon("fill", getTriPoints(p1,p2,p3))
        end)
    end)
    return triangle
end
function getFallbackLine(entity)
    local fallbackLine =
        drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({255, 0, 0})
            love.graphics.line({entity.x,entity.y, entity.x + 8,entity.y + 8})
            end)
        end)
    return fallbackLine
end
function getColor(maxColors, num)
    local color = {r = 255, g = 0, b = 0}
    local amount = (255 * 3) * (num / maxColors)
    for i = 1, amount do
        if (color.r > 0 and color.b == 0) then
            color.r = color.r - 1
            color.g = color.g + 1
        end
        if (color.g > 0 and color.r == 0) then
            color.g = color.g - 1
            color.b = color.b + 1
        end
        if (color.b > 0 and color.g == 0) then
            color.r = color.r + 1
            color.b = color.b - 1
        end
    end
    return color
end

function triSolid.sprite(room, entity)
    local sprites = {}
    local pos = { x = entity.x, y = entity.y}
    local text = getText(0, pos, 8, 8)
    local rect = getRectangle(room, entity, pos, true)
    local tris = getTriangleSprites(entity)
    for i, tri in ipairs(tris) do
         table.insert(sprites, tri)
    end
    table.insert(sprites,rect)
    table.insert(sprites,text)
    return sprites
end
function triSolid.nodeSprite(room, entity, node, nodeIndex, viewport)
    local sprites = {}
    local pos = {x = node.x, y = node.y}
    local text = getText(nodeIndex, pos, 8, 8)
    local rect = getRectangle(room, entity, pos, false)
    table.insert(sprites,rect)
    table.insert(sprites,text)
    return sprites
end
function triSolid.selection(room, entity)
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


return triSolid