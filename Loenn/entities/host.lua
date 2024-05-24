local host = {}
local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
host.justification = { 0, 0 }

host.name = "PuzzleIslandHelper/Host"

host.depth = 1

host.texture = "objects/PuzzleIslandHelper/gameshow/host/lonn"

host.placements =
{
    {
        name = "Gameshow Host",
        data = {
        }
    }
}
return host