local wipEntity = {}

wipEntity.justification = { 0, 0 }

wipEntity.name = "PuzzleIslandHelper/WipEntity"
wipEntity.texture = "objects/PuzzleIslandHelper/wip/texture"
wipEntity.depth = 1

wipEntity.placements =
{
    {
        name = "Wip Entity",
        data = {
            color = "FFFFFF",
            floatValue = 0,
            boolValue = false,
            stringValue = "",
            name = ""
        }
    }
}
wipEntity.fieldInformation =
{
    color = 
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}
return wipEntity