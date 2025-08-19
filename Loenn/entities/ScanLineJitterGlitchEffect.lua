local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local ScanLineJitterGlitchEffect={}
ScanLineJitterGlitchEffect.name="DBBHelper/ScanLineJitterGlitchEffect"

ScanLineJitterGlitchEffect.fieldOrder={
    "x","y",
    "width","height",
    "Velocity", "Strength",
    "Angle","MaskTexture",
    "Label","VelocityContinuization",
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--maximumValue=1.0
ScanLineJitterGlitchEffect.fieldInformation={
    Strength={
        fieldType="number",
        minimumValue=0.0,
    },
    Velocity={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },
}
ScanLineJitterGlitchEffect.placements={
    name="ScanLineJitterGlitchEffect",
    data={
        width=8,
        height=8,
        Velocity=5.0,
        Strength=0.01,
        Angle=0.0,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
        Label="Default",
        VelocityContinuization=false,
    }
}

ScanLineJitterGlitchEffect.fillColor = function(room, entity)
    local color=xnaColors.Pink
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

ScanLineJitterGlitchEffect.borderColor = function(room, entity)
    local color=xnaColors.Violet
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return ScanLineJitterGlitchEffect