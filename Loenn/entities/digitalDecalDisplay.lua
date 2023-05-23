local drawableSprite = require("structs.drawable_sprite")

local digitalDecalDisplay = {}

digitalDecalDisplay.justification = { 0, 0 }

digitalDecalDisplay.name = "PuzzleIslandHelper/DigitalDecalDisplay"

digitalDecalDisplay.texture = "objects/PuzzleIslandHelper/speechBubble/lonn"

function digitalDecalDisplay.depth(room, entity)
    return entity.depth or 0
end
digitalDecalDisplay.placements = {
    name = "Digital Decal Display",
    data = {
        fps = 12.0,
        flag = "decal_flag",
        inverted = false,
        decalPath = "1-forsakencity/flag",
        offsetX = 0,
        offsetY = 0,
        shouldScale = true,
        glitch = true,
        playerDetectY = -1,
        playerDetectX = -1,
        color = "00FF00",
        depth = 1,
    }
}
digitalDecalDisplay.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return digitalDecalDisplay
