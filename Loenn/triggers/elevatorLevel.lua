local elevatorLevel = {}

elevatorLevel.name = "PuzzleIslandHelper/ElevatorLevel"

elevatorLevel.placements =
{
    {
        name = "ElevLevel",
        data = {
            floorNumber = 1,
            elevatorID = ""
        }
    },
}
elevatorLevel.fieldInformation =
{
    floorNumber = 
    {
        fieldType = "integer"
    }
}

return elevatorLevel