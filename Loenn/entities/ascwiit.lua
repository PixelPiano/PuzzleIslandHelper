local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local ascwiit= {}
ascwiit.justification = { 0, 0 }

ascwiit.name = "PuzzleIslandHelper/Ascwiit"

ascwiit.depth = 1

ascwiit.texture = "objects/PuzzleIslandHelper/ascwiit/lonn"

local flyFacings = {"Default","Unchanged","Random","Left","Right"}
ascwiit.placements =
{
    {
        name = "Ascwiit",
        data = 
        {
            scale = 1,
            flyFacing = "Default",
            flag = "",
            pathID = "",
            pathFlag = "",
            persistent = false,
            onlyFlagOnAdded = false,
            useSequenceDirection = false,
            eatedFirfil = false,
            fleesFromPlayer = false,
            scared = false,
            peck = true,
            hop = true,
            facePlayer = false,
            chirp = true,
            sapFlag = "",
            snapToGround = true,
            startingState = "Idle",
            flyingStability = 0,
        }
    }
}
ascwiit.fieldOrder = {
    "x","y","scale","flyingStability","flag","pathID","pathFlag","sapFlag","persistent","onlyFlagOnAdded","useSequenceDirection",
    "eatedFirfil","fleesFromPlayer","scared","dummy","peck","hop","chirp","facePlayer","snapToGround","flyFacing","startingState"
}
local states = {"Idle","Flee","Path","FlyTo","Dummy"}
ascwiit.fieldInformation = 
{
    flyFacing = {
        options = flyFacings,
        editable = false
    },
    startingState = {
        options = states,
        editable = false
    }
}
function ascwiit.scale(room, entity)
     return {1.0 * (entity.scale or 1), 1.0 * (entity.scale or 1)}

end


return ascwiit