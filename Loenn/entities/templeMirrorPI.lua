local drawableSprite = require("structs.drawable_sprite")

local templeMirrorPortal = {}

templeMirrorPortal.name = "PuzzleIslandHelper/TempleMirrorPI"
templeMirrorPortal.depth = -1999
templeMirrorPortal.placements = {
    name = "Temple Mirror PI",
}

local frameTexture = "objects/temple/portal/portalframe"
local torchTexture = "objects/temple/portal/portaltorch00"

local torchOffset = 90

function templeMirrorPortal.sprite(room, entity)
    local frameSprite = drawableSprite.fromTexture(frameTexture, entity)
    local torchSpriteLeft = drawableSprite.fromTexture(torchTexture, entity)
    local torchSpriteRight = drawableSprite.fromTexture(torchTexture, entity)

    torchSpriteLeft:addPosition(-torchOffset, 0)
    torchSpriteLeft:setJustification(0.5, 0.75)

    torchSpriteRight:addPosition(torchOffset, 0)
    torchSpriteRight:setJustification(0.5, 0.75)

    local sprites = {
        frameSprite,
        torchSpriteLeft,
        torchSpriteRight
    }

    return sprites
end


return templeMirrorPortal