local tilesShader = {}

tilesShader.name = "PuzzleIslandHelper/TilesShader"

local op = {"Foreground","Background"}
tilesShader.defaultData = 
{
   effect = "",
   tiles = "Foreground"
}
tilesShader.fieldInformation = 
{
	tiles = {
		options = op,
		editable = false
	}
}
return tilesShader