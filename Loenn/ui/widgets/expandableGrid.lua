local ui = require("ui")
local uiElements = require("ui.elements")
local uiUtils = require("ui.utils")
local utils = require("utils")
local drawing = require("utils.drawing")
local logging = require("logging")
local expGridElement = {}

-- For styling
uiElements.add("expandableGrid", {
    base = "group",

    style = {
        padding = 8,
        spacing = 8,
        rowSpacing = nil,
        columnSpacing = nil
    }
})

local function columnWidthHook(grids, columnIndex)
    return function(orig, self)
        local width = 0

        for _, grid in ipairs(grids) do
            local column = grid._columns[columnIndex]

            for _, child in ipairs(column.children) do
                child:layoutLazy()

                width = math.max(width, child.width)
            end
        end

        return width
    end
end
local function makeRow(columns,outerPadding,columnSpacing,rows,columnCount,columnElements,rowSpacing)
    local row = uiElements.row(columns):with({
        style = {
            padding = outerPadding,
            spacing = columnSpacing
        }
    })

    row:hook({
        layout = function(orig, self)
            orig(self)

            -- Adjust element Y positions to become more "grid like"
            local offsetY = 0
            local lowestElement

            for y = 1, rows + 1 do
                local rowHeight = 0

                for x = 1, columnCount do
                    local element = columnElements[x][y]

                    if element then
                        rowHeight = math.max(rowHeight, element.height)
                    end
                end

                for x = 1, columnCount do
                    local element = columnElements[x][y]

                    if element then
                        local centerVertically = rawget(element, "centerVertically")

                        if centerVertically then
                            element.__gridY = offsetY
                            element.__gridY += math.floor((rowHeight - element.height) / 2)

                        else
                            element.__gridY = offsetY
                        end
                    end
                end

                if rowHeight > 0 then
                    offsetY += rowHeight + rowSpacing
                end
            end

            self.height = offsetY + outerPadding * 2 - rowSpacing
            self.innerHeight = offsetY - rowSpacing
        end
    })

    row._columns = columns
    return row
end
function expGridElement.getGrid(elements, columnCount, buttonOptions, createElement, removeElement)
    local columns = {}
    local columnElements = {}
    local gridStyle = uiElements.grid.__default.style or {}

    local outerPadding = gridStyle.outerPadding or 8
    local columnSpacing = gridStyle.columnSpacing or gridStyle.spacing or 8
    local rowSpacing = gridStyle.rowSpacing or gridStyle.spacing or 8

    local column = 1
    local rows = 1

    for i = 1, columnCount do
        columnElements[i] = {}
    end

    for i, element in ipairs(elements) do
        local targetColumn = columnElements[column]

        -- Add blank elements in empty spaces
        if not element then
            table.insert(targetColumn, uiElements.new({}))

        else
            table.insert(targetColumn, element)

            element:hook({
                layoutLateLazy = function(orig, self)
                    orig(self)

                    if self.__gridY then
                        self.y = self.__gridY

                        self.parent:layout()
                    end
                end
            })
        end
        column += 1

        if column > columnCount then
            column = 1
            rows += 1
        end
    end
    local plus = uiElements.button("+",function (button) end):with(buttonOptions)
    local delete = uiElements.button("-",function (button) end):with(buttonOptions)
    local both = uiElements.row({plus,delete})
    table.insert(columnElements[1],both)
    plus:hook({
        layoutLateLazy = function(orig, self)
            orig(self)

            if self.__gridY then
                self.y = self.__gridY

                self.parent:layout()
            end
        end
    })

    delete:hook({
        layoutLateLazy = function(orig, self)
            orig(self)

            if self.__gridY then
                self.y = self.__gridY

                self.parent:layout()
            end
        end
    })
    both:hook({
        layoutLateLazy = function(orig, self)
            orig(self)

            if self.__gridY then
                self.y = self.__gridY

                self.parent:layout()
            end
        end
    })
    rows+=1
    for i = 1, columnCount do
        -- Spacing needed for dropdown positioning
        columns[i] = uiElements.group(columnElements[i]):with({
            style = {
                spacing = 0
            }
        })
    end

    local row = makeRow(columns,outerPadding,columnSpacing,rows,columnCount,columnElements,rowSpacing)
    local wrap = uiElements.column({})
    wrap:addChild(row)
    plus:with({onClick = function(self, x, y, button)
        if button ~= 1 then
            return
        end
        row:removeSelf()
        local moveButton = column==1
        local element = utils.callIfFunction(createElement) or uiElements.new({})
        local targetColumn = columnElements[column]
        local b
        if moveButton then
            b=table.remove(targetColumn)
        end
        table.insert(targetColumn,element)
        element:hook({
            layoutLateLazy = function(orig, self)
                orig(self)

                if self.__gridY then
                    self.y = self.__gridY

                    self.parent:layout()
                end
            end
        })
        if moveButton then
            table.insert(targetColumn,b)
        end
        columns[column] = uiElements.group(targetColumn):with({
            style = {
                spacing = 0
            }})
        column+=1
        if column > columnCount then
            column = 1
            rows += 1
        end
        
        wrap:addChild(row)
        self:reflow()
    end})
    delete:with({
        onClick = function (self, x, y, button)
            if button ~= 1 then
                return
            end
            
            column -= 1
            if column <= 0 and rows <= 2 then
                column += 1
                return
            end
            row:removeSelf()
            if column == 0 then
                rows-=1
                column = columnCount
            end
            local targetColumn = columnElements[column]
            local moveButton = column==1
            local b
            if moveButton then
                b = table.remove(targetColumn)
            end
            removeElement((rows-1)*columnCount + column)
            table.remove(targetColumn)
            if moveButton then
                table.insert(targetColumn,b)
            end
            columns[column] = uiElements.group(targetColumn):with({
                style = {
                    spacing = 0
                }})
            wrap:addChild(row)
            self:reflow()
        end
    })
    return wrap
end

function expGridElement.alignColumns(grids)
    -- Don't need to do anything
    if not grids or #grids < 2 then
        return
    end

    local columnCount = #grids[1]._columns

    for _, grid in ipairs(grids) do
        if #grid._columns ~= columnCount then
            return false, "Column counts mismatching"
        end
    end

    for _, grid in ipairs(grids) do
        for columnIndex, column in ipairs(grid._columns) do
            column:hook({
                calcWidth = columnWidthHook(grids, columnIndex)
            })
        end
    end
end

return expGridElement