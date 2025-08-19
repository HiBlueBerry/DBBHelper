local drawableSpriteStruct=require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local FogEffectControl={}
FogEffectControl.name="DBBHelper/FogEffectControl"

FogEffectControl.fieldOrder={
    "x","y",
    "width","height",
    "AreaControlMode","Label",
    "VelocityXStart","VelocityXEnd",
    "VelocityYStart","VelocityYEnd",
    "AmplifyStart","AmplifyEnd",
    "FrequencyStart","FrequencyEnd",
    "LICStart","LICEnd",
    "VelocityControlMode","AmplifyControlMode",
    "FrequencyControlMode","LICControlMode"

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

--maximumValue=1.0
FogEffectControl.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    VelocityXStart={
        fieldType="number",
    },
    VelocityXEnd={
        fieldType="number",
    },
    VelocityYStart={
        fieldType="number",
    },
    VelocityYEnd={
        fieldType="number",
    },

    AmplifyStart={
        fieldType="number",
        minimumValue=0.0
    },
    AmplifyEnd={
        fieldType="number",
        minimumValue=0.0
    },
    FrequencyStart={
        fieldType="number",
        minimumValue=0.0
    },
    FrequencyEnd={
        fieldType="number",
        minimumValue=0.0
    },
   
    LICStart={
        fieldType="number",
        minimumValue=0.0
    },
    LICEnd={
        fieldType="number",
        minimumValue=0.0
    },

    VelocityControlMode={
        options=Parameter_Mode,
        editable=false
    },
    AmplifyControlMode={
        options=Parameter_Mode,
        editable=false
    },
    FrequencyControlMode={
        options=Parameter_Mode,
        editable=false
    },
    LICControlMode={
        options=Parameter_Mode,
        editable=false
    },
}
FogEffectControl.placements={
    name="FogEffectControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        VelocityXStart=0.1,
        VelocityXEnd=0.1,
        VelocityYStart=0.0,
        VelocityYEnd=0.0,
        AmplifyStart=0.5,
        AmplifyEnd=0.5,
        FrequencyStart=2.0,
        FrequencyEnd=2.0,
        LICStart=1.0,
        LICEnd=1.0,
        VelocityControlMode="Linear",
        AmplifyControlMode="Linear",
        FrequencyControlMode="Linear",
        LICControlMode="Linear",
    }
}
FogEffectControl.fillColor = function(room, entity)
    local color=utils.getColor("EEEECC")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

FogEffectControl.borderColor = function(room, entity)
    local color=utils.getColor("FFFFDD")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return FogEffectControl