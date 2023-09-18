local communalHelper = require("mods").requireFromPlugin("libraries.communal_helper", "CommunalHelper")

local rotatingCassetteBlock = {}

local colorNames = communalHelper.cassetteBlockColorNames
local colors = communalHelper.cassetteBlockHexColors

rotatingCassetteBlock.name = "PuzzleIslandHelper/RotatingCassetteBlock"
rotatingCassetteBlock.minimumSize = {16, 16}
rotatingCassetteBlock.fieldInformation = {
    index = {
        options = colorNames,
        editable = false,
        fieldType = "integer"
    },
    customColor = {
        fieldType = "color"
    },
    tempo = {
        minimumValue = 0.0
    }
}

rotatingCassetteBlock.placements = {}
for i = 1, 4 do
    rotatingCassetteBlock.placements[i] = {
        name = string.format("rotating_cassette_block_%s", i - 1),
        data = {
            index = i - 1,
            tempo = 1.0,
            width = 16,
            height = 16,
            customColor = colors[i]
        }
    }
end

function rotatingCassetteBlock.sprite(room, entity)
    return communalHelper.getCustomCassetteBlockSprites(room, entity)
end

return rotatingCassetteBlock
