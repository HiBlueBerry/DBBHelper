local drawableSpriteStruct=require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local TintAndSaturation={}
TintAndSaturation.name="DBBHelper/TintAndSaturation"

TintAndSaturation.fieldOrder={
    "x","y",
    "width","height",
    "TintColorStart","TintColorEnd",
    "TintStrengthStart","TintStrengthEnd",
    "SaturationStart","SaturationEnd",
    "AreaControlMode","TintColorControlMode",
    "TintStrengthControlMode","SaturationControlMode",
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
TintAndSaturation.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    TintColorStart={
        fieldType="color",
    },
    TintColorEnd={
        fieldType="color",
    },
    TintStrengthStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0,
    },
    TintStrengthEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0,
    },
    SaturationStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=2.0,
    },
    SaturationEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=2.0,
    },
    TintColorControlMode={
        options=Parameter_Mode,
        editable=false
    },
    TintStrengthControlMode={
        options=Parameter_Mode,
        editable=false
    },
    SaturationControlMode={
        options=Parameter_Mode,
        editable=false
    },
}
TintAndSaturation.placements={
    name="TintAndSaturation",
    data={
        width=8,
        height=8,
        --色调
        TintColorStart="FFFFFF",
        TintColorEnd="FFFFFF",
        --色调强度
        TintStrengthStart=0.0,
        TintStrengthEnd=0.0,
        --饱和度
        SaturationStart=1.0,
        SaturationEnd=1.0,
        --控制特效区域的方式
        AreaControlMode="Left_to_Right",
        TintColorControlMode="Linear",
        TintStrengthControlMode="Linear",
        SaturationControlMode="Linear",
    }
}
TintAndSaturation.fillColor = function(room, entity)
    local color=utils.getColor("BBFFEE")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

TintAndSaturation.borderColor = function(room, entity)
    local color=utils.getColor("FFEEFF")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return TintAndSaturation