local drawableSprite = require("structs.drawable_sprite")

local paper = {}

paper.name = "PuzzleIslandHelper/Paper"
paper.depth = 1
paper.fillColor = {0.4, 0.4, 1.0, 0.4}
paper.borderColor = {0.4, 0.4, 1.0, 1.0}
paper.nodeLimits = {1, 1}
paper.nodeJustification = {0,0}
paper.nodeVisibility = "always"
paper.placements = {
    name = "Paper",
    data = {
        width = 8,
        height = 8,
        dialogID = "put_id_here",
        texturePath = "objects/PuzzleIslandHelper/noteSprites/paperA",
        usesTexture = true
    }
}
function paper.nodeTexture(room, entity, node, nodeIndex, viewport)
    return entity.texturePath
end

return paper
