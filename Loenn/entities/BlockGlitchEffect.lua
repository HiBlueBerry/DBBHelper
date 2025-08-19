local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local BlockGlitchEffect={}
BlockGlitchEffect.name="DBBHelper/BlockGlitchEffect"

BlockGlitchEffect.fieldOrder={
    "x","y",
    "width","height",
    "Velocity","BlockNum",
    "Strength","MaskTexture",
    "Label","RGBSplit",
    
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--maximumValue=1.0
BlockGlitchEffect.fieldInformation={
    Velocity={
        fieldType="number",
        minimumValue=0,
        maximumValue=50.0
    },
    BlockNum={
        fieldType="number",
        minimumValue=0
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
BlockGlitchEffect.placements={
    name="BlockGlitchEffect",
    data={
        width=8,
        height=8,
        Velocity=1.0,
        BlockNum=4.0,
        Strength=1.0,
        RGBSplit=false,
        Label="Default",
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

BlockGlitchEffect.fillColor = function(room, entity)
    local color=xnaColors.Pink
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

BlockGlitchEffect.borderColor = function(room, entity)
    local color=xnaColors.Violet 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return BlockGlitchEffect