local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local GodLight2DControl={}
GodLight2DControl.name="DBBHelper/GodLight2DControl"

GodLight2DControl.depth=-9000
GodLight2DControl.fieldOrder={
    "x","y",
    "width","height",

    "AreaControlMode","Label",
    "ColorStart","AlphaStart",
    "ColorEnd","AlphaEnd",
    "BaseStrengthStart","BaseStrengthEnd",
    "ConcentrationFactorStart","ConcentrationFactorEnd",
    "ExtingctionFactorStart","ExtingctionFactorEnd",
    "ColorControlMode",
    "BaseStrengthControlMode",
    "ConcentrationFactorControlMode",
    "ExtingctionFactorControlMode",
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
GodLight2DControl.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    ColorStart={
        fieldType="color",
    },
    ColorEnd={
        fieldType="color",
    },
    AlphaStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    AlphaEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    BaseStrengthStart={
        fieldType="number",
        minimumValue=0.0,
    },
    BaseStrengthEnd={
        fieldType="number",
        minimumValue=0.0,
    },
    ConcentrationFactorStart={
        fieldType="number",
        minimumValue=0.01,
        maximumValue=0.99
    },
    ConcentrationFactorEnd={
        fieldType="number",
        minimumValue=0.01,
        maximumValue=0.99
    },
    ExtingctionFactorStart={
        fieldType="number",
        minimumValue=0.01,
    },
    ExtingctionFactorEnd={
        fieldType="number",
        minimumValue=0.01,
    },
    ColorControlMode={
        options=Parameter_Mode,
        editable=false
    },
    BaseStrengthControlMode={
        options=Parameter_Mode,
        editable=false
    },
    ConcentrationFactorControlMode={
        options=Parameter_Mode,
        editable=false
    },
    ExtingctionFactorControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
GodLight2DControl.placements={
    name="GodLight2DControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",
        
        ColorStart="FFFFFF",ColorEnd="FFFFFF",AlphaStart=1.0,AlphaEnd=1.0,ColorControlMode="Linear",
        
        BaseStrengthStart=0.5,BaseStrengthEnd=0.5,BaseStrengthControlMode="Linear",
        
        ConcentrationFactorStart=0.6,ConcentrationFactorEnd=0.6,ConcentrationFactorControlMode="Linear",
        
        ExtingctionFactorStart=2.0,ExtingctionFactorEnd=2.0,ExtingctionFactorControlMode="Linear",
        
    }
}

GodLight2DControl.fillColor = function(room, entity)
    --淡鹅黄
    local color=utils.getColor("FAFAD2")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

GodLight2DControl.borderColor = function(room, entity)
    --亮金色
    local color=utils.getColor("FFD700")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return GodLight2DControl