local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local movingPlatform = {}

movingPlatform.justification = { 0, 0 }

movingPlatform.name = "PuzzleIslandHelper/MovingPlatform"
movingPlatform.nodeLimits = {1,1}
movingPlatform.nodeLineRenderType = "line"
movingPlatform.nodeVisibility = "always"
movingPlatform.minimumSize = {8,8}
movingPlatform.depth = -8501
local methods = {"Linear","Sine","Cube","Bounce","Elastic","Expo","Quad","Quint"}
local detect = {"Position","Start","Destination"}
local move =  {"OnPlayerNear","OnFlag","OnTouched","OnRiding","BackAndForth","OnDashed"}
local directions = {"In","Out","InOut"}
movingPlatform.fieldOrder = {
    "x","y","width","height","flag","stopFlag","volume","moveTime","detectRadiusX","detectRadiusY","playerDetectArea","moveMethod","ease","easeDirection","tiletype","outlineColor","canReturn","invertOnPlayerDetect","outlineDestination","isForPIPuzzle"
}
movingPlatform.placements =
{
    name = "Moving Platform",
    data = 
    {
        outlineColor = "000000",
        moveMethod = "OnFlag",
        stopFlag="",
        flag = "moving_platform",
        tiletype = "3",
        width = 8,
        height = 8,
        canReturn = true,
        moveTime = "2",
        ease = "Linear",
        easeDirection = "In",
        detectRadiusX = 40,
        detectRadiusY = 40,
        playerDetectArea = "Destination",
        invertOnPlayerDetect = false,
        outlineDestination = true,
        cameraLookAhead = true,
        gentleMode = false,
        isForPIPuzzle = false,
        volume = 1
        
    }
}

movingPlatform.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
        playerDetectArea = {
            editable = false,
            options = detect
        },
        ease = {
            editable = false,
            options = methods
        },
        easeDirection = {
            editable = false,
            options = directions
        },
        moveMethod = {
            editable = false,
            options = move
        },
        outlineColor = {
            fieldType = "color",
            allowXNAColors = true,
        }
    }
end
movingPlatform.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)
function movingPlatform.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y 
    local width, height = entity.width or 24, entity.height or 24

    return utils.rectangle(x, y, width, height), {utils.rectangle(nodeX, nodeY, width, height)}
end

return movingPlatform