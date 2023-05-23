local digitalEffect = {}
digitalEffect.justification = { 0, 0 }

digitalEffect.name = "PuzzleIslandHelper/DigitalEffect"

digitalEffect.depth = -1000001

digitalEffect.texture = "objects/PuzzleIslandHelper/digitalEffect/digitalEffectIcon"
digitalEffect.placements =
{
    {
        name = "Digital Effect",
        data = 
        {
            background="008502",
            line="75d166",
            line2="00FF00",
            line3="3dbe28",
            line4="00ff40",
            lineFlicker = true,
            backFlicker = true,
        }
    }
}
digitalEffect.fieldInformation =
{
    background =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line2 =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line3 =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
    line4 =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}
return digitalEffect
