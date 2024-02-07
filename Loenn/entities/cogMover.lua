local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")
local cogMover = {}
cogMover.justification = {0.5,0.5}
cogMover.name = "PuzzleIslandHelper/CogMover"
cogMover.texture = "objects/PuzzleIslandHelper/cog/holderStart"
cogMover.nodeTexture = "objects/PuzzleIslandHelper/cog/holder"
cogMover.depth = 0
cogMover.nodeLimits = {1,1}
cogMover.nodeLineRenderType = "line"
cogMover.nodeLineRenderOffset = {8,8}
cogMover.nodeVisibility = "always"

cogMover.placements = {
    name = "Cog Mover",
    data = {
        onlyOnce = false,
        maxSpeed = 50,
        acceleration = 1
    }
}

return cogMover