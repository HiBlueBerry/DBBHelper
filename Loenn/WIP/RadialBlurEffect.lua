local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local RadialBlurEffect={}
RadialBlurEffect.name="DBBHelper/RadialBlurEffect"

RadialBlurEffect.fieldOrder={
    "x","y",
    "width","height",
    "CenterX","CenterY",
    "BlurRadius","BlurIter",
    "MaskTexture",
}
--maximumValue=1.0
RadialBlurEffect.fieldInformation={
    CenterX={
        fieldType="number",
    },
    CenterY={
        fieldType="number",
    },
    BlurRadius={
        fieldType="number",
    },
    BlurIter={
        fieldType="integer",
        minimumValue=0
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
RadialBlurEffect.placements={
    name="RadialBlurEffect",
    data={
        width=8,
        height=8,
        CenterX=0.5,
        CenterY=0.5,
        BlurRadius=0.01,
        BlurIter=5,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

RadialBlurEffect.fillColor = function(room, entity)
    local color=xnaColors.LightBlue
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

RadialBlurEffect.borderColor = function(room, entity)
    local color=xnaColors.SkyBlue 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return RadialBlurEffect