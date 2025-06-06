local fireBarrier = {}

fireBarrier.name = "PuzzleIslandHelper/FlagFireBarrier"
fireBarrier.fillColor = {209 / 255, 9 / 255, 1 / 255, 102 / 255}
fireBarrier.borderColor = {246 / 255, 98 / 255, 18 / 255}
fireBarrier.placements = {
    name = "Flag Fire Barrier",
    data = {
        width = 8,
        height = 8,
        flag = "",
        inverted = false
    }
}

fireBarrier.depth = -8500

return fireBarrier