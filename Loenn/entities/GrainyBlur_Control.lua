local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local GrainyBlurEffectControl={}
GrainyBlurEffectControl.name="DBBHelper/GrainyBlurEffectControl"

GrainyBlurEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "BlurRadiusStart","BlurRadiusEnd",
    "BlurRadiusControlMode",
}
Parameter_Mode={
    "Linear",
    "easeInSin","easeOutSin","easeInOutSin",
    "easeInCubic","easeOutCubic","easeInOutCubic",
    "easeInQuard","easeOutQuard","easeInOutQuard",
}
Area_Mode={
    "Left_to_Right","Bottom_to_Top","Instant"
}
GrainyBlurEffectControl.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    BlurRadiusStart={
        fieldType="number",
        minimumValue=0.0,
    },
    BlurRadiusEnd={
        fieldType="number",
        minimumValue=0.0,
    },
    BlurRadiusControlMode={
        options=Parameter_Mode,
        editable=false
    },
}
GrainyBlurEffectControl.placements={
    name="GrainyBlurEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        BlurRadiusStart=0.001,BlurRadiusEnd=0.001,BlurRadiusControlMode="Linear",
    }
}

GrainyBlurEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Navy
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

GrainyBlurEffectControl.borderColor = function(room, entity)
    local color=xnaColors.RoyalBlue
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return GrainyBlurEffectControl