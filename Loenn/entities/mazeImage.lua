local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local mazeImage = {}
mazeImage.name = "PuzzleIslandHelper/MazeImage"
mazeImage.depth = 1

mazeImage.placements = {
    name = "Maze Image",
    data = {
        displayConnectors = false,
        displayNodes = false,
        displayMarkers = false
    }
}

function mazeImage.sprite(room, entity)
    
    local sprites = {}
    local path = "objects/PuzzleIslandHelper/maze/"
    table.insert(sprites,drawableSprite.fromTexture(path + "background",entity))
    if entity.displayConnectors then
        table.insert(sprites,drawableSprite.fromTexture(path + "connectors",entity))
    end
    if entity.displayMarkers then
        table.insert(sprites,drawableSprite.fromTexture(path + "markers",entity))
    end    
    if entity.displayNodes then
        table.insert(sprites,drawableSprite.fromTexture(path + "nodes",entity))
    end
    table.insert(sprites, drawableSprite.fromTexture(path + "border",entity))
    return sprites
end

return mazeImage