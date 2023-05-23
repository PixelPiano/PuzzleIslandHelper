local difficultyDisplay = {}
difficultyDisplay.justification = { 0, 0 }

difficultyDisplay.name = "PuzzleIslandHelper/DifficultyDisplay"

difficultyDisplay.depth = -1000001

difficultyDisplay.texture = "objects/PuzzleIslandHelper/difficultyDisplay/lonn"
difficultyDisplay.placements =
{
    {
        name = "Difficulty Display",
        data = 
        {
            difficultyLevel = 1,
        }
    }
}
difficultyDisplay.fieldInformation =
{
    background =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line2 =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line3 =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line4 =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}
return difficultyDisplay
