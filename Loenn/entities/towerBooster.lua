local booster = {}

booster.name = "PuzzleIslandHelper/TowerBooster"
booster.depth = -8500
booster.nodeLimits = {1,1}
booster.placements = {
    {
        name = "Tower Booster",
        data = {
            baseRadius = 32,
            baseSpinSpeed = -60,
            baseWaveHeight = 20
        }
    }
}
booster.texture = "objects/booster/boosterRed00"

return booster