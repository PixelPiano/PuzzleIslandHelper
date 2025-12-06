local mods = require("mods")
local fancyTileEntitieshelper = mods.requireFromPlugin("libraries.fancy_tile_entities_helper")

local solidTiles = {}

solidTiles.associatedMods = {"FancyTileEntities"}
solidTiles.name = "PuzzleIslandHelper/CustomFancyDashBlock"
solidTiles.placements = {
    name = "Fancy Fancy Dash Block",
    data = {
        tileData = "0",
        flag = "",
        setFlagTo= true,
        blendin = true,
        permanent = true,
        canDash = true,
        canBoost = true,
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