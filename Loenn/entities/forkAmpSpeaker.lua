local drawableSprite = require("structs.drawable_sprite")
local forkAmpSpeaker= {}
forkAmpSpeaker.justification = { 0, 0 }
forkAmpSpeaker.name = "PuzzleIslandHelper/ForkAmpSpeaker"

forkAmpSpeaker.depth = -8500
forkAmpSpeaker.nodeLimits = {1, 1}
forkAmpSpeaker.nodeJustification = {0,0}
forkAmpSpeaker.nodeVisibility = "always"
forkAmpSpeaker.nodeLineRenderType = "line"
forkAmpSpeaker.nodeLineRenderOffset = {4,8}
local facings = {"Right","Left"}
forkAmpSpeaker.placements =
{
    {
        name = "Fork Amp Speaker",
        data =
        {
            facing = "Right",
            rateA = 0,
            rateB = 0,
            rateC = 0,
            rateD = 0,
            flag = ""
        }
    }
}
forkAmpSpeaker.fieldInformation = 
{
    facing =
    {
        options = facings,
        editable = false
    }
}
function forkAmpSpeaker.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/forkAmp/wirelessScreen00",entity)
    sprite:setJustification(0,0)
    return sprite
end
function forkAmpSpeaker.nodeSprite(room, entity, node, index, viewport)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/forkAmp/isolatedSpeaker", entity)
    local r = sprite:getRectangle()
    local spriteWidth = r.width
    local spriteHeight = r.height
    sprite:setJustification(0.5, 0.5)
    sprite:addPosition(spriteWidth / 2, spriteHeight / 2)
    sprite:addPosition(node.x - entity.x, node.y - entity.y)
    if entity.facing == "Left" then
        sprite:setScale(-1,1)
    end
    return sprite
end

return forkAmpSpeaker