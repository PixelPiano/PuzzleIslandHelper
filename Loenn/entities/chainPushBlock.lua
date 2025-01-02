local fakeTilesHelper = require("helpers.fake_tiles")

local exitBlock = {}

exitBlock.name = "PuzzleIslandHelper/ChainPushBlock"
exitBlock.depth = -13000
exitBlock.placements = {
    name = "Chain Push Block",
    data = {
        tileType = "3",
        flag = "flag_exit_block",
        inverted = false,
        width = 8,
        height = 8,
        corner = 0
    }
}

exitBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
exitBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return exitBlock
