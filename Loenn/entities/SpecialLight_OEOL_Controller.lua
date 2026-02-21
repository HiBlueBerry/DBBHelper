local drawableSpriteStruct=require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local SpecialLight_OEOL_Controller={}
SpecialLight_OEOL_Controller.name="DBBHelper/SpecialLight_OEOL_Controller"

SpecialLight_OEOL_Controller.fieldOrder={
    "x","y",
    "width","height",
    "OnlyEnableOriginalLight"
}

OEOL_Mode={
    "Special and original light render",
    "Only original light render"
}

SpecialLight_OEOL_Controller.fieldInformation={
    OnlyEnableOriginalLight={
        options=OEOL_Mode,
        editable=false
    },
}
SpecialLight_OEOL_Controller.placements={
    name="SpecialLight_OEOL_Controller",
    data={
        width=8,
        height=8,
        OnlyEnableOriginalLight="Special and original light render"
    }
}
SpecialLight_OEOL_Controller.fillColor = function(room, entity)
    local color=utils.getColor("00BFFF")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

SpecialLight_OEOL_Controller.borderColor = function(room, entity)
    local color=utils.getColor("1CA3C3")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return SpecialLight_OEOL_Controller