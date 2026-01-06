local statid= {}
statid.justification = { 0, 0 }

statid.name = "PuzzleIslandHelper/Statid"


statid.texture = "objects/PuzzleIslandHelper/statid/petal"
function statid.depth(room, entity)
    return entity.depth
end
    
statid.placements =
{
    {
        name = "Statid",
        data = 
        {
            petals = 4,
            digital = false,
            color = "FFFFFF",
            shadeColor = "000000",
            depth = 2,
            dead = false,
            petalSize = 1,
            scaleRange = 0,
            shadeBasedOnDepth = true,
            thoughtPath = "",
            flagOnSapped = "",
            canProduceSap = false
        }
    }
}
statid.fieldInformation =
{
    color =  
    {
        fieldType = "color",
        allowXNAColors = true
    },
    shadeColor =  
    {
        fieldType = "color",
        allowXNAColors = true
    },

}
return statid