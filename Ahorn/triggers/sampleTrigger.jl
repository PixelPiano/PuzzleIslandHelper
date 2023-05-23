module PuzzleIslandHelperSampleTrigger

using ..Ahorn, Maple

@mapdef Trigger "PuzzleIslandHelper/SampleTrigger" SampleTrigger(
    x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    sampleProperty::Integer=0
)

const placements = Ahorn.PlacementDict(
    "Sample Trigger (PuzzleIslandHelper)" => Ahorn.EntityPlacement(
        SampleTrigger,
        "rectangle",
    )
)

end