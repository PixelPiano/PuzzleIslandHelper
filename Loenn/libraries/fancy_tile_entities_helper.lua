local matrix = require("utils.matrix")
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")

local fancyTileEntitiesHelper = {}


function fancyTileEntitiesHelper.getEntitySpriteFunction(blendKey, layer, color)
    
    return function(room, entity, node)
        local tileX, tileY = math.floor((entity.x or node.x or 0) / 8) + 1, math.floor((entity.y or node.y or 0) / 8) + 1
        local tileWidth, tileHeight = math.floor((entity.width or 8) / 8), math.floor((entity.height or 8) / 8)

        local tileTable = {}

        local data = entity.tileData:split("\n")

        for ty = 1, tileHeight do
            local row = data[ty]
            for tx = 1, tileWidth do
                local tile = "0"
                if row then
                    tile = row:sub(tx, tx) or "0"
                end

                if not tile or tile == "" or tile == "\r" then tile = "0" end

                tileTable[((ty-1)*tileWidth)+tx] = tile
            end
        end

        local tileMatrix = matrix.fromTable(tileTable, tileWidth, tileHeight)
        -- this kinda remakes the matrix so idk if this makes sense
        local fakeTiles = fakeTilesHelper.generateFakeTiles(room, tileX, tileY, tileMatrix, layer, entity[blendKey])

        local fakeTilesSprites = fakeTilesHelper.generateFakeTilesSprites(room, tileX, tileY, fakeTiles, layer, entity.x, entity.y, color)

        local drawableSprite
        drawableSprite = drawableRectangle.fromRectangle("bordered", utils.rectangle(entity.x, entity.y, entity.width or 8, entity.height or 8), {0, 0, 0, 0}, {0, 0, 0})
        drawableSprite.depth = entity.depth
        
        --table.insert(drawables, drawableSprite)
        table.insert(fakeTilesSprites, 1, drawableSprite)
        return fakeTilesSprites
    end
end

return fancyTileEntitiesHelper