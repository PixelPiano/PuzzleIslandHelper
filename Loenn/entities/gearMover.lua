local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local gearMover = {}
gearMover.justification = {0.5,0.5}
gearMover.name = "PuzzleIslandHelper/GearMover"
gearMover.texture = "objects/PuzzleIslandHelper/gear/holderStart"
gearMover.nodeTexture = "objects/PuzzleIslandHelper/gear/holder"
gearMover.depth = 0
gearMover.nodeLimits = {1,1}
gearMover.nodeLineRenderType = "line"
gearMover.nodeLineRenderOffset = {8,8}
gearMover.nodeVisibility = "always"

gearMover.placements = {
    name = "Gear Mover",
    data = {
        onlyOnce = false,
        maxSpeed = 50,
        acceleration = 1
    }
}

return gearMover