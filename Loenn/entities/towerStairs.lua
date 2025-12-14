local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLine = require("structs.drawable_line")
local xnaColors = require("consts.xna_colors")
local green = xnaColors.Lime
local black = xnaColors.Black
local logging = require("logging")
local towerStairs = {}

towerStairs.justification = { 0, 0 }

towerStairs.name = "PuzzleIslandHelper/TowerStairs"
towerStairs.depth = 3
towerStairs.placements =
{
    {
        name = "Tower Stairs",
        data = {
            width = 8,
            height = 8,
            collidableFlag = "",
            halfRevolutions = 3,
            requireUpInput = false,
            flagWhenOnSlope = "",
            flagState = true,
            xExtend = 8,
            platformWidth = 16,
            depth = 0,
            flipX = false,
            flipZ = false
        }
    }
}
local function getLine(entity, radius, from, to, offsetX, offsetY)
    
    local lerp = 1
    if to.z < 0 then
        lerp = 1 - (radius / math.abs(to.z))
    end
    return drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({lerp * 255,lerp * 255,lerp * 255})
            love.graphics.line({from.x + offsetX ,from.y + offsetY, to.x + offsetX,to.y + offsetY})
            end)
        end)
end
local function GetX(radius, y, halfWavelength)
    return radius * math.sin(y * math.pi / halfWavelength);
end
local function GetZ(radius, y, halfWavelength)
    return radius * math.cos(y * math.pi / halfWavelength)
end
local function GetPoint(radius, y, halfWavelength)
    return {x = GetX(radius, y, halfWavelength), y = y, z = GetZ(radius, y, halfWavelength)}
end
local function GetPoints(entity,radius, height, waveHeight)
    local points = {};
    local halfWave = waveHeight / 2
    local floors, blarg = math.modf(height / halfWave)
    for i = 0, floors do  
        for j = 0, halfWave do
            local point = GetPoint(radius, i * halfWave + j, halfWave)
            if entity.flipX then
                point.x *= -1;
            end
            if entity.flipZ then
                point.z *= -1;
            end
            table.insert(points,point)
        end
    end
    return points
end
function towerStairs.sprite(room, entity)
    local sprites = {}
    local radius = (entity.width or 16) / 2.0
    local height = entity.height or 16

    local points = GetPoints(entity,radius, height, height / (entity.halfRevolutions / 2.0))
    for i = 2, #points do
        local a = points[i-1]
        local b = points[i]
        if b.y <= height then
            table.insert(sprites,getLine(entity,radius, a, b, entity.x + radius, entity.y))
        end
    end
    return sprites
end
return towerStairs