local fakeTilesHelper = require("helpers.fake_tiles")

local exitBlock = {}

exitBlock.name = "PuzzleIslandHelper/PrologueGlitchBlock"
exitBlock.depth = -13000
exitBlock.placements = {
    name = "Prologue Glitch Block",
    data = {
        tileType = "3",     
        width = 8,
        height = 8,
    }
}

exitBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
exitBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return exitBlock
