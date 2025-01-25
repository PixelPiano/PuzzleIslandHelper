local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local labTubeLight = {}
local path = "objects/PuzzleIslandHelper/machines/gizmos/tubeLight"
labTubeLight.name = "PuzzleIslandHelper/LabTubeLight"
labTubeLight.depth = 2000
function labTubeLight.minimumSize(room, entity)
    local f = entity.facing or "Down"
    if f == "Up" or f == "Down" then
        return {16, 8}
    else
        return {8, 16}
    end
end
function labTubeLight.canResize(room,entity)
    local f = entity.facing or "Down"
    if f == "Up" or f == "Down" then
        return {true, false}
    else
        return {false, true}
    end
end
local facings = {"Down","Up","Left","Right"}
labTubeLight.placements = {
    name = "Lab Tube Light",
    data = {
        width = 16,
        height = 16,
        digital = false,
        broken = false,
        facing = "Down",
    }
}
local function getSprite(path,entity, x, y, quadX,quadY,scaleX,scaleY,rotation)
    local sprite = drawableSprite.fromTexture(path, entity)
    sprite:setJustification(0, 0)
    sprite:setOffset(0, 0)
    sprite:addPosition(x,y)
    sprite:useRelativeQuad(quadX, quadY, 8, 8)
    sprite:setScale(scaleX,scaleY)
    sprite.rotation = rotation
    return sprite
end
-- Manual offsets and justifications of the sprites
function labTubeLight.sprite(room, entity)
    local sprites = {}
    local f = entity.facing or "Down"
    local p = path;
    if entity.digital then
        p = path .. "Digi"
    elseif entity.broken then
        p = path .. "BrokenLonn"
    end
    local rotation = 0
    local length
    local scaleX = 1
    local scaleY = 1
    local offX = 0
    local offY = 0
    if f == "Up" then
        scaleY = -1
        offY = 8
    elseif f == "Right" then
        scaleX = -1
        rotation = -math.pi / 2
    elseif f == "Left" then
        offX = 8
        rotation = math.pi / 2
    end
    local xm = 1
    local ym = 1
    local horizontal = f == "Up" or f == "Down"
    if horizontal then
        ym = 0
        length = math.max(entity.width or 0, 8)
    else
        xm = 0
        length = math.max(entity.height or 0, 8)
    end

    table.insert(sprites, getSprite(p,entity,offX,offY,0,0,scaleX,scaleY,rotation))
    for i = 8, length - 16, 8 do
        table.insert(sprites, getSprite(p,entity,i * xm + offX,i * ym + offY,8,0,scaleX,scaleY,rotation))
    end
    table.insert(sprites, getSprite(p,entity,(length - 8) * xm + offX,(length - 8) * ym + offY,16,0,scaleX,scaleY,rotation))
    return sprites
end

function labTubeLight.selection(room, entity)
    if entity.facing == "Down" or entity.facing == "Up" then
        return utils.rectangle(entity.x, entity.y, math.max(entity.width, 16),8)
    else
        return utils.rectangle(entity.x, entity.y,8,math.max(entity.height, 16))
    end
end
labTubeLight.fieldInformation =
{
    facing = {
        options = facings,
        editable = false
    }
}
return labTubeLight