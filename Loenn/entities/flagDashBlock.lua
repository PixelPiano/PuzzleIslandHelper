local fakeTilesHelper = require("helpers.fake_tiles")

local flagDashBlock = {}

flagDashBlock.name = "PuzzleIslandHelper/FlagDashBlock"
flagDashBlock.depth = -13000
flagDashBlock.placements = {
    name = "Flag Dash Block",
    data = {
        flag = "flag_dash_block"
    }
}

flagDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
flagDashBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return flagDashBlock
