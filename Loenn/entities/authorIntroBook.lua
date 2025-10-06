local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local authorIntroBook= {}
authorIntroBook.justification = { 0, 0 }

authorIntroBook.name = "PuzzleIslandHelper/AuthorIntroBook"

authorIntroBook.depth = 1

authorIntroBook.texture = "objects/PuzzleIslandHelper/redBook"
authorIntroBook.nodeLimits = {1, 1}
authorIntroBook.nodeLineRenderType = "line"
authorIntroBook.placements =
{
    {
        name = "Author Intro Book",
        data = 
        {
            positionFlag= ""
        }
    }
}


return authorIntroBook