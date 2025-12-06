local firfilContainer = {}
firfilContainer.justification = { 0, 0 }
firfilContainer.name = "PuzzleIslandHelper/FirfilContainer"
firfilContainer.texture = "objects/PuzzleIslandHelper/firfil/container"
firfilContainer.depth = 1

firfilContainer.placements =
{
    {
        name = "Firfil Container",
        data = {
            flag = "",
            flagOnCollected = "",
            collectable = false,
            instantCollect = false,
            persistent = true
        }
    }
}
return firfilContainer