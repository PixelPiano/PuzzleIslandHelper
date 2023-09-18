local _q = {}
_q.justification = { 0, 0 }

_q.name = "PuzzleIslandHelper/FadeToColor"

_q.depth = -8500

_q.texture = "objects/PuzzleIslandHelper/fadeToColor/fade"

_q.placements =
{
    {
        name = "Fade To Color",
        data = {
        flag = "fade_to_color",
        color = "000000",
        speed = 4,
        onEnter = false,
        useFlag = true
        }
    }

}
_q.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return _q