local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local ColorShiftGlitchEffectControl={}
ColorShiftGlitchEffectControl.name="DBBHelper/ColorShiftGlitchEffectControl"

ColorShiftGlitchEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "SplitAmountStart","SplitAmountEnd",
    "AngleStart","AngleEnd",
    "VelocityStart","VelocityEnd",
    "SplitAmountControlMode","AngleControlMode",
    "VelocityControlMode",
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
ColorShiftGlitchEffectControl.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    SplitAmountStart={
        fieldType="number",
        minimumValue=0.0,
    },
    SplitAmountEnd={
        fieldType="number",
        minimumValue=0.0,
    },
    AngleStart={
        fieldType="number",
    },
    AngleEnd={
        fieldType="number",
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
ColorShiftGlitchEffectControl.placements={
    name="ColorShiftGlitchEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        SplitAmountStart=0.01,SplitAmountEnd=0.01,SplitAmountControlMode="Linear",
        AngleStart=0.0,AngleEnd=0.0,AngleControlMode="Linear",
        VelocityStart=0.0,VelocityEnd=0.0,VelocityControlMode="Linear",
    }
}

ColorShiftGlitchEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Indigo
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

ColorShiftGlitchEffectControl.borderColor = function(room, entity)
    local color=xnaColors.MediumPurple
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return ColorShiftGlitchEffectControl