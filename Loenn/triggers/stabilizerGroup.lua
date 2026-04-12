local stabilizerGroup = {}

stabilizerGroup.name = "PuzzleIslandHelper/StabilizerGroup"
stabilizerGroup.nodeLimits = {1, -1}
stabilizerGroup.nodeLineRenderType = "fan"
stabilizerGroup.nodeVisibility = "always"
stabilizerGroup.placements =
{
    {
        name = "Stabilizer Group",
        data = {
            groupID = "",
            combo = "0101",
            locked = false
        }
    },
}


return stabilizerGroup