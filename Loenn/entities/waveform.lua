local drawableSprite = require("structs.drawable_sprite")
local textureTemp = "objects/PuzzleIslandHelper/waveform/sus"
local waveform = {}
waveform.justification = { 0, 0 }

waveform.name = "PuzzleIslandHelper/Waveform"

waveform.depth = -8500

waveform.texture = "objects/PuzzleIslandHelper/waveform/sus"

waveform.placements =
{
    {
        name = "Waveform",
        data = {
            flag = "waveformFlag",
            sprite = "objects/PuzzleIslandHelper/waveform/sus",
            event = "event:/PianoBoy/defaultWaveform",
            color = "Black",
        }
    }
}
waveform.fieldInformation =
{
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    }
}

return waveform
