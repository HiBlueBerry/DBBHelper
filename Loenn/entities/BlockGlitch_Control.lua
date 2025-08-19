local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local BlockGlitchEffectControl={}
BlockGlitchEffectControl.name="DBBHelper/BlockGlitchEffectControl"

BlockGlitchEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "VelocityStart","VelocityEnd",
    "BlockNumStart","BlockNumEnd",
    "StrengthStart","StrengthEnd",
    "VelocityControlMode","BlockNumControlMode",
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
BlockGlitchEffectControl.fieldInformation={

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

    BlockNumStart={
        fieldType="number",
        minimumValue=0.0,
    },
    BlockNumEnd={
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

    VelocityControlMode={
        options=Parameter_Mode,
        editable=false
    },
    BlockNumControlMode={
        options=Parameter_Mode,
        editable=false
    },
    StrengthControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
BlockGlitchEffectControl.placements={
    name="BlockGlitchEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        VelocityStart=1.0,VelocityEnd=1.0,VelocityControlMode="Linear",
        BlockNumStart=4.0,BlockNumEnd=4.0,BlockNumControlMode="Linear",
        StrengthStart=1.0,StrengthEnd=1.0,StrengthControlMode="Linear",
    }
}

BlockGlitchEffectControl.fillColor = function(room, entity)
    local color=xnaColors.Indigo
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

BlockGlitchEffectControl.borderColor = function(room, entity)
    local color=xnaColors.MediumPurple
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return BlockGlitchEffectControl