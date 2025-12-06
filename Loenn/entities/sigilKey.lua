local sigilKey= {}
sigilKey.justification = { 0, 0 }
sigilKey.name = "PuzzleIslandHelper/Column/SigilKey"

sigilKey.depth = -8500
sigilKey.fillColor = {0.4, 0.4, 1.0, 0.4}
sigilKey.borderColor = {0.4, 0.4, 1.0, 1.0}
sigilKey.nodeLimits = {1, 1}
sigilKey.nodeTexture = "objects/PuzzleIslandHelper/sigil/wip"
sigilKey.nodeVisibility = "always"
sigilKey.nodeJustification = {0,0}
sigilKey.placements =
{
    {
        name = "Sigil Key",
        data =
        {
            width = 16,
            height = 16,
            texture = "objects/PuzzleIslandHelper/sigil/wip",
            key = 'a',
            behind = false,
            depth = 0,

        }
    }

}


return sigilKey