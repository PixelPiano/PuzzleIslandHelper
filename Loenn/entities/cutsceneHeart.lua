local drawableSprite = require("structs.drawable_sprite")

local cutsceneHeart= {}
cutsceneHeart.justification = {0,0}
cutsceneHeart.name = "PuzzleIslandHelper/CutsceneHeart"
cutsceneHeart.depth = 1
local spriteNames = {"blue","green","red","tetris"}
cutsceneHeart.placements =
{
    {
        name = "Cutscene Heart",
        data =
        {
            event = "event:/game/07_summit/gem_get",
            sprite= "green",
            room = "",
            returnRoom = "",
            flag = "greenMiniHeart",
            teleportsPlayer=true,
            flipped = false,
        }
    }
}
cutsceneHeart.fieldInformation =
{
    sprite =
    {
        options = spriteNames,
        editable = false
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