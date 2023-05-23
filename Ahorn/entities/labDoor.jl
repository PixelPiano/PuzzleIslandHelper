module PuzzleIslandHelperLabDoor
using ..Ahorn, Maple

@mapdef Entity "PuzzleIslandHelper/LabDoor" LabDoor(x::Integer, y::Integer, startState::Bool=false, automatic::Bool=false, flag::String="")

const placements = Ahorn.PlacementDict(
   "Lab Door (Puzzle Island Helper)" => Ahorn.EntityPlacement(
      LabDoor,
      "point"
   )
)



function Ahorn.selection(entity::LabDoor)
   x, y = Ahorn.position(entity)
   width = 6
   height = 48

   return Ahorn.Rectangle(x, y, width, height)
end

sprite = "objects/PuzzleIslandHelper/machineDoor/idle00"
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LabDoor, room::Maple.Room)
   Ahorn.drawSprite(ctx, sprite, 2, 24)
end

end