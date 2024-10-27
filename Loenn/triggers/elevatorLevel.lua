local elevatorLevel = {}

elevatorLevel.name = "PuzzleIslandHelper/SetElevatorFloor"

elevatorLevel.placements =
{
    {
        name = "Elevator Floor Trigger",
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