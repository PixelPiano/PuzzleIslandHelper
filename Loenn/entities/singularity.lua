local singularity= {}
singularity.justification = { 0, 0 }

singularity.name = "PuzzleIslandHelper/Singularity"

singularity.depth = -8500
singularity.nodeLimits = {-1,-1}
singularity.texture = "objects/PuzzleIslandHelper/singularity"
singularity.justification = {0.5, 0.5}
local states = {"Idle","Dummy","Path","Slam","Flee","Launch"}
singularity.placements =
{
    {
        name = "Singularity",
        data = 
        {
            flag = "",
            startState = "Idle",
        }
    },
    {
        name = "Singularity (Path)",
        data =
        {
            flag = "",
            startState = "Path",
            counterIndex = "",
            pathFlag = "",
            pathID = "",
            naive = true,
            easeIn = true,
            spawnStardust = true,
            setEndFlagsIfRemoved = true,
            removeIfOutOfLevel = true,
            removeIfPathDoesNotExist= true,
            removeIfPathFlagFalse = true,
            breakObjects = true
        }
    }
}
singularity.fieldInformation = {
    startState = {
        options = states,
        editable = false
    },
}

return singularity