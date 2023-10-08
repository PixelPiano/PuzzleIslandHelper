local parallaxWindowTarget= {}
parallaxWindowTarget.justification = { 0, 0 }
parallaxWindowTarget.name = "PuzzleIslandHelper/ParallaxWindowTarget"

parallaxWindowTarget.depth = -8500
parallaxWindowTarget.fillColor = {0, 1, 0, 0.4}
parallaxWindowTarget.borderColor = {0.2, 1, 0.2, 1.0}
parallaxWindowTarget.placements =
{
    {
        name = "Parallax Window Target",
        data =
        {
            width = 16,
            height = 16,
        }
    }
}


return parallaxWindowTarget