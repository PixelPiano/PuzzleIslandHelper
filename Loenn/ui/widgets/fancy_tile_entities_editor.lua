local matrix = require("utils.matrix")
local fakeTilesHelper = require("helpers.fake_tiles")
local loadedState = require("loaded_state")
local configs = require("configs")

local brushHelper = require("brushes")
local ui = require("ui")
local form = require("ui.forms.form")
local uiUtils = require("ui.utils")
local uiElements = require("ui.elements")
local utils = require("utils")
local listWidgets = require("ui.widgets.lists")
local toolHandler = require("tools")

local TileEditor = {}

local UPSCALE = 2
-- remove (for debug purposes)

function dump(o)
    function dump_inner(o, i)
        if i > 2 then return "too much recursion" end
        if type(o) == 'table' then
            local s = '{ '
            for k,v in pairs(o) do
                if type(k) ~= 'number' then k = '"'..k..'"' end
                s = s .. '['..k..'] = ' .. dump_inner(v, i+1) .. ','
            end
            return s .. '} '
        else
            return tostring(o)
        end
    end
    return dump_inner(o, 0)
end

local function printMatrix(matrix)
    local width, height = matrix:size()
    local out = ""
    for ty = 1, height do
        for tx = 1, width do
            out = out .. matrix:get(tx, ty)
        end
        out = out .. "\n"
    end
    print(out)
end

local function areaInteraction(interactionData)
    return function(widget, x, y)
        local innerX = math.floor(utils.clamp(x - widget.screenX, 0, interactionData.areaWidth)/((UPSCALE)*8))+1
        local innerY = math.floor(utils.clamp(y - widget.screenY, 0, interactionData.areaHeight)/((UPSCALE)*8))+1

        if interactionData.tileData:get(innerX, innerY) ~= interactionData.currentMaterial then
            interactionData.tileData:set(innerX, innerY, interactionData.currentMaterial)
            widget:reflow()
            widget:redraw()
            interactionData.callback(interactionData.tileData)
        end
    end
end

local function areaDrawing(interactionData)
    return function(orig, widget)
        orig(widget)
        local room = loadedState.getSelectedItem()

        local widgetX, widgetY = widget.screenX, widget.screenY

        local tileData = interactionData.tileData
        local tileX, tileY = 0, 0

        local fakeTiles = fakeTilesHelper.generateFakeTiles(room, tileX, tileY, tileData, "tilesFg", false)
        local fakeTilesSprites = fakeTilesHelper.generateFakeTilesSprites(room, tileX, tileY, fakeTiles, "tilesFg", widgetX, widgetY, {1,1,1,1})

        for _, sprite in ipairs(fakeTilesSprites) do
            sprite.x = (sprite.x - widgetX)*UPSCALE + widgetX
            sprite.y = (sprite.y - widgetY)*UPSCALE + widgetY
            sprite.scaleX = UPSCALE
            sprite.scaleY = UPSCALE
            sprite:draw()
        end

    end
end

local function materialSortFunction(lhs, rhs)
    local lhsText = lhs.text
    local rhsText = rhs.text

    return lhsText < rhsText
end


local function getMaterialList(layer, callback)
    local materials = brushHelper.getMaterialLookup(layer)
    local materialItems = {}

    for id, path in pairs(materials) do
        local materialText = id
        local materialData = path
        
        local item = {
            text = materialText,
            textNoMods = materialText,
            alternativeNames = materialText,
            data = materialData,
            tooltip = nil,
            currentLayer = layer,
            associatedMods = nil
        }

        table.insert(materialItems, item)
    end

    local materialListOptions = {
        searchBarLocation = "none",
        searchBarCallback = function () end,
        -- initialSearch = "",
        initialItem = "Air",
        -- dataToElement = materialDataToElement,
        sortingFunction = materialSortFunction,
        -- searchScore = getMaterialScore,
        -- searchRawItem = true,
        -- searchPreprocessor = prepareMaterialSearch,
        sort = true
    }

    return listWidgets.getList(callback, materialItems, materialListOptions)
end

function TileEditor.getMatrixFromField(fieldData, width, height)
    if not fieldData then return nil end

    local tileMatrix = matrix.filled("0", width, height)
    local data = fieldData:split("\n")
    for ty = 1, height do
        local row = data[ty]
        for tx = 1, width do
            local tile = "0"
            if row then
                tile = row:sub(tx, tx) or "0"
            end

            if not tile or tile == "" or tile == "\r" then tile = "0" end

            tileMatrix:set(tx, ty, tile)
        end
    end
    return tileMatrix
end

function TileEditor.getFieldFromMatrix(matrix)
    if not matrix then return nil end

    local width, height = matrix:size()
    local out = ""
    for ty = 1, height do
        for tx = 1, width do
            out = out .. matrix:get(tx, ty)
        end
        out = out .. "\n"
    end
    return out
end

function TileEditor.getTileEditor(field, options)
    options = options or {}

    local callback = options.callback or function() end

    
    local w
    local h
    if field.metadata.formData then
        w = field.metadata.formData.width/8
        h = field.metadata.formData.height/8
        if w > 32 or h > 32 then UPSCALE = 1
        elseif w > 16 or h > 16 then UPSCALE = 2
        elseif w > 8 or h > 8 then UPSCALE = 4
        elseif w > 4 or h > 4 then UPSCALE = 8
        else UPSCALE = 16 end
    end

    local width, height = w or utils.clamp(options.width or 8, 0, 32), h or utils.clamp(options.height or 8, 0, 32)
    local areaWidth = width*8*UPSCALE
    local areaHeight = height*8*UPSCALE
    local tileData = TileEditor.getMatrixFromField(field:getValue(), width, height) or matrix.filled("0", width, height)

    local tileCanvas = love.graphics.newCanvas(areaWidth, areaHeight)
    local interactionData = {
        tileCanvas = tileCanvas,
        tileData = tileData,
        currentMaterial = "0",
        areaWidth = areaWidth,
        areaHeight = areaHeight,
        --sliderImage = sliderImage,
        --sliderImageData = sliderImageData,
        callback = callback,
    }

    --local sliderImage, sliderImageData = getTilesetScrollables(height*8, width*8)

    local scrolledMaterialList, materialList = getMaterialList("tilesFg", function(list, material) 
        interactionData.currentMaterial = material
     end)

    local areaElement = uiElements.image(tileCanvas):with({
        interactive = 1,
        onDrag = areaInteraction(interactionData),
        onClick = areaInteraction(interactionData)
    }):hook({
        draw = areaDrawing(interactionData)
    })

    local panel = uiElements.panel({areaElement})
    panel.style.padding = 0
    
    local columns = {
        panel,
        uiElements.panel({scrolledMaterialList}):with(uiUtils.fillHeight(false)),
    }


    local pickerRow = uiElements.row(columns)
    return pickerRow
end

return TileEditor