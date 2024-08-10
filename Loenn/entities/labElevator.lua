local labElevator = {}
labElevator.justification = { 0, 0 }

labElevator.name = "PuzzleIslandHelper/LabElevator"

labElevator.depth = -8500
local events = {"ButtonsStuck","Broken","Default"}
labElevator.canResize = {false, false}
labElevator.texture = "objects/PuzzleIslandHelper/labElevator/lonn"
labElevator.nodeLimits = {0, -1}
labElevator.nodeLineRenderType = "line"
labElevator.nodeVisibility = "always"
labElevator.placements =
{
    {
        name = "Lab Elevator",
        data = {
            moveSpeed = 3,
            defaultFloor = 0,
            snapToClosestFloorOnSpawn = false,
            reliesOnLabPower = true,
            event = "Default"
        }
    }
}
labElevator.fieldInformation = {
    event ={
        options = events,
        editable = false
    }
}

return labElevator