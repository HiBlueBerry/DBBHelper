local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local AdvancedBlockGlitchEffectControl={}
AdvancedBlockGlitchEffectControl.name="DBBHelper/AdvancedBlockGlitchEffectControl"

AdvancedBlockGlitchEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "VelocityFirstStart","VelocityFirstEnd",
    "VelocitySecondStart","VelocitySecondEnd",
    "SizeFirstXStart","SizeFirstXEnd",
    "SizeFirstYStart","SizeFirstYEnd",
    "SizeSecondXStart","SizeSecondXEnd",
    "SizeSecondYStart","SizeSecondYEnd",
    "StrengthStart","StrengthEnd",
    "VelocityFirstControlMode","VelocitySecondControlMode",
    "SizeFirstXControlMode","SizeFirstYControlMode",
    "SizeSecondXControlMode","SizeSecondYControlMode",
    "StrengthControlMode",
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
AdvancedBlockGlitchEffectControl.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    VelocityFirstStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    VelocityFirstEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },

    VelocitySecondStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    VelocitySecondEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },

    SizeFirstXStart={
        fieldType="number",
        minimumValue=0.0,
    },
    SizeFirstXEnd={
        fieldType="number",
        minimumValue=0.0,
    },

    SizeFirstYStart={
        fieldType="number",
        minimumValue=0.0,
    },
    SizeFirstYEnd={
        fieldType="number",
        minimumValue=0.0,
    },

    SizeSecondXStart={
        fieldType="number",
        minimumValue=0.0,
    },
    SizeSecondXEnd={
        fieldType="number",
        minimumValue=0.0,
    },

    SizeSecondYStart={
        fieldType="number",
        minimumValue=0.0,
    },
    SizeSecondYSEnd={
        fieldType="number",
        minimumValue=0.0,
    },

    StrengthStart={
        fieldType="number",
        minimumValue=0.0,
    },
    StrengthEnd={
        fieldType="number",
        minimumValue=0.0,
    },

    VelocityFirstControlMode={
        options=Parameter_Mode,
        editable=false
    },
    VelocitySecondControlMode={
        options=Parameter_Mode,
        editable=false
    },
    SizeFirstXControlMode={
        options=Parameter_Mode,
        editable=false
    },
    SizeFirstYControlMode={
        options=Parameter_Mode,
        editable=false
    },
    SizeSecondXControlMode={
        options=Parameter_Mode,
        editable=false
    },
    SizeSecondYControlMode={
        options=Parameter_Mode,
        editable=false
    },
    StrengthControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
AdvancedBlockGlitchEffectControl.placements={
    name="AdvancedBlockGlitchEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        VelocityFirstStart=1.0,VelocityFirstEnd=1.0,VelocityFirstControlMode="Linear",
        VelocitySecondStart=1.0,VelocitySecondEnd=1.0,VelocitySecondControlMode="Linear",
        SizeFirstXStart=4.0,SizeFirstXEnd=4.0,SizeFirstXControlMode="Linear",
        SizeFirstYStart=4.0,SizeFirstYEnd=4.0,SizeFirstYControlMode="Linear",
        SizeSecondXStart=4.0,SizeSecondXEnd=4.0,SizeSecondXControlMode="Linear",
        SizeSecondYStart=4.0,SizeSecondYEnd=4.0,SizeSecondYControlMode="Linear",
        StrengthStart=1.0,StrengthEnd=1.0,StrengthControlMode="Linear",
    }
}

AdvancedBlockGlitchEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Indigo
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

AdvancedBlockGlitchEffectControl.borderColor = function(room, entity)
    local color=xnaColors.MediumPurple
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return AdvancedBlockGlitchEffectControl