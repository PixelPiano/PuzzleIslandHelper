local drawableSprite = require("structs.drawable_sprite")

local stickyNote = {}

stickyNote.name = "PuzzleIslandHelper/StickyNote"
stickyNote.fillColor = {0.4, 0.4, 1.0, 0.4}
stickyNote.borderColor = {0.4, 0.4, 1.0, 1.0}
stickyNote.nodeLimits = {1, 1}
stickyNote.nodeJustification = {0,0}
stickyNote.nodeVisibility = "always"
stickyNote.placements = {
    name = "Sticky Note",
    data = {
        width = 8,
        height = 8,
        userTexturePath = "",
        texturePath = "objects/PuzzleIslandHelper/stickyNote/idle",
        color = "ffff00",
        textureScale = 1,
        depth = 10
    }
}
stickyNote.fieldInformation = 
{
    color=
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}
function stickyNote.nodeTexture(room, entity, node, nodeIndex, viewport)
    return entity.texturePath
end

return stickyNote
