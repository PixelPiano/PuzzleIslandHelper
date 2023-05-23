module PuzzleIslandHelperCustomWater

using ..Ahorn, Maple

@mapdef Entity "PuzzleIslandHelper/CustomWater" CustomWater(
    x::Integer, y::Integer, displacementFlag::String="",invertFlag::Bool=false
)

const placements = Ahorn.PlacementDict(
    "Custom Water (Puzzle Island Helper)" => Ahorn.EntityPlacement(
        CustomWater,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::CustomWater) = 8, 8
Ahorn.resizable(entity::CustomWater) = true, true

Ahorn.selection(entity::CustomWater) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomWater, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.0, 0.0, 1.0, 0.4), (0.0, 0.0, 1.0, 1.0))
end

end