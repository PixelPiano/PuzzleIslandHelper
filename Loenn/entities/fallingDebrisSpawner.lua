local fakeTilesHelper = require("helpers.fake_tiles")
local fallingDebrisSpawner = {}
fallingDebrisSpawner.depth = -12999
fallingDebrisSpawner.name = "PuzzleIslandHelper/FallingDebrisSpawner"
fallingDebrisSpawner.minimumSize = {8,8}
fallingDebrisSpawner.maximumSize = {8,8}
fallingDebrisSpawner.placements = {
    name = "Falling Debris Spawner",
    data = {
        width = 8,
        height = 8,
        tiletype = '1',
        flag = "",
        maxDebris = 2,
        loops = 1,
        minInterval = 0,
        maxInterval = 0.2,
        maxXOffset = 0,
        minXOffset = 0,
        maxXSpeed = 0,
        minXSpeed = 0,
        smallDebrisChance = 0.5,
        persistent = false,
        onlyOnce = false,
        onFlagActivated = true
    }
}
fallingDebrisSpawner.fieldInformation = function() 
    return {
        tiletype = {
            options = fakeTilesHelper.getTilesOptions(),
            editable = false
        },
    }
end
fallingDebrisSpawner.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype",false)

return fallingDebrisSpawner