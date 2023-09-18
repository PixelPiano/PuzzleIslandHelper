local drawableSprite = require("structs.drawable_sprite")
local _q = {}
_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/FadeWarpKey"

_q.depth = -8500

function _q.texture(room, entity)
    if entity.standing then
        return "objects/PuzzleIslandHelper/fadeWarp/key(Standing)"
    else
        return "objects/PuzzleIslandHelper/fadeWarp/key(Flat)"
    end
end

function _q.depth(room, entity)
    return entity.depth or 0
end

_q.placements =
{
    {
        name = "Custom Warp Key",
        data = {
            standing = false,
            depth = 0,
            keyId = 0
        }
    }
}

return _q