local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local AdvancedBlockGlitchEffect={}
AdvancedBlockGlitchEffect.name="DBBHelper/AdvancedBlockGlitchEffect"

AdvancedBlockGlitchEffect.fieldOrder={
    "x","y",
    "width","height",
    "VelocityFirst","VelocitySecond",
    "SizeFirstX","SizeFirstY",
    "SizeSecondX","SizeSecondY",
    "Strength","MaskTexture",
    "Label","RGBSplit"
}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--maximumValue=1.0
AdvancedBlockGlitchEffect.fieldInformation={

    VelocityFirst={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    VelocitySecond={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=50.0
    },
    SizeFirstX={
        fieldType="number",
        minimumValue=0.0,
    },
    SizeFirstY={
        fieldType="number",
        minimumValue=0.0,
    },
    SizeSecondX={
        fieldType="number",
        minimumValue=0.0,
    },
    SizeSecondY={
        fieldType="number",
        minimumValue=0.0,
    },
    Strength={
        fieldType="number",
        minimumValue=0.0,
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },

}
AdvancedBlockGlitchEffect.placements={
    name="AdvancedBlockGlitchEffect",
    data={
        width=8,
        height=8,
        VelocityFirst=1.0,
        VelocitySecond=1.0,
        SizeFirstX=4.0,
        SizeFirstY=4.0,
        SizeSecondX=4.0,
        SizeSecondY=4.0,
        Strength=1.0,
        Label="Default",
        RGBSplit=false,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

AdvancedBlockGlitchEffect.fillColor = function(room, entity)
    local color=xnaColors.Pink
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

AdvancedBlockGlitchEffect.borderColor = function(room, entity)
    local color=xnaColors.Violet
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return AdvancedBlockGlitchEffect