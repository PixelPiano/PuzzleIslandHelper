local grapherEntity = {}
grapherEntity.justification = { 0, 0 }

grapherEntity.name = "PuzzleIslandHelper/GrapherEntity"

grapherEntity.depth = -1000001

grapherEntity.texture = "objects/PuzzleIslandHelper/graph/graphEntityIcon"

grapherEntity.placements =
{
    {
        name = "Grapher Entity",
        data = {
            timeMod = 0.2,
            Colorgrade = "PianoBoy/Inverted",
            lineWidth = 1,
            size = 10,
            color = "Black",
        }
    }
}
grapherEntity.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return grapherEntity
