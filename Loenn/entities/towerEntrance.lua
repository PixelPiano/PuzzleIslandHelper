local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local towerEntrance = {}

towerEntrance.name = "PuzzleIslandHelper/TowerEntrance"

towerEntrance.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
towerEntrance.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
local states = {"Decayed","Young"}
local facings = {"Right","Left"}
towerEntrance.canResize = {false, true}
towerEntrance.minimumSize = {24, 16}
towerEntrance.placements = {
    name = "Tower Entrance",
    data = {
        width = 24,
        height = 16,
        exit = false,
        requiresKey = false,
        entranceID = "",
        destinationID ="",
        cutsceneID = "",
        cutsceneTriggerFlags = "",
        flagsOnCutscene = "",
        facing = "Right",
        state = "Decayed"
    }
}
towerEntrance.fieldInformation = 
{
    facing = 
    {
        options = facings,
        editable = false
    },
    state =
    {
        options = states,
        editable = false
    }
}
towerEntrance.depth = 1

return towerEntrance