local fakeTilesHelper = require("helpers.fake_tiles")

local flagConditionBlock = {}

flagConditionBlock.name = "PuzzleIslandHelper/FlagConditionBlock"
flagConditionBlock.placements = {
    name = "Flag Condition Block",
    data = {
        tileType = "3",
        useAnimatedTiles = true,
        flag = "",
        depth = -12999,
        width = 8,
        height = 8,
        blendIn = true
    }
}
function flagConditionBlock.depth(room, entity)
    return entity.depth
end
--flagConditionBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tileType", false, "tilesFg", {1.0, 1.0, 1.0, 0.7})
flagConditionBlock.sprite = function(room,entity,node)
    return fakeTilesHelper.getEntitySpriteFunction("tileType", "blendIn")(room,entity,node)
end
flagConditionBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return flagConditionBlock