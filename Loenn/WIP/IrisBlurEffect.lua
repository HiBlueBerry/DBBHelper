local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local IrisBlurEffect={}
IrisBlurEffect.name="DBBHelper/IrisBlurEffect"

IrisBlurEffect.fieldOrder={
    "x","y",
    "width","height",
    "Stride","Offset",
    "Area","Spread",
    "MaskTexture","MaskMode"
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/CircleMask",
}
--maximumValue=1.0
IrisBlurEffect.fieldInformation={
    Stride={
        fieldType="number",
        minimumValue=0
    },
    Offset={
        fieldType="number",
    },
    Area={
        fieldType="number",
        minimumValue=0,
        maximumValue=1.0
    },
    Spread={
        fieldType="number",
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },
}
IrisBlurEffect.placements={
    name="IrisBlurEffect",
    data={
        width=8,
        height=8,
        Stride=0.02,
        Offset=0.0,
        Area=1.0,
        Spread=1.0,
        MaskMode=false,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

IrisBlurEffect.fillColor = function(room, entity)
    local color=xnaColors.LightBlue
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

IrisBlurEffect.borderColor = function(room, entity)
    local color=xnaColors.SkyBlue 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return IrisBlurEffect