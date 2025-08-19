local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local LineGlitchEffectControl={}
LineGlitchEffectControl.name="DBBHelper/LineGlitchEffectControl"

LineGlitchEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "VelocityStart","VelocityEnd",
    "NumLevelStart","NumLevelEnd",
    "DetailStart","DetailEnd",
    "StrengthStart","StrengthEnd",
    "VelocityControlMode","NumLevelControlMode",
    "DetailControlMode","StrengthControlMode",
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
LineGlitchEffectControl.fieldInformation={

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
    NumLevelStart={
        fieldType="number",
        minimumValue=0,
    },
    NumLevelEnd={
        fieldType="number",
        minimumValue=0,
    },
    DetailStart={
        fieldType="number",
        minimumValue=0,
    },
    DetailEnd={
        fieldType="number",
        minimumValue=0,
    },
    StrengthStart={
        fieldType="number",
        minimumValue=0.0,
    },
    StrengthEnd={
        fieldType="number",
        minimumValue=0.0,
    },
    VelocityControlMode={
        options=Parameter_Mode,
        editable=false
    },
    NumLevelControlMode={
        options=Parameter_Mode,
        editable=false
    },
    DetailControlMode={
        options=Parameter_Mode,
        editable=false
    },
    StrengthControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
LineGlitchEffectControl.placements={
    name="LineGlitchEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        VelocityStart=1.0,VelocityEnd=1.0,VelocityControlMode="Linear",
        NumLevelStart=4.0,NumLevelEnd=4.0,NumLevelControlMode="Linear",
        DetailStart=2.0,DetailEnd=2.0,DetailControlMode="Linear",
        StrengthStart=0.01,StrengthEnd=0.01,StrengthControlMode="Linear",
    }
}

LineGlitchEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Indigo
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

LineGlitchEffectControl.borderColor = function(room, entity)
    local color=xnaColors.MediumPurple
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return LineGlitchEffectControl