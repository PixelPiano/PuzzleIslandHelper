local tilesStruct = require("structs.tiles")

local script = {
    name = "emptyTileToBg",
    displayName = "Replace Very Empty Tiles",
    parameters = {
        layer = "Fg",
        to = "7",
    },
    fieldInformation = {
        layer = {
            fieldType = "loennScripts.dropdown",
            options = {
                "Fg", "Bg"
            }
        }
    },
    fieldOrder = { "to" },
    tooltip = "Replaces all truely empty tiles with another",
    tooltips = {
        layer = "The layer to replace tiles in",
        to = "The tileset ID which will be placed",
    },
}

local function encodeString(str)
    return { innerText = str }
end

function script.run(room, args)
    local to = args.to or "7"

    local fgTiles = tilesStruct.decode(room.fgTiles)
    local bgTiles = tilesStruct.decode(room.bgTiles)
    local layer
    if args.layer == "Fg" then
        layer = fgTiles
    end
    if args.layer == "Bg" then
        layer = bgTiles
    end
    for i = 1, #layer do
        if fgTiles[i] == '0' and bgTiles[i] == '0' then
            layer[i] = to
        end
    end
    if args.layer == "Fg" then
        room.tilesFg.matrix = layer.matrix
    end
    if args.layer == "Bg" then
        room.tilesBg = tilesStruct.decode(encodeString(tilesStruct.matrixToTileString(layer.matrix)))
    end
end

return script