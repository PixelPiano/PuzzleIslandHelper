local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")
local slope = {}

slope.justification = { 0, 0 }

slope.name = "PuzzleIslandHelper/Slope"

slope.depth = -10001
slope.texture = "objects/PuzzleIslandHelper/slopeMarker"
slope.nodeLimits = {1,-1}
slope.nodeLineRenderType = "line"
slope.nodeVisibility = "always"
slope.canResize = {false, false}
slope.placements =
{
    {
        name = "Slope",
        data = {
            collidableFlag = "",
            flagWhenOnSlope = "",
            easeCombo = "",
            flagID = "",
            requireUpInput = false,
            xExtend = 8,
            platformWidth = 16,
            flagState = true,
            invertFlagWhenOffSlope = false,
            depth = 0,
            colorA = "FF0000",
            colorB = "FFFF00"
        }
    }
}
slope.fieldInformation =
{
    colorA =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    colorB =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    platformWidth =
    {
        fieldType = "integer"
    },
    xExtend =
    {
        fieldType = "integer"
    }
}
local function hex2rgb(hex)
    hex = hex:gsub("#","")
    return {tonumber("0x"..hex:sub(1,2)), tonumber("0x"..hex:sub(3,4)), tonumber("0x"..hex:sub(5,6))}
end
local function colorLerp(a, b, p)
 return (a + (b - a) * p);
end
function slope.selection(room, entity)
    local nodes = entity.nodes or {}
    local rectangles = {}
    local x, y = entity.x or 0, entity.y or 0
    for i, node in ipairs(nodes) do
        table.insert(rectangles, utils.rectangle(node.x - 2,node.y - 2,4,4))
    end
    return utils.rectangle(x - 2, y - 2, 4, 4), rectangles --{utils.rectangle(nodeX, nodeY, 8, 8)}
end

local function getLine(entity, from, to, percent)
    local cA = hex2rgb(entity.colorA) or {255,255,255}
    local cB = hex2rgb(entity.colorB) or {255,255,255}
    local r = colorLerp(cA[1],cB[1],percent)
    local g = colorLerp(cA[2],cB[2],percent)
    local b = colorLerp(cA[3],cB[3],percent)
    return drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({r,g,b})
            love.graphics.line({from.x ,from.y, to.x,to.y})
            end)
        end)
end

function slope.sprite(room, entity)
    local sprites = {}
    local nodes = entity.nodes or {}
    local index = 0.0
    local count = #nodes
    local from = {x = entity.x, y = entity.y}
    for i, node in ipairs(nodes) do
        local to = {x = node.x, y = node.y}
        table.insert(sprites, getLine(entity,from, to,index / count))
        from = to
        index = index + 1
    end
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/slopeMarker", entity)
    table.insert(sprites, sprite)
    return sprites
end
function slope.nodeSprite(room, entity)
    local sprites = {}
    local nodes = entity.nodes or {}
    for i, node in ipairs(nodes) do
        local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/slopeMarker", entity)
        sprite:addPosition(node.x - entity.x, node.y - entity.y)
        table.insert(sprites,sprite)
    end
    return sprites
end

return slope