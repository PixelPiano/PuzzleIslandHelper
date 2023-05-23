module PuzzleIslandHelperWaveform
using ..Ahorn, Maple

@mapdef Entity "PuzzleIslandHelper/Waveform" Waveform(x::Integer, y::Integer, duration::Float, flag::String="")

const placements = Ahorn.PlacementDict(
   "Lab Door (Puzzle Island Helper)" => Ahorn.EntityPlacement(
      Waveform,
      "point"
   )
)



function Ahorn.selection(entity::Waveform)
   x, y = Ahorn.position(entity)
   width = 6
   height = 48

   return Ahorn.Rectangle(x, y, width, height)
end

sprite = "objects/PuzzleIslandHelper/machineDoor/idle00"
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Waveform, room::Maple.Room)
   Ahorn.drawSprite(ctx, sprite, 2, 24)
end

end