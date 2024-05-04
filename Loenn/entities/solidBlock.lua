local fakeTilesHelper = require("helpers.fake_tiles")
local solidBlock = {}

solidBlock.justification = { 0, 0 }

solidBlock.name = "PuzzleIslandHelper/SolidBlock"
solidBlock.minimumSize = {8,8}
solidBlock.depth = -8501
solidBlock.placements =
{
    name = "Solid Block",
    data = 
    {
        tiletype = "3",
        width = 8,
        height = 8,
    }
}

solidBlock.fieldInformation =
{
    tiletype = {
        options = fakeTilesHelper.getTilesOptions(),
        editable = false
    }
}
solidBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)
return solidBlock