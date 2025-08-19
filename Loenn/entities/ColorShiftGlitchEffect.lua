local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local ColorShiftGlitchEffect={}
ColorShiftGlitchEffect.name="DBBHelper/ColorShiftGlitchEffect"

ColorShiftGlitchEffect.fieldOrder={
    "x","y",
    "width","height",
    "SplitAmount","Velocity",
    "Angle","MaskTexture",
    "Label","UVMode",
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--maximumValue=1.0
ColorShiftGlitchEffect.fieldInformation={
    SplitAmount={
        fieldType="number",
        minimumValue=0
    },
    Angle={
        fieldType="number",
    },
    Velocity={
        fieldType="number",
        minimumValue=0,
        maximumValue=50.0
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },
}
ColorShiftGlitchEffect.placements={
    name="ColorShiftGlitchEffect",
    data={
        width=8,
        height=8,
        SplitAmount=0.01,
        Velocity=0.0,
        Angle=0.0,
        UVMode=false,
        Label="Default",
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

ColorShiftGlitchEffect.fillColor = function(room, entity)
    local color=xnaColors.Pink
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

ColorShiftGlitchEffect.borderColor = function(room, entity)
    local color=xnaColors.Violet 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return ColorShiftGlitchEffect