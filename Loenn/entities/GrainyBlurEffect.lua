local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local GrainyBlurEffect={}
GrainyBlurEffect.name="DBBHelper/GrainyBlurEffect"

GrainyBlurEffect.fieldOrder={
    "x","y",
    "width","height",
    "BlurRadius","MaskTexture",
    "Label",
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--maximumValue=1.0
GrainyBlurEffect.fieldInformation={
    BlurRadius={
        fieldType="number",
        minimumValue=0
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },
}
GrainyBlurEffect.placements={
    name="GrainyBlurEffect",
    data={
        width=8,
        height=8,
        BlurRadius=0.001,
        Label="Default",
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

GrainyBlurEffect.fillColor = function(room, entity)
    local color=xnaColors.LightBlue
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

GrainyBlurEffect.borderColor = function(room, entity)
    local color=xnaColors.SkyBlue 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return GrainyBlurEffect