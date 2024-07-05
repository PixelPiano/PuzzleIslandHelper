local drawableSprite = require("structs.drawable_sprite")
local ruinsDoor = {}
ruinsDoor.justification = { 0, 0 }

ruinsDoor.name = "PuzzleIslandHelper/RuinsDoor"

local doors = {"A","B","C","D","E"}

function ruinsDoor.texture(room, entity)
    if drawableSprite.fromTexture("objects/PuzzleIslandHelper/fadeWarp/ruinsHouseDoor" .. entity.doorType) ~= nil then
        return "objects/PuzzleIslandHelper/fadeWarp/ruinsHouseDoor" .. entity.doorType
    else
        return "objects/PuzzleIslandHelper/fadeWarp/ruinsHouseDoorA"
    end
end


function ruinsDoor.depth(room, entity)
    return entity.depth or 0
end

ruinsDoor.placements =
{
    name = "Ruins Door",
    data = {
        fps = 12.0,
        depth = 9001,
        color = "000000",
        roomName = "",
        targetDoorId = "",
        doorId = "",
        flipX = false,
        flag = "",
        keyId = -1,
        dialog = "",
        doorType = "A"
    }
}
ruinsDoor.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    doorType =
    {
        options = doors,
        editable = false,
    }
}

return ruinsDoor