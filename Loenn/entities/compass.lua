local drawableSprite = require("structs.drawable_sprite")
local compass= {}

compass.justification = {0,0}
compass.name = "PuzzleIslandHelper/Compass"
compass.depth = 1
function compass.texture(room, entity)
    if entity.leader then
        return "objects/PuzzleIslandHelper/Compass/dialLonn"
    else
        return "objects/PuzzleIslandHelper/Compass/dialMini"
    end
end
local directions = {"Left","Right","Up","Down"}
compass.nodeLimits = {0, 1}
compass.nodeLineRenderType = "line"
compass.placements =
{
    {
        name = "Compass",
        data = 
        {
            compassID = "",
            flag = "",
            leader = true
        }
    }
}
return compass