local shakeController= {}
shakeController.justification = { 0, 0 }

shakeController.name = "PuzzleIslandHelper/SingularityShakeController"

shakeController.depth = -1

shakeController.texture = "objects/PuzzleIslandHelper/shakeController"

shakeController.placements =
{
    {
        name = "Singularity Shake Controller",
        data = 
        {
            mult = 1,
            flag = "",
            impact = 0.3,
            delay = 0.5,
            windUp = 0.4,
            charge = 0.5,
            recoilDelay = 0.3,
            recoil = 1
        }
    }
}

return shakeController