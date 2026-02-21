local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local TextureLightControl={}
TextureLightControl.name="DBBHelper/TextureLightControl"

TextureLightControl.depth=-9000
TextureLightControl.fieldOrder={
    "x","y",
    "width","height",

    "AreaControlMode","Label",
    "TintColorStart","TintColorEnd",
    "ScaleXStart","ScaleXEnd",
    "ScaleYStart","ScaleYEnd",
    "RotationStart","RotationEnd",

    "TintColorControlMode",
    "ScaleXControlMode",
    "ScaleYControlMode",
    "RotationControlMode",
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
TextureLightControl.fieldInformation={

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
    TintColorControlMode={
        options=Parameter_Mode,
        editable=false
    },
    ScaleXControlMode={
        options=Parameter_Mode,
        editable=false
    },
    ScaleYControlMode={
        options=Parameter_Mode,
        editable=false
    },
    RotationControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
TextureLightControl.placements={
    name="TextureLightControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        
        TintColorStart="FFFFFF",TintColorEnd="FFFFFF",TintColorControlMode="Linear",
        
        ScaleXStart=1.0,ScaleXEnd=1.0,ScaleXControlMode="Linear",
        
        ScaleYStart=1.0,ScaleYEnd=1.0,ScaleYControlMode="Linear",
        
        RotationStart=0.0,RotationEnd=0.0,RotationControlMode="Linear",
    }
}

TextureLightControl.fillColor = function(room, entity)
    --淡鹅黄
    local color=utils.getColor("FAFAD2")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

TextureLightControl.borderColor = function(room, entity)
    --亮金色
    local color=utils.getColor("FFD700")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return TextureLightControl