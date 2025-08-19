local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local TiltShiftBlurEffect={}
TiltShiftBlurEffect.name="DBBHelper/TiltShiftBlurEffect"

TiltShiftBlurEffect.fieldOrder={
    "x","y",
    "width","height",
    "Stride","Spread",
    "Offest","Area",
    "MaskTexture",
    "MaskMode","Reverse",
}
--maximumValue=1.0
TiltShiftBlurEffect.fieldInformation={
    Stride={
        fieldType="number",
        minimumValue=0.0
    },
    Spread={
        fieldType="number",
        minimumValue=0.0
    },
    Offest={
        fieldType="number",
    },
    Area={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },

}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/CircleMask",
}
TiltShiftBlurEffect.placements={
    name="TiltShiftBlurEffect",
    data={
        width=8,
        height=8,
        Stride=0.02,
        Spread=1.0,
        Offest=0.0,
        Area=1.0,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
        MaskMode=false,
        Reverse=false,
    }
}
TiltShiftBlurEffect.fillColor = function(room, entity)
    local color=xnaColors.LightBlue
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

TiltShiftBlurEffect.borderColor = function(room, entity)
    local color=xnaColors.SkyBlue 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return TiltShiftBlurEffect