local trianglePortal = {}

trianglePortal.justification = { 0, 0 }

trianglePortal.name = "PuzzleIslandHelper/TrianglePortal"

trianglePortal.fillColor = {0.4, 0.4, 1.0, 0.4}
trianglePortal.borderColor = {0.4, 0.4, 1.0, 1.0}
trianglePortal.placements = {
    name = "Triangle Portal",
    data = {
        color = "00ff00",
        width = 32,
        height = 32,
        flag = "allDigitalAreasCleared",
        light1flag="portalLight1",
        light2flag="portalLight2",
        light3flag="portalLight3",
        usesFlags = false
    }
}


trianglePortal.fieldInformation = {
      color =
    {
        fieldType = "color",
        allowXNAColors = true,
    },
}

return trianglePortal
