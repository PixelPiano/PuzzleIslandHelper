local booster = {}

booster.name = "PuzzleIslandHelper/PrologueBooster"
booster.depth = -8500
booster.placements = {
    {
        name = "Prologue Booster",
        data = {
            red = true,
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