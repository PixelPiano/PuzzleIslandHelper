local enums = require("consts.celeste_enums")

local birdNpc = {}

birdNpc.name = "PuzzleIslandHelper/PrologueBird"
birdNpc.depth = -1000000
birdNpc.nodeLineRenderType = "line"
birdNpc.justification = {0.5, 1.0}
birdNpc.texture = "characters/bird/crow00"
birdNpc.nodeLimits = {0, -1}
birdNpc.fieldInformation = {
    mode = {
        options = enums.bird_npc_modes,
        editable = false
    }
}
birdNpc.placements = {
    name = "prologue bird",
    data = {
    }
}

function birdNpc.scale(room, entity)
    return -1, 1
end

return birdNpc