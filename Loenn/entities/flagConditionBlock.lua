local fakeTilesHelper = require("helpers.fake_tiles")

local flagConditionBlock = {}

flagConditionBlock.name = "PuzzleIslandHelper/FlagConditionBlock"
flagConditionBlock.depth = -13000
flagConditionBlock.placements = {
    name = "Flag Condition Block",
    data = {
        tileType = "3",
        useAnimatedTiles = true,
        flag = "",   
        width = 8,
        height = 8,
        blendIn = true
    }
}

--flagConditionBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
flagConditionBlock.sprite = function(room,entity,node)
    return fakeTilesHelper.getEntitySpriteFunction("tileType", "blendIn")(room,entity,node)
end
flagConditionBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return flagConditionBlock