local roomStruct = require("structs.room")
local decalStruct = require("structs.decal")
local drawableSprite = require("structs.drawable_sprite")

local script = {
    name = "decalToHousePart",
    displayName = "Decal to House Part",
    tooltip = "Replaces house part decals with House Part entities",
    parameters = {
        outline = false,
        parameterToMakeTheBoxStopDisappearing = false
    }
}

local function doLayer(layer, args, room, isFG)
    local entities = {}
    for _, decal in ipairs(layer) do

        if(string.find(decal.texture, "PianoBoy/house/")) then
             local entity = {}
            entity._name = "PuzzleIslandHelper/HousePart"
            entity.decalPath = string.gsub(decal.texture,"decals/PianoBoy/house/","")
            entity.x = decal.x
            entity.y = decal.y
            entity.scaleX = decal.scaleX or 1
            entity.scaleY = decal.scaleY or 1
            entity.rotation = decal.rotation or 0
            entity.color = decal.color
            entity.outline = args.outline
            entity.rotationRate = 0
            if isFG then
                entity.depth = -10500
            else
                entity.depth = 9000
            end
            table.insert(entities,entity)
        end
       
    end
    for _, entity in ipairs(entities) do
      table.insert(room.entities, entity)
    end
end

function script.run(room, args)
    doLayer(room.decalsFg, args, room, true)
    doLayer(room.decalsBg, args, room, false)
end

return script
