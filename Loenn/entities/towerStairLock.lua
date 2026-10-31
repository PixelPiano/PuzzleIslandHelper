local towerLock = {}

towerLock.name = "PuzzleIslandHelper/TowerStairLock"

local states = {"Decayed","Young"}
function towerLock.texture(room, entity)
    if entity.state == "Decayed" then
        return "objects/PuzzleIslandHelper/tower/heart00"
    else
        return "objects/PuzzleIslandHelper/tower/sigil00"
    end
end

towerLock.placements = {
    name = "Tower Stair Lock",
    data = {
        lockID = "",
        flagOnUnlocked = "",
        flagState = true,
        state = "Decayed"
    }
}
towerLock.fieldInformation = 
{
    state =
    {
        options = states,
        editable = false
    }
}
towerLock.depth = 2

return towerLock