local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local linkedFallingBlock = {}

linkedFallingBlock.justification = { 0, 0 }

linkedFallingBlock.name = "PuzzleIslandHelper/LinkedFallingBlock"
linkedFallingBlock.minimumSize = {8,8}
linkedFallingBlock.depth = -8501
linkedFallingBlock.placements =
{
    name = "Lame Falling Block",
    data = 
    {
        linkId = "",
        tiletype = "3",
        width = 8,
        height = 8,
        finalBoss = false,
        behind = false
    }
}

linkedFallingBlock.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
linkedFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return linkedFallingBlock