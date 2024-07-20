local shaderEntityTest = {}
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

shaderEntityTest.justification = { 0, 0 }

shaderEntityTest.name = "PuzzleIslandHelper/ShaderEntityTest"

shaderEntityTest.depth = 1
shaderEntityTest.texture = "objects/PuzzleIslandHelper/invert/glassOrbFilled"

shaderEntityTest.placements =
{
    {
        name = "Shader Entity Test",
        data = {
            shaderPath = "PuzzleIslandHelper/Shaders/glassStatic"
        }
    }
}
return shaderEntityTest