local customIceBlock = {}

customIceBlock.name = "PuzzleIslandHelper/FlagIceBlock"
customIceBlock.fillColor = {76 / 255, 168 / 255, 214 / 255, 102 / 255}
customIceBlock.borderColor = {108 / 255, 214 / 255, 235 / 255}
customIceBlock.placements = {
    name = "Flag Ice Block",
    data = {
        width = 8,
        height = 8,
        flag = "",
        inverted = false
    }
}

customIceBlock.depth = -8500

return customIceBlock