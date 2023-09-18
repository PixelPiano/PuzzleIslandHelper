local escapeTimer = {}
escapeTimer.justification = { 0, 0 }

escapeTimer.name = "PuzzleIslandHelper/EscapeTimer"

escapeTimer.depth = -10100

escapeTimer.texture = "objects/PuzzleIslandHelper/escapeTimer/lonn"

escapeTimer.placements =
{
    {
        name = "Escape Timer",
        data = {
        startFrom = 120
        }
    }
}

return escapeTimer