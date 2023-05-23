module PuzzleIslandHelperPuzzleBonfireLight
using ..Ahorn, Maple

@mapdef Entity "PuzzleIslandHelper/PuzzleBonfireLight" PuzzleBonfireLight(x::Integer, y::Integer, lightColor::String="DB7093",lightFadeStart::Integer=32,lightFadeEnd::Integer=64,bloomRadius::Number=32.0, baseBrightness::Number=0.5, brightnessVariance::Number=0.5, flashFrequency::Number=0.25, wigglerDuration::Number=4.0, wigglerFrequency::Number=0.2, photosensitivityConcern::Bool=false, backwardsCompatibility::Bool=true)

const placements = Ahorn.PlacementDict(
   "Puzzle Bonfire Light (Puzzle Island Helper)" => Ahorn.EntityPlacement(
      PuzzleBonfireLight,
      "point"
   )
)



function Ahorn.selection(entity::PuzzleBonfireLight)
   x, y = Ahorn.position(entity)
   x -= 3
   y -= 0
   width = 6
   height = 4

   return Ahorn.Rectangle(x, y, width, height)
end

sprite = "objects/PuzzleIslandHelper/puzzleBonfireLight/idle00"
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PuzzleBonfireLight, room::Maple.Room)
   Ahorn.drawSprite(ctx, sprite, 0, 2)
end

end