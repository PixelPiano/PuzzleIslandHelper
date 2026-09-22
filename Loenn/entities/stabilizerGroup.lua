local drawableSprite = require "structs.drawable_sprite"
local utils = require("utils")
local stabilizerGroup = {}

stabilizerGroup.name = "PuzzleIslandHelper/StabilizerGroup"
stabilizerGroup.placements =
{
    {
        name = "Stabilizer Group",
        data = {
            groupID = "",
            combo = "0101",
            combos = "",
            persistent = false,
            isMachine = true,
            facing = "Left",
            validTrackingFlag = "",
            satelliteID = "",
            requiresWorldShiftPermissions = false
        }
    },
}
stabilizerGroup.fieldInformation =
{
    facing =
    {
        options = {"Left","Right"},
        editable = false
    },
}
function stabilizerGroup.sprite(room, entity)

    local sprites = {}
    local x = 0
    local xScale = entity.facing == "Right" and 1 or -1
    local machine = entity.isMachine
    local path = "objects/PuzzleIslandHelper/stabilizer/"
    if machine then
        local side = drawableSprite.fromTexture(path .. "edge",entity)
        side:setJustification(0, 0)
        side:addPosition(-side.meta.width,0)
        table.insert(sprites,side)
    end

    for c in entity.combo:gmatch"." do
        local sprite
        local spritePath
        if c == '1' then
             if machine then
                spritePath = path .. "cubbyOn"
             else
                spritePath = path .."on"
             end
        else
             if machine then
                spritePath = path .. "cubbyOff"
             else
                spritePath = path .."off"
             end
        end
        sprite = drawableSprite.fromTexture(spritePath, entity)
        sprite:setJustification(0.5, 0.5)
        sprite:addPosition(x + sprite.meta.width / 2, sprite.meta.height / 2)
        sprite:setScale(-xScale, 1)
        x = x + sprite.meta.width
        table.insert(sprites, sprite)
    end
    if machine then
        local sat = drawableSprite.fromTexture(path.."sat",entity)
        sat:setJustification(0.5,1)
        sat:addPosition(x / 2, 0)
        sat:setScale(xScale,1)
        table.insert(sprites,sat)

        local light = drawableSprite.fromTexture(path.."satTip",entity)
        light:setJustification(0.5,0)
        light:addPosition(x/2 - sat.meta.width / 2 + (8 + (4 + light.meta.width / 2) * xScale),-sat.meta.height + 4)
        light:setScale(xScale,1)
        table.insert(sprites,light)

        local sideB = drawableSprite.fromTexture(path .. "edge",entity)
        sideB:setJustification(0, 0)
        sideB:setScale(-1,1)
        sideB:addPosition(x + sideB.meta.width,0)
        table.insert(sprites,sideB)
    end
    return sprites
end
return stabilizerGroup