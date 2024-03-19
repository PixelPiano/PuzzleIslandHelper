local backdropSafetyCheck = {}

backdropSafetyCheck.name = "PuzzleIslandHelper/BackdropSafetyCheck"

local areas = {"Forest","Resort","Backend","Pipes"}
backdropSafetyCheck.placements =
{
    {
        name = "Backdrop Safety Check",
        data = {
            area = "Forest"
        }
    },
}
backdropSafetyCheck.defaultData = 
{
    area = "Forest"
}

backdropSafetyCheck.fieldInformation = 
{
    area = 
    {
        options = areas,
        editable = false
    }
}

return backdropSafetyCheck