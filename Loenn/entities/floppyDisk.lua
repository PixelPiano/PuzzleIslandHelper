local drawableSprite = require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local floppyDisk= {}

floppyDisk.justification = { 0, 0 }
floppyDisk.name = "PuzzleIslandHelper/FloppyDisk"

floppyDisk.depth = -8500

local function getEntityColor(entity)
    local rawColor = entity.color or "FFFFFF"
    local color = utils.getColor(rawColor) or xnaColors.LightSkyBlue

    return color
end
function floppyDisk.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/floppy/laying",entity)
    sprite:setColor(getEntityColor(entity))
    sprite:addPosition(4, 4)
    return sprite
end
floppyDisk.fieldInformation = {
    color = {
        fieldType = "color",
        allowXNAColors = true,
    },
    --data = {
      --  fieldType = "PuzzleIslandHelper.objectList",
    --}
}
floppyDisk.placements =
{
    {
        name = "Floppy Disk",
        data =
        {
            data = {},
            preset = "Default",
            color = "FFFFFF"
        }
    }
}

return floppyDisk