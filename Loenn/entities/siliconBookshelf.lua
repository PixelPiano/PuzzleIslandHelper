local siliconBookshelf = {}

siliconBookshelf.name = "PuzzleIslandHelper/SiliconBookshelf"
siliconBookshelf.depth = 2
siliconBookshelf.texture = "objects/PuzzleIslandHelper/siliconBookshelf/bookshelf"
siliconBookshelf.justification = {0,0}
siliconBookshelf.placements = {
    name = "Silicon Bookshelf",
    data = {
        colorA = "000000",
        colorB = "FFFFFF"
    }
}
siliconBookshelf.fieldInformation =
{
    colorA =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    colorB =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return siliconBookshelf