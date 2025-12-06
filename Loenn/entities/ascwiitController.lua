local ascwiitController= {}
ascwiitController.justification = { 0, 0 }

ascwiitController.name = "PuzzleIslandHelper/AscwiitController"

ascwiitController.depth = 1

ascwiitController.texture = "objects/PuzzleIslandHelper/ascwiit/controller"

ascwiitController.placements =
{
    {
        name = "Ascwiit Controller",
        data = 
        {
            allowLeft = true,
            allowRight = true,
            groupID = "",
            disableFlag = "",
            removeAscwiitsIfWrongWay = true,
            removeAscwiitsIfDisabled = true
        }
    }
}
return ascwiitController