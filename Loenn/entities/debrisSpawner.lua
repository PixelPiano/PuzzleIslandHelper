local fakeTilesHelper = require("helpers.fake_tiles")
local debrisSpawner = {}

debrisSpawner.name = "PuzzleIslandHelper/DebrisSpawner"
debrisSpawner.fillColor = {0.4, 0.4, 1.0, 0.3}
debrisSpawner.borderColor = {0.4, 0.4, 1.0, 0.7}
debrisSpawner.placements =
{
    {
        name = "Debris Spawner",
        data = {
            width = 8,
            height = 8,
            tileType = "3",
            flag = "",
            inverted = false,
            onlyOncePerSession = false
        }
    },
}
debrisSpawner.fieldInformation = fakeTilesHelper.getFieldInformation("tileType")

return debrisSpawner