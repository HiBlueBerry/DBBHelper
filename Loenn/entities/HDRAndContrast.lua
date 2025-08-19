local drawableSpriteStruct=require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local HDRAndContrast={}
HDRAndContrast.name="DBBHelper/HDRAndContrast"

HDRAndContrast.fieldOrder={
    "x","y",
    "width","height",
    "ExposureStart","ExposureEnd",
    "GammaStart","GammaEnd",
    "ContrastStart","ContrastEnd",
    "AreaControlMode","ExposureControlMode",
    "GammaControlMode","ContrastControlMode",
    

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
HDRAndContrast.fieldInformation={

    AreaControlMode={
        options=Area_Mode,
        editable=false
    },
    ExposureStart={
        fieldType="number",
        minimumValue=-3.5,
        maximumValue=3.5,
    },
    ExposureEnd={
        fieldType="number",
        minimumValue=-3.5,
        maximumValue=3.5,
    },
    GammaStart={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=3.5,
    },
    GammaEnd={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=3.5,
    },
    ContrastStart={
        fieldType="number",
        minimumValue=-3.5,
        maximumValue=3.5,
    },

    ContrastEnd={
        fieldType="number",
        minimumValue=-3.5,
        maximumValue=3.5,
    },

    ExposureControlMode={
        options=Parameter_Mode,
        editable=false
    },
    GammaControlMode={
        options=Parameter_Mode,
        editable=false
    },
    ContrastControlMode={
        options=Parameter_Mode,
        editable=false
    },
}
HDRAndContrast.placements={
    name="HDRAndContrast",
    data={
        width=8,
        height=8,
        ExposureStart=1.0,
        ExposureEnd=1.0,
        GammaStart=1.0,
        GammaEnd=1.0,
        ContrastStart=1.0,
        ContrastEnd=1.0,
        AreaControlMode="Left_to_Right",
        ExposureControlMode="Linear",
        GammaControlMode="Linear",
        ContrastControlMode="Linear",
    }
}
HDRAndContrast.fillColor = function(room, entity)
    local color=utils.getColor("CCEEEE")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

HDRAndContrast.borderColor = function(room, entity)
    local color=utils.getColor("DDFFFF")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return HDRAndContrast