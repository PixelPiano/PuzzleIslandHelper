local drawableSprite = require("structs.drawable_sprite")

local cutsceneHeart= {}
cutsceneHeart.justification = {0,0}
cutsceneHeart.name = "PuzzleIslandHelper/CutsceneHeart"
cutsceneHeart.depth = 1
cutsceneHeart.placements =
{
    {
        name = "Cutscene Heart",
        data =
        {
            event = "event:/game/07_summit/gem_get",
            sprite= "green",
            room = "",
            flag = "greenMiniHeart",
            teleportsPlayer=true
        }
    }
}
function cutsceneHeart.texture(room, entity)
    if drawableSprite.fromTexture("objects/PuzzleIslandHelper/cutsceneHeart/" .. entity.sprite .. "00") ~= nil then
        return "objects/PuzzleIslandHelper/cutsceneHeart/" .. entity.sprite .. "00"
    else
        return"objects/PuzzleIslandHelper/cutsceneHeart/" .. entity.sprite
    end
end

return cutsceneHeart