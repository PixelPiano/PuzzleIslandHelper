local singularityPathTrigger = {}

singularityPathTrigger.name = "PuzzleIslandHelper/SingularityPathTrigger"
singularityPathTrigger.placements =
{
    {
        name = "Singularity Path Trigger",
        data = {
            width = 16,
            height = 16,
            flag = "",
            pathID = "",
            onlyOnce = true,
            onEnter = true,
            onStay = true,
            onLeave = true
        }
    },
}
return singularityPathTrigger