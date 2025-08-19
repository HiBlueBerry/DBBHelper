local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local BokehBlurEffect={}
BokehBlurEffect.name="DBBHelper/BokehBlurEffect"

BokehBlurEffect.fieldOrder={
    "x","y",
    "width","height",
    "InnerRadius","Interval",
    "Iter","MaskTexture"
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/CircleMask",
}
--maximumValue=1.0
BokehBlurEffect.fieldInformation={
    InnerRadius={
        fieldType="number",
        minimumValue=0
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
BokehBlurEffect.placements={
    name="BokehBlurEffect",
    data={
        width=8,
        height=8,
        InnerRadius=0.0,
        Interval=0.01,
        Iter=8,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

BokehBlurEffect.fillColor = function(room, entity)
    local color=xnaColors.LightBlue
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

BokehBlurEffect.borderColor = function(room, entity)
    local color=xnaColors.SkyBlue 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return BokehBlurEffect