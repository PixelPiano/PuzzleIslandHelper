local drawableSprite = require("structs.drawable_sprite")
local _q = {}
_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/FadeWarp"

_q.depth = -8500

local doors = {"A","B","C","D","E"}
function _q.texture(room, entity)
    if drawableSprite.fromTexture("objects/PuzzleIslandHelper/fadeWarp/ruinsHouseDoor" .. entity.doorType) ~= nil then
        return "objects/PuzzleIslandHelper/fadeWarp/ruinsHouseDoor" .. entity.doorType
    else
        return "objects/PuzzleIslandHelper/fadeWarp/ruinsHouseDoorA"
    end
end


function _q.depth(room, entity)
    return entity.spriteDepth or 0
end

function _q.ignoredFields(entity)
    if entity.actsLikeDoor then
        return {"actsLikeDoor","_name","_id"}
    else
        return {"actsLikeDoor","_name","_id","doorType"}
    end
end

_q.placements =
{
    {
        name = "Custom Warp (Normal)",
        data = {
        fps = 12.0,
        spriteDepth = 1,
        color = "000000",
        --decalPath = "1-forsakencity/flag",
        fadeSpeed = 1,
        roomName = "",
        useWipeInstead = false,
        usesTarget = false,
        targetId = "Target",
        usesSprite = true,
        flipX = false,
        flag = "fade_warp_flag",
        usesFlag = false,
        actsLikeDoor = false
        }
    },
    {
        name = "Custom Warp (Door)",
        data = {
            fps = 12.0,
            spriteDepth = 9001,
            color = "000000",
            --decalPath = "1-forsakencity/flag",
            fadeSpeed = 1,
            roomName = "",
            useWipeInstead = false,
            usesTarget = false,
            targetId = "Target",
            usesSprite = true,
            flipX = false,
            flag = "fade_warp_flag",
            usesFlag = false,
            actsLikeDoor = true,
            keyId = -1,
            dialog = "",
            doorType = "A"
        }
    }
}
_q.fieldInformation =
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

return _q