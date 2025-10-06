local imageOverlay= {}
imageOverlay.justification = { 0, 0 }
imageOverlay.name = "PuzzleIslandHelper/ImageOverlay"
function imageOverlay.texture(room, entity)
    return entity.decalPath or "decals/1-forsakencity/flag"
end
function imageOverlay.depth(room, entity)
    return entity.depth or 0
end
imageOverlay.placements =
{
    {
        name = "Image Overlay",
        data = 
        {
            decalPath = "decals/1-forsakencity/flag",
            overlayPath = "decals/1-forsakencity/flag",
            depth = 1,
            flag = ""
        }
    }
}
return imageOverlay