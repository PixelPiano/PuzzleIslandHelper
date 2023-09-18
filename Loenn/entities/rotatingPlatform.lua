local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local rotatingPlatform = {}

rotatingPlatform.justification = { 0, 0 }

rotatingPlatform.name = "PuzzleIslandHelper/RotatingPlatform"
rotatingPlatform.minimumSize = {8,8}
rotatingPlatform.depth = 0
local methods = {"Linear","Sine","Cube","Bounce","Elastic","Expo","Quad","Quint"}
local detect = {"Position","Start","Destination"}
local move =  {"OnPlayerNear","OnFlag","OnTouched","OnRiding","BackAndForth","OnDashed"}
local directions = {"In","Out","InOut"}
rotatingPlatform.fieldOrder = {
    "x","y","width","height","flag","moveTime","detectRadiusX","detectRadiusY","playerDetectArea","moveMethod","ease","easeDirection","tiletype","outlineColor","canReturn","invertOnPlayerDetect"
}
rotatingPlatform.placements =
{
    name = "Rotating Platform",
    data = 
    {
        outlineColor = "000000",
        moveMethod = "OnFlag",
        flag = "rotating_platform",
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
        invertOnPlayerDetect = false
        
    }
}

rotatingPlatform.fieldInformation = function() 
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
rotatingPlatform.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return rotatingPlatform