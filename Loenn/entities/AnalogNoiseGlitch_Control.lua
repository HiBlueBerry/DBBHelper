local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local AnalogNoiseGlitchEffectControl={}
AnalogNoiseGlitchEffectControl.name="DBBHelper/AnalogNoiseGlitchEffectControl"

AnalogNoiseGlitchEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "VelocityStart","VelocityEnd",
    "StrengthStart","StrengthEnd",
    "JitterVelocityStart","JitterVelocityEnd",
    "JitterThresholdStart","JitterThresholdEnd",
    "VelocityControlMode","StrengthControlMode",
    "JitterVelocityControlMode","JitterThresholdControlMode",
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
AnalogNoiseGlitchEffectControl.fieldInformation={

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

    JitterVelocityStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    JitterVelocityEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },

    JitterThresholdStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    JitterThresholdEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },


    VelocityControlMode={
        options=Parameter_Mode,
        editable=false
    },
    StrengthControlMode={
        options=Parameter_Mode,
        editable=false
    },
    JitterVelocityControlMode={
        options=Parameter_Mode,
        editable=false
    },
    JitterThresholdControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
AnalogNoiseGlitchEffectControl.placements={
    name="AnalogNoiseGlitchEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        VelocityStart=1.0,VelocityEnd=1.0,VelocityControlMode="Linear",
        StrengthStart=0.25,StrengthEnd=0.25,StrengthControlMode="Linear",
        JitterVelocityStart=1.0,JitterVelocityEnd=1.0,JitterVelocityControlMode="Linear",
        JitterThresholdStart=0.5,JitterThresholdEnd=0.5,JitterThresholdControlMode="Linear",
        
    }
}

AnalogNoiseGlitchEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Indigo
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

AnalogNoiseGlitchEffectControl.borderColor = function(room, entity)
    local color=xnaColors.MediumPurple
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return AnalogNoiseGlitchEffectControl