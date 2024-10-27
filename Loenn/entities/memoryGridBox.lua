local memoryGridBox = {}

memoryGridBox.justification = { 0, 0 }

memoryGridBox.name = "PuzzleIslandHelper/MemoryGridBox"
memoryGridBox.texture = "objects/PuzzleIslandHelper/noiseSquare"
memoryGridBox.depth = 1

memoryGridBox.placements =
{
    {
        name = "Memory Grid Box",
        data = {
            isTouchCutscene = false
        }
    }
}

return memoryGridBox