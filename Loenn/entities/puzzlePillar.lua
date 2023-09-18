local fakeTilesHelper = require("helpers.fake_tiles")

local puzzlePillar = {}

puzzlePillar.name = "PuzzleIslandHelper/PuzzlePillar"
puzzlePillar.depth = -13000
puzzlePillar.placements = {
    name = "PuzzlePillar",
    data = {
        tileType = "3",
        width = 8,
        height = 8,
        sprite = "A"
    }
}

puzzlePillar.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
puzzlePillar.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return puzzlePillar
