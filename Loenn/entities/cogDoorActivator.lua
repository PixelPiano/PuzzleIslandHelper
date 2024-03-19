local gearDoorActivator= {}
gearDoorActivator.justification = { 0, 0 }
gearDoorActivator.name = "PuzzleIslandHelper/GearDoorActivator"
gearDoorActivator.depth = 0
gearDoorActivator.texture = "objects/PuzzleIslandHelper/gear/holder"
gearDoorActivator.placements =
{
    {
        name = "Gear Door Activator",
        data =
        {
            spins = 1,
            onlyOnce = false,
            doorID = ""
        }
    }

}


return gearDoorActivator