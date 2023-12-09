local drawableSprite = require "structs.drawable_sprite"

local customPrologueBridge = {}

customPrologueBridge.name = "PuzzleIslandHelper/PIPrologueBridge"
customPrologueBridge.minimumSize = {8, 8}
customPrologueBridge.canResize = {true, false}

customPrologueBridge.fieldInformation = {
    delay = {
        minimumValue = 0.0
    },
    speed = {
        minimumValue = 0.0
    }
}

customPrologueBridge.placements = {
    {
        name = "prologue_bridge",
        type = "rectangle",
        data = {
            width = 8,
            height = 8,
            flag = "",
            left = false,
            delay = 0.2,
            speed = 0.8
        }
    }
}

local tiles = {
    "objects/customBridge/tile3",
    "objects/customBridge/tile4",
    "objects/customBridge/tile5",
    "objects/customBridge/tile6",
    "objects/customBridge/tile7",
    "objects/customBridge/tile8"
}

function customPrologueBridge.sprite(room, entity)
    local w = math.floor((entity.width or 8) / 8)

    local sprites = {}

    for i = 0, w - 1 do
        local tileSprite = drawableSprite.fromTexture(tiles[i % 6 + 1], entity)
        tileSprite:setJustification(0, 0)
        tileSprite:addPosition(i * 8, 0)

        table.insert(sprites, tileSprite)
    end

    return sprites
end

return customPrologueBridge
