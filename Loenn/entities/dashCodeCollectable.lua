local dashCodeCollectable= {}
dashCodeCollectable.justification = { 0, 0 }
dashCodeCollectable.name = "PuzzleIslandHelper/DashCodeCollectable"

dashCodeCollectable.depth = -8500
dashCodeCollectable.fillColor = {0.4, 0.4, 1.0, 0.4}
dashCodeCollectable.borderColor = {0.4, 0.4, 1.0, 1.0}
dashCodeCollectable.nodeLimits = {1, 1}
dashCodeCollectable.nodeJustification = {0,0}
dashCodeCollectable.nodeTexture = "objects/PuzzleIslandHelper/dashCodeCollectable/dccEntityIcon"
dashCodeCollectable.nodeVisibility = "always"
dashCodeCollectable.placements =
{
    {
        name = "Dash Code Collectable",
        data =
        {
            width = 16,
            height = 16,
            code = "U,U,L",
            event = "event:/new_content/game/10_farewell/glitch_short",
            sprite = "objects/PuzzleIslandHelper/dashCodeCollectable/miniHeart",
            flagOnCollected = "dash_code_collected",
            canRespawn = true,
            flagDebug = true,
            usesBounds = true,
            visibleBounds = true,
            noCollectable = false

        }
    }

}


return dashCodeCollectable