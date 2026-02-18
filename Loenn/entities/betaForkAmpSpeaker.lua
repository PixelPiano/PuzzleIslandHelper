local drawableSprite = require("structs.drawable_sprite")
local betaForkAmpSpeaker= {}
betaForkAmpSpeaker.name = "PuzzleIslandHelper/BetaForkAmpSpeaker"
betaForkAmpSpeaker.depth = -8500
local facings = {"Right","Left"}
betaForkAmpSpeaker.placements =
{
    {
        name = "Beta Fork Amp Speaker",
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
betaForkAmpSpeaker.fieldInformation = 
{
    facing =
    {
        options = facings,
        editable = false
    }
}
function betaForkAmpSpeaker.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/PuzzleIslandHelper/forkAmp/isolatedSpeaker", entity)
    local r = sprite:getRectangle()
    local spriteWidth = r.width
    local spriteHeight = r.height
    sprite:setJustification(0.5, 0.5)
    sprite:addPosition(spriteWidth / 2, spriteHeight / 2)
    if entity.facing == "Left" then
        sprite:setScale(-1,1)
    end
    return sprite
end

return betaForkAmpSpeaker