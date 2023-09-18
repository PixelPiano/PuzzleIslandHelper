local _q = {}
_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/FadeWarpTarget"

_q.depth = -8500

_q.texture = "objects/PuzzleIslandHelper/fadeWarp/target"

_q.placements =
{
    {
        name = "Fade Warp Target",
        data = {
        targetId = "Target",
        placePlayerOnGroundBelow = true
        }
    }
}

return _q