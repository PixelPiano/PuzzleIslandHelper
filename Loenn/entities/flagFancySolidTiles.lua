local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

local solidTiles = {}

solidTiles.associatedMods = {"FancyTileEntities"}
solidTiles.name = "PuzzleIslandHelper/CustomFancySolidTiles"
solidTiles.placements = {
    name = "Flag Fancy Solid Tiles",
    data = {
        tileData = "0",
        flag = "",
        blendEdges = true,
        loadGlobally = false,
        onlyCheckFlagOnAwake = false,
        width = 8,
        height = 8
    }
}

solidTiles.fieldInformation = {
    tileData = {
        fieldType = "FancyTileEntities.buttonStringField"
    }
}

solidTiles.sprite = fancyTileEntitieshelper.getEntitySpriteFunction("blendEdges", "tilesFg", {1, 1, 1, 1})

return solidTiles