local keyDoor= {}
keyDoor.justification = { 0, 0 }

keyDoor.name = "PuzzleIslandHelper/KeyDoor"

keyDoor.depth = 1

keyDoor.texture = "objects/PuzzleIslandHelper/keyDoor/full"

local modes = {"Keys","Flag"}
keyDoor.placements =
{
    {
        name = "Key Door",
        data = 
        {
            room = "",
            marker = "",
            mode = "Keys",
            flag = "",

        }
    }
}
keyDoor.fieldInformation = 
{
    mode = 
    {
        options = modes,
        editable = false
    }
}

return keyDoor