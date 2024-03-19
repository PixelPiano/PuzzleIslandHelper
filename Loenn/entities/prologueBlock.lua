local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
local prologueBlock = {}

prologueBlock.justification = { 0, 0 }

prologueBlock.name = "PuzzleIslandHelper/PrologueBlock"
prologueBlock.minimumSize = {8,8}
prologueBlock.placements =
{
    name = "Prologue Block",
    data = 
    {
        tiletype = "w",
        width = 8,
        height = 8,
        order = -1,
        delay = 0,
        waitForController = false,
        fg = false
    }
}
function prologueBlock.depth(room, entity)
    if entity.fg then
        return -10001
    else
        return 5000
    end
end
prologueBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        order = {
            fieldType = "integer"
        }
    }
end
function prologueBlock.sprite(room, entity)
    local font = love.graphics.getFont()
    local text = ""
    if entity.order < 0 then
        text = "Instant"
    else
        text = ""..(entity.order or -1)
    end
    local result =
            drawableFunction.fromFunction(function()
                drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(text, entity.x, entity.y, entity.width, entity.height, font, 1)
                end)
            end)
    local sprites = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)(room, entity, "")
    table.insert(sprites,result)
    return sprites
end

return prologueBlock
