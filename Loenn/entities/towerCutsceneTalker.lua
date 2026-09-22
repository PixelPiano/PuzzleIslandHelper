local xnaColors = require("consts.xna_colors")

local lightBlue = xnaColors.LightBlue
local towerCutsceneTalker = {}

towerCutsceneTalker.name = "PuzzleIslandHelper/TowerCutsceneTalker"

towerCutsceneTalker.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
towerCutsceneTalker.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
local types = {"Backend","Transit","Temple"}
towerCutsceneTalker.placements = {
    name = "Tower Cutscene Talker",
    data = {
        width = 8,
        height = 8,
        type = "Backend",
        flag = ""
    }
}
towerCutsceneTalker.fieldInformation = 
{
    type = {
        options = types,
        editable = false
    }
}
towerCutsceneTalker.depth = 0

return towerCutsceneTalker