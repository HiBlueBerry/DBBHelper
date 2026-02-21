local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local PointLightControl={}
PointLightControl.name="DBBHelper/PointLightControl"

PointLightControl.depth=-9000
PointLightControl.fieldOrder={
    "x","y",
    "width","height",

    "AreaControlMode","Label",
    "ColorStart","AlphaStart",
    "ColorEnd","AlphaEnd",
    "ExtinctionStart","ExtinctionEnd",
    "SphereRadiusStart","SphereRadiusEnd",
    "EdgeWidthStart","EdgeWidthEnd",
    "FresnelCoefficientStart","FresnelCoefficientEnd",
    "CameraZStart","CameraZEnd",
    "AspectRatioProportionStart","AspectRatioProportionEnd",
    "ColorControlMode",
    "ExtinctionControlMode","SphereRadiusControlMode",
    "EdgeWidthControlMode","FresnelCoefficientControlMode",
    "CameraZControlMode","AspectRatioProportionControlMode",
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
PointLightControl.fieldInformation={

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
    ExtinctionStart={
        fieldType="number",
        minimumValue=0.01,
    },
    ExtinctionEnd={
        fieldType="number",
        minimumValue=0.01,
    },
    SphereRadiusStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=0.5
    },
    SphereRadiusEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=0.5
    },
    EdgeWidthStart={
        fieldType="number",
        minimumValue=0.0,
    },
    EdgeWidthEnd={
        fieldType="number",
        minimumValue=0.0,
    },
    FresnelCoefficientStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    FresnelCoefficientEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    AspectRatioProportionStart={
        fieldType="number",
        minimumValue=0.01,
    },
    AspectRatioProportionEnd={
        fieldType="number",
        minimumValue=0.01,
    },
    ColorControlMode={
        options=Parameter_Mode,
        editable=false
    },
    ExtinctionControlMode={
        options=Parameter_Mode,
        editable=false
    },
    SphereRadiusControlMode={
        options=Parameter_Mode,
        editable=false
    },
    EdgeWidthControlMode={
        options=Parameter_Mode,
        editable=false
    },
    FresnelCoefficientControlMode={
        options=Parameter_Mode,
        editable=false
    },
    CameraZControlMode={
        options=Parameter_Mode,
        editable=false
    },
    AspectRatioProportionControlMode={
        options=Parameter_Mode,
        editable=false
    },

}
PointLightControl.placements={
    name="PointLightControl",
    data={
        width=8,
        height=8,
        AreaControlMode="Left_to_Right",
        Label="Default",

        ColorStart="FFFFFF",ColorEnd="FFFFFF",AlphaStart=1.0,AlphaEnd=1.0,ColorControlMode="Linear",
    
        ExtinctionStart=10.0,ExtinctionEnd=10.0,ExtinctionControlMode="Linear",
        
        SphereRadiusStart=0.1,SphereRadiusEnd=0.1,SphereRadiusControlMode="Linear",
        
        EdgeWidthStart=5.0,EdgeWidthEnd=5.0,EdgeWidthControlMode="Linear",
        
        FresnelCoefficientStart=1.0,FresnelCoefficientEnd=1.0,FresnelCoefficientControlMode="Linear",
        
        CameraZStart=0.5,CameraZEnd=0.5,CameraZControlMode="Linear",
        
        AspectRatioProportionStart=1.0,AspectRatioProportionEnd=1.0,AspectRatioProportionControlMode="Linear",

    }
}

PointLightControl.fillColor = function(room, entity)
    --淡鹅黄
    local color=utils.getColor("FAFAD2")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

PointLightControl.borderColor = function(room, entity)
    --亮金色
    local color=utils.getColor("FFD700")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return PointLightControl