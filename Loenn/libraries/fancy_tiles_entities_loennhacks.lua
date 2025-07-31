-- No longer required cause field.metadata.formData exists
--local form = require('ui.forms.form')

--local FTELoennHacks = {}

--if form._fte_unloadSeq then form._fte_unloadSeq() end 

--local _orig_form_getFormFields = form.getFormFields
--form.getFormFields = function(data, options)
--    local elements = _orig_form_getFormFields(data, options) -- returns list of `element`
--    for _,v in ipairs(elements) do
--        if options.fields[v.name].fte_passData then
--            v.fte_entityRef = data -- Passes a reference to the dummyData as the value `element.fte_entityRef`
--        end
--    end
--    return elements
--end


--function form._vivh_unloadSeq() -- Handles hotreload.
--    form.getFormFields = _orig_form_getFormFields
--end

--return FTELoennHacks