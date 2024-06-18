local roomStruct = require("structs.room")
local decalStruct = require("structs.decal")
local drawableSprite = require("structs.drawable_sprite")

local script = {
    name = "decalsToDecalTarget",
    displayName = "Decals to Decal Target",
    tooltip = "Replaces decals with Decal Targets",
    parameters = {
        groupId = ""
    }
}

local function doLayer(layer, args, room, isFG)
    local entities = {}
    for _, decal in ipairs(layer) do

        local entity = {}
        entity._name = "PuzzleIslandHelper/DecalEffectTarget"
        entity.decalPath = string.gsub(decal.texture,"decals/","")
        entity.groupId = args.groupId
        entity.x = decal.x
        entity.y = decal.y
        entity.scaleX = decal.scaleX or 1
        entity.scaleY = decal.scaleY or 1
        entity.rotation = decal.rotation or 0
        entity.fps = 12
        if isFG then
            entity.depth = -10500
        else
            entity.depth = 9000
        end
        table.insert(entities,entity)
    end
    for _, entity in ipairs(entities) do
      table.insert(room.entities, entity)
    end
end

function script.run(room, args)
    doLayer(room.decalsFg, args, room, true)
    room.decalsFg = {}
    doLayer(room.decalsBg, args, room, false)
    room.decalsBg = {}
end

return script
