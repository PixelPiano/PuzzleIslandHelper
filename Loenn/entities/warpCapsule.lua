local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableLine = require("structs.drawable_line")
local logging = require("logging")
local warpCapsule = {}
warpCapsule.justification = { 0, 0 }
warpCapsule.name = "PuzzleIslandHelper/WarpCapsule"

warpCapsule.depth = 1
warpCapsule.nodeLimits = {1, 1}
warpCapsule.nodeJustification = {0,0}
warpCapsule.nodeTexture = "objects/PuzzleIslandHelper/digiWarpReceiver/terminal"
warpCapsule.nodeVisibility = "always"
warpCapsule.placements =
{
    {
        name = "Warp Capsule",
        data = {
            warpID = "",
            warpDelay = 0,
            disableMachineFlag = "",
            invertMachineFlag = false,
            disableFlag = "",
            invertFlag = false,
            rune = "",
            isDefaultRune = false,
            isPartOfFirstSet = false,
            runeDisplayWidth = 24,
            runeDisplayHeight = 16,
            isLaboratory = false,
        }
    }
}
local xFactor = 0.16666666666666666
local yFactor = 0.5
local indexOffsets = {{x = 1, y = 0},
                      {x = 3, y = 0},
                      {x = 5, y = 0},
                      {x = 0, y = 1},
                      {x = 2, y = 1},
                      {x = 4, y = 1},
                      {x = 6, y = 1},
                      {x = 1, y = 2},
                      {x = 3, y = 2},
                      {x = 5, y = 2}}

local function getIndexOffset(entity, index, width, height)
    local ox = entity.x
    local oy = entity.y-height
    local pair = indexOffsets[index + 1]
    return {x = ox + (pair.x * xFactor) * width, y = oy + (pair.y * yFactor) * height}
end
local function getIndexPositions(entity, width, height)
    local result = {}
    local list = entity.rune or "None"
    for token in list:gmatch("%S+") do
        if string.len(token) == 2 then
            local c1 = tonumber(token:sub(1,1))
            local c2 = tonumber(token:sub(2,2))
            local s = getIndexOffset(entity, c1, width, height)
            local e = getIndexOffset(entity, c2, width, height)
            table.insert(result, {a = s, b = e})
        end
    end
    return result
end
local function getLineSprites(entity, width, height)
    local lines = {}
    local ind = getIndexPositions(entity, width, height)
    if ind then 
        for i = 1, #ind, 1 do
            local p1 = ind[i].a
            local p2 = ind[i].b
            if p1 and p2 then
                table.insert(lines,getLine(p1,p2))
            else
                table.insert(lines,getFallbackLine(entity))
                return lines
            end
        end
    end
    return lines
end

function getLine(p1, p2)
    local line =
    drawableFunction.fromFunction(function()
        drawing.callKeepOriginalColor(function()
        love.graphics.setColor({255, 0, 0})
        love.graphics.line({p1.x, p1.y, p2.x, p2.y})
        end)
    end)
    return line
end
local function getFallbackLine(entity)
    local fallbackLine =
        drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({255, 0, 0})
            love.graphics.line({entity.x,entity.y, entity.x + 8,entity.y + 8})
            end)
        end)
    return fallbackLine
end

function warpCapsule.sprite(room, entity)
    local sprites = {}
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/digiWarpReceiver/lonn",entity)
    sprite:setJustification(0,0)
    table.insert(sprites, sprite)
    if not entity.prototype then
        local lines = getLineSprites(entity, 26, 16)
        for i, line in ipairs(lines) do
         table.insert(sprites, line)
        end
    end
    return sprites
end
return warpCapsule