local fluidBottle= {}
fluidBottle.justification = { 0, 0 }

fluidBottle.name = "PuzzleIslandHelper/FluidBottle"

fluidBottle.depth = -8500

fluidBottle.texture = "objects/PuzzleIslandHelper/potion/potion00"

local effects = {"Sticky","Hot","Bouncy","Invert","Refill"}
fluidBottle.placements =
{
    {
        name = "Fluid Bottle",
        data = 
        {
            effect = "Sticky",
            reinforced = false,
            color = "0000FF"
        }
    }
}
fluidBottle.fieldInformation = 
{
    effect =
    {
        editable = false,
        options = effects
    },
    color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}
return fluidBottle