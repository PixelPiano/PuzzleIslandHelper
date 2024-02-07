local cogDoorActivator= {}
cogDoorActivator.justification = { 0, 0 }
cogDoorActivator.name = "PuzzleIslandHelper/CogDoorActivator"
cogDoorActivator.depth = 0
cogDoorActivator.texture = "objects/PuzzleIslandHelper/cog/holder"
cogDoorActivator.placements =
{
    {
        name = "Cog Door Activator",
        data =
        {
            spins = 1,
            onlyOnce = false,
            doorID = ""
        }
    }

}


return cogDoorActivator