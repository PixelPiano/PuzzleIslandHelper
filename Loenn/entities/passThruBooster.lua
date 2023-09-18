local booster = {}

booster.name = "PuzzleIslandHelper/PassThruBooster"
booster.depth = -8500
booster.placements = {
    {
        name = "Pass Thru Booster (Green)",
        data = {
            red = false,
            aboveFG = false,
            ch9_hub_booster = false
        }
    },
    {
        name = "Pass Thru Booster (Red)",
        data = {
            red = true,
            aboveFG = false,
            ch9_hub_booster = false
        }
    }
}

function booster.texture(room, entity)
    local red = entity.red

    if red then
        return "objects/booster/boosterRed00"

    else
        return "objects/booster/booster00"
    end
end

return booster