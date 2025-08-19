local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local DirectionalBlurEffect={}
DirectionalBlurEffect.name="DBBHelper/DirectionalBlurEffect"

DirectionalBlurEffect.fieldOrder={
    "x","y",
    "width","height",
    "BlurRadius","Angle",
    "MaskTexture","Label",
    "Iter",
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--maximumValue=1.0
DirectionalBlurEffect.fieldInformation={
    BlurRadius={
        fieldType="number",
        minimumValue=0
    },
    Angle={
        fieldType="number",
    },
    Iter={
        fieldType="integer",
        minimumValue=0
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },
}
DirectionalBlurEffect.placements={
    name="DirectionalBlurEffect",
    data={
        width=8,
        height=8,
        BlurRadius=0.02,
        Angle=0.5,
        Iter=5,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
        Label="Default",
    }
}

DirectionalBlurEffect.fillColor = function(room, entity)
    local color=xnaColors.LightBlue
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

DirectionalBlurEffect.borderColor = function(room, entity)
    local color=xnaColors.SkyBlue 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return DirectionalBlurEffect