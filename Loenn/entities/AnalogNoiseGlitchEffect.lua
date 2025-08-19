local drawableSpriteStruct=require("structs.drawable_sprite")
local utils=require("utils")
local xnaColors = require("consts.xna_colors")
local AnalogNoiseGlitchEffect={}
AnalogNoiseGlitchEffect.name="DBBHelper/AnalogNoiseGlitchEffect"

AnalogNoiseGlitchEffect.fieldOrder={
    "x","y",
    "width","height",
    "Velocity","Strength",
    "JitterVelocity","JitterThreshold",
    "MaskTexture","Label",
    "GreyJitter",

}
Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}
--velocity maximumValue=50.0
--One funny thing: Running the game for a long time will lead to float number overflow.
AnalogNoiseGlitchEffect.fieldInformation={
    Velocity={
        fieldType="number",
        minimumValue=0,
        maximumValue=50.0
    },
    Strength={
        fieldType="number",
        minimumValue=0
    },
    JitterVelocity={
        fieldType="number",
        minimumValue=0,
        maximumValue=50.0
    },
    JitterThreshold={
        fieldType="number",
        minimumValue=0,
        maximumValue=1.0
    },
    MaskTexture={
        options=Preset,
        editable=true,
    },
}
AnalogNoiseGlitchEffect.placements={
    name="AnalogNoiseGlitchEffect",
    data={
        width=8,
        height=8,
        Velocity=1.0,
        Strength=0.25,
        JitterVelocity=1.0,
        JitterThreshold=0.5,
        GreyJitter=false,
        Label="Default",
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    }
}

AnalogNoiseGlitchEffect.fillColor = function(room, entity)
    local color=xnaColors.Pink
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

AnalogNoiseGlitchEffect.borderColor = function(room, entity)
    local color=xnaColors.Violet 
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end
return AnalogNoiseGlitchEffect