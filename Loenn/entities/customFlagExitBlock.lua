local fakeTilesHelper = require("helpers.fake_tiles")

local exitBlock = {}

exitBlock.name = "PuzzleIslandHelper/CustomFlagExitBlock"
exitBlock.depth = -13000
exitBlock.placements = {
    name = "Custom Exit Block",
    data = {
        tileType = "3",
        flag = "flag_exit_block",
        inverted = false,
        playSound = true,
        instant = false,
        width = 8,
        height = 8,
        audioEvent = "event:/game/general/passage_closed_behind",
        forceGlitchEffect = false
    }
}

exitBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
exitBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return exitBlock
