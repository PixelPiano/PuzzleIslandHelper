local stringField = require("ui.forms.fields.string")
local programType = {}
programType.fieldType = "PuzzleIslandHelper.programType"
local validProgramTypes =
{
    "text","invalid","info","wave","access","fountain","life","pipe"
}
function programType.getElement(name, value, options)
    -- Add extra options and pass it onto the string field
    options.displayTransformer = tostring
    options.options = validProgramTypes
    options.editable = false
    local formField = stringField.getElement(name, value, options)
    return formField
end
return programType
