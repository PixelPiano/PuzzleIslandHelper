local _q = {}
_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/ChainedMonitor"

_q.depth = 0

_q.texture = "objects/PuzzleIslandHelper/chains/lonn"

_q.placements =
{
    {
        name = "Chained Monitor",
        data = 
        {
            flag = "invert",
            flagOnComplete= "chainedComp1"
        }
    }
}

return _q