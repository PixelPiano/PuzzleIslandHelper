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
            levels = 3,
            requireUpInput = false,
            flagWhenOnSlope = "",
            flagState = true,
            xExtend = 8,
            platformWidth = 16,
            depth = 0,
        }
    }
}
local function getLine(entity, from, to, lerp)
    return drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
            love.graphics.setColor({lerp * 255,lerp * 255, lerp * 255})
            love.graphics.line({from.x ,from.y, to.x,to.y})
            end)
        end)
end

function towerStairs.sprite(room, entity)
    local sprites = {}
    local radius = entity.width / 2.0
    local height = entity.height
    local halfWave = (height / entity.levels) / 2
    local origin = {x = entity.x + radius, y = entity.y}
    --table.insert(sprites,getRectangle(room, entity))
    for i = 0, entity.levels * 2 do
        local from = {x = radius * math.sin((i * halfWave) * math.pi / halfWave) + origin.x, y = i * halfWave + origin.y}
        for j = 1, halfWave do

            local to = {x = radius * math.sin((i * halfWave + j) * math.pi / halfWave) + origin.x, y = i * halfWave + j + origin.y}
            if to.y <= origin.y + height then
                local z = math.abs(radius * math.cos((i * halfWave + j) * math.pi / halfWave)) / radius
                local line = getLine(entity,from,to,z)
                from = to
                table.insert(sprites, line)
            end
        end
    end
    return sprites
end
local function getColors(from, fillAmount, borderAmount)
    local colors = {}
    local mults = {fillAmount, borderAmount}
    local fill = {from[1] * mults[1], from[2] *mults[1], from[3] * mults[1], 0.6}
    local border ={from[1] * mults[2], from[2] * mults[2], from[3] * mults[2], 0.8}
    table.insert(colors, fill)
    table.insert(colors, border)
    return colors
end
local function getRectangle(room, entity)
    local colors = getColors(black, 0.3, 0.8)
    local rectangle = utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    return drawableRectangle.fromRectangle("bordered", rectangle, colors[1], colors[2])
end
return towerStairs