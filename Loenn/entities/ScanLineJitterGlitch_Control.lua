local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local ScanLineJitterGlitchEffectControl={}
ScanLineJitterGlitchEffectControl.name="DBBHelper/ScanLineJitterGlitchEffectControl"

ScanLineJitterGlitchEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "VelocityStart","VelocityEnd",
    "StrengthStart","StrengthEnd",
    "AngleStart","AngleEnd",
    "VelocityControlMode","StrengthControlMode",
    "AngleControlMode",
    
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
ScanLineJitterGlitchEffectControl.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    VelocityStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    VelocityEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    StrengthStart={
        fieldType="number",
        minimumValue=0.0,
    },
    StrengthEnd={
        fieldType="number",
        minimumValue=0.0,
    },
    AngleStart={
        fieldType="number",
    },
    AngleEnd={
        fieldType="number",
    },
    SplitAmountControlMode={
        options=Parameter_Mode,
        editable=false
    },
    AngleControlMode={
        options=Parameter_Mode,
        editable=false
    },
    VelocityControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
ScanLineJitterGlitchEffectControl.placements={
    name="ScanLineJitterGlitchEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",

        VelocityStart=5.0,VelocityEnd=5.0,VelocityControlMode="Linear",
        StrengthStart=0.01,StrengthEnd=0.01,StrengthControlMode="Linear",
        AngleStart=0.0,AngleEnd=0.0,AngleControlMode="Linear",
    }
}

ScanLineJitterGlitchEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Indigo
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

ScanLineJitterGlitchEffectControl.borderColor = function(room, entity)
    local color=xnaColors.MediumPurple
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return ScanLineJitterGlitchEffectControl