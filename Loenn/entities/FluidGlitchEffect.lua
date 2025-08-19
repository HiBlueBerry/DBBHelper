local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local FluidGlitchEffect={}
FluidGlitchEffect.name="DBBHelper/FluidGlitchEffect"

FluidGlitchEffect.fieldOrder={
    "x","y",
    "width","height",
    "Velocity","Strength",
    "MaskTexture","Label",
    "Vertical","RGBSplit"
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--maximumValue=1.0
FluidGlitchEffect.fieldInformation={
    Velocity={
        fieldType="number",
        minimumValue=0,
        maximumValue=50.0
    },
    Strength={
        fieldType="number",
        minimumValue=0
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },
}
FluidGlitchEffect.placements={
    name="FluidGlitchEffect",
    data={
        width=8,
        height=8,
        Velocity=1.0,
        Strength=0.02,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
        Label="Default",
        Vertical=false,
        RGBSplit=false,
    }
}
FluidGlitchEffect.fillColor = function(room, entity)
    local color=xnaColors.Pink
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

FluidGlitchEffect.borderColor = function(room, entity)
    local color=xnaColors.Violet 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return FluidGlitchEffect