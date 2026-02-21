local drawableSpriteStruct=require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local SpecialLight_GLBS_Controller={}
SpecialLight_GLBS_Controller.name="DBBHelper/SpecialLight_GLBS_Controller"

SpecialLight_GLBS_Controller.fieldOrder={
    "x","y",
    "width","height",
    "GeneralLightBlendState"
}

GLBS_Mode={
    "AlphaKeep",
    "AlphaWeakened"
}

SpecialLight_GLBS_Controller.fieldInformation={
    GeneralLightBlendState={
        options=GLBS_Mode,
        editable=false
    },
   
}
SpecialLight_GLBS_Controller.placements={
    name="SpecialLight_GLBS_Controller",
    data={
        width=8,
        height=8,
        GeneralLightBlendState="AlphaKeep"
    }
}
SpecialLight_GLBS_Controller.fillColor = function(room, entity)
    local color=utils.getColor("00BFFF")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

SpecialLight_GLBS_Controller.borderColor = function(room, entity)
    local color=utils.getColor("B7A24D")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return SpecialLight_GLBS_Controller