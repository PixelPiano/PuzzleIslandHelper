local steamEmitter = {}
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

steamEmitter.justification = { 0, 0 }

local directions = {"Up","Down","Left","Right"}
steamEmitter.name = "PuzzleIslandHelper/SteamEmitter"

steamEmitter.depth = -100000
steamEmitter.texture = "objects/PuzzleIslandHelper/steamEmitter/lonn"
steamEmitter.placements =
{
    {
        name = "Steam Emitter",
        data = {
             direction = "Up",
             interval = 0.1,
             flag = ""
        }
    }
}
steamEmitter.fieldInformation =
{
    direction = 
    {
        options = directions,
        editable = false
    }
}
return steamEmitter