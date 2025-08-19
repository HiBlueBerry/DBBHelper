local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local FluidGlitchEffectControl={}
FluidGlitchEffectControl.name="DBBHelper/FluidGlitchEffectControl"

FluidGlitchEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "VelocityStart","VelocityEnd",
    "StrengthStart","StrengthEnd",
    "VelocityControlMode","StrengthControlMode",
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
FluidGlitchEffectControl.fieldInformation={

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
    VelocityControlMode={
        options=Parameter_Mode,
        editable=false
    },
    StrengthControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
FluidGlitchEffectControl.placements={
    name="FluidGlitchEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        VelocityStart=1.0,VelocityEnd=1.0,VelocityControlMode="Linear",
        StrengthStart=0.02,StrengthEnd=0.02,StrengthControlMode="Linear",
    }
}

FluidGlitchEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Indigo
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

FluidGlitchEffectControl.borderColor = function(room, entity)
    local color=xnaColors.MediumPurple
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return FluidGlitchEffectControl