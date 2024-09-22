local tilesColorgrade = {}

tilesColorgrade.name = "PuzzleIslandHelper/TilesColorgrade"

local blends = {"AlphaBlend","Additive","Opaque","NonPremultiplied"}
tilesColorgrade.defaultData = 
{
   colorgrade = "golden",
   fg = false,
   blend = "AlphaBlend"
}
tilesColorgrade.fieldInformation =
{
	blend = {
		options = blends,
		editable = false
	}
}
return tilesColorgrade