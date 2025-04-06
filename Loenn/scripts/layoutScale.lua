local tilesStruct = require("structs.tiles")
local fakeTilesHelper = require("helpers.fake_tiles")
local logging = require("logging")
local script = {
    name = "layoutScale",
    displayName = "Layout Scale",
    parameters = {
        factor = 1,
    },
    tooltip = "Scales Tiles and Entities in a room",
    tooltips = {
        factor = "The number to scale the room by",
    },
}
local function encodeString(str)
    return { innerText = str }
end

function script.run(room, args)
    local factor = math.floor(args.factor or 1)
    if factor <= 0 then end
    local propertyName = "tilesBg"
    local fgTiles = tilesStruct.matrixToTileString(room["tilesFg"].matrix)
    local bgTiles = tilesStruct.matrixToTileString(room[propertyName].matrix)
    local function scaleTiles(input, scalefactor)
        local output = ""
        local temp = ""
        for i = 1, #input do
            local tile = input:sub(i,i)
            if tile == '\n' then
                for j = 1, scalefactor do
                    output = output + temp
                    if j < scalefactor then
                        output = output + '\n'
                    end
                end
                temp = ""
            else
                temp = temp + tile
            end
        end
        return output
    end
    room.width = room.width * factor
    room.height = room.height * factor
    room["tilesBg"] = tilesStruct.decode(encodeString(scaleTiles(bgTiles,factor)))
    room["tilesFg"] = tilesStruct.decode(encodeString(scaleTiles(fgTiles,factor)))
end
--     local layer = ""
--     for i = 1, #fgTiles do
--         local fg = fgTiles:sub(i,i)
--         local bg = bgTiles:sub(i,i)
--         if bg == '0' and fg == '0' then
--             layer = layer .. "//Replace//"
--         else
--             layer = layer .. bg
--         end
--     end
--     room[propertyName] = tilesStruct.decode(encodeString(string.gsub(layer, "//Replace//", to)))
-- end
return script