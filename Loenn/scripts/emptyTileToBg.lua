local tilesStruct = require("structs.tiles")
local fakeTilesHelper = require("helpers.fake_tiles")
local logging = require("logging")
local script = {
    name = "fillEmptyTiles",
    displayName = "Replace Very Empty Tiles",
    parameters = {
        to = "7",
    },
    fieldInformation = fakeTilesHelper.getFieldInformation("to","tilesBg"),
    tooltip = "Replaces any unoccupied tile in the selected layer with a bg tile",
    tooltips = {
        to = "The tileset ID which will be used",
    },
}
local function encodeString(str)
    return { innerText = str }
end

function script.run(room, args)
    local to = args.to or "7"

    local propertyName = "tilesBg"
    local fgTiles = tilesStruct.matrixToTileString(room["tilesFg"].matrix)
    local bgTiles = tilesStruct.matrixToTileString(room[propertyName].matrix)
    local layer = ""
    for i = 1, #fgTiles do
        local fg = fgTiles:sub(i,i)
        local bg = bgTiles:sub(i,i)
        if bg == '0' and fg == '0' then
            layer = layer .. "//Replace//"
        else
            layer = layer .. bg
        end
    end
    room[propertyName] = tilesStruct.decode(encodeString(string.gsub(layer, "//Replace//", to)))
end
return script