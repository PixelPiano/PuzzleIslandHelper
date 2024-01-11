local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local audienceMember= {}
audienceMember.justification = { 0, 0 }

audienceMember.name = "PuzzleIslandHelper/AudienceMember"

audienceMember.depth = -8500

local path = "objects/PuzzleIslandHelper/gameshow/audience/"
audienceMember.placements =
{
    {
        name = "Audience Member",
        data = 
        {
            faceType = "stapler"
        }
    }
}
audienceMember.fieldInformation =
{
    faceType =
    {
        options = {"angry","ok","stapler","uwu"},
        editable = false
    }
}
function audienceMember.sprite(room, entity)
    return drawableSprite.fromTexture(path .. entity.faceType .. "/" .. entity.faceType .. "Face00", entity)
end
return audienceMember