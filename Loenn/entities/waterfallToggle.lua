local waterfallToggle = {}

waterfallToggle.name = "PuzzleIslandHelper/WaterfallToggle"
waterfallToggle.depth = 2000

function waterfallToggle.texture(room, entity)
    local onlyTrue = entity.onlyTrue
    local onlyFalse = entity.onlyFalse

    if onlyTrue then
        return "objects/coreFlipSwitch/switch13"

    elseif onlyFalse then
        return "objects/coreFlipSwitch/switch15"

    else
        return "objects/coreFlipSwitch/switch01"
    end
end

waterfallToggle.placements = {
    {
        name = "Waterfall Toggle",
        data = {
            onlyTrue = false,
            onlyFalse = false,
            persistent = false,
            iceFlag = "",
            inverted = false
        },
    }
}

return waterfallToggle