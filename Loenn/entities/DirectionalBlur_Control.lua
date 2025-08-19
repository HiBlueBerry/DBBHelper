local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local DirectionalBlurEffectControl={}
DirectionalBlurEffectControl.name="DBBHelper/DirectionalBlurEffectControl"

DirectionalBlurEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "BlurRadiusStart","BlurRadiusEnd",
    "AngleStart","AngleEnd",
    "BlurRadiusControlMode","AngleControlMode",
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
DirectionalBlurEffectControl.fieldInformation={

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
    AngleStart={
        fieldType="number",
    },
    AngleEnd={
        fieldType="number",
    },
    BlurRadiusControlMode={
        options=Parameter_Mode,
        editable=false
    },
    AngleControlMode={
        options=Parameter_Mode,
        editable=false
    },
}
DirectionalBlurEffectControl.placements={
    name="DirectionalBlurEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        BlurRadiusStart=0.001,BlurRadiusEnd=0.001,BlurRadiusControlMode="Linear",
        AngleStart=0.5,AngleEnd=0.5,AngleControlMode="Linear",
    }
}

DirectionalBlurEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Navy
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

DirectionalBlurEffectControl.borderColor = function(room, entity)
    local color=xnaColors.RoyalBlue
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return DirectionalBlurEffectControl