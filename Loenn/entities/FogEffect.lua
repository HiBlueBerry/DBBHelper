local drawableSpriteStruct=require("structs.drawable_sprite")
local xnaColors = require("consts.xna_colors")
local utils = require("utils")
local FogEffect={}
FogEffect.name="DBBHelper/FogEffect"

FogEffect.fieldOrder={
    "x","y",
    "width","height",
    "VelocityX","VelocityY",
    "Amplify","Frequency",
    "NumLevel","LightInfluenceCoefficient",
    "MaskTexture","Label",
    "ScaleX","ScaleY",
    "Color1","Alpha1",
    "Color2","Alpha2",
    "Color3","Alpha3",
    "Color4","Alpha4",
    "Tint","FracMode",
}

Preset={
    "objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/EllipsePassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/GradientPassMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMask",
    "objects/DBB_Items/DBBGeneralEffectTexture/DBBTextMaskOptional",
}

--maximumValue=1.0
FogEffect.fieldInformation={

    VelocityX={
        fieldType="number",
    },
    VelocityY={
        fieldType="number",
    },
    Amplify={
        fieldType="number",
        minimumValue=0.0
    },
    Frequency={
        fieldType="number",
        minimumValue=0.0
    },
    NumLevel={
        fieldType="integer",
        minimumValue=1
    },
    LightInfluenceCoefficient={
        fieldType="number",
        minimumValue=0.0
    },
    Alpha1={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    Alpha2={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    Alpha3={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    Alpha4={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    Color1={
        fieldType="color",
    },
    Color2={
        fieldType="color",
    },
    Color3={
        fieldType="color",
    },
    Color4={
        fieldType="color",
    },
    Tint={
        fieldType="color",
    },
    MaskTexture={
        options=Preset,
        editable=true
    },
    ScaleX={
        fieldType="number"
    },
    ScaleY={
        fieldType="number"
    }
}
FogEffect.placements={
    name="FogEffect",
    data={
        width=8,
        height=8,
        VelocityX=0.1,
        VelocityY=0.0,
        Amplify=0.5,
        Frequency=2.0,
        NumLevel=6,
        LightInfluenceCoefficient=1.0,
        MaskTexture="objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask",
        Label="Default",
        ScaleX=1.0,
        ScaleY=1.0,
        Tint="FFFFFF",
        Color1="1A99B3",
        Alpha1=1.0,
        Color2="000033",
        Alpha2=1.0,
        Color3="B3FFFF",
        Alpha3=1.0,
        Color4="B3B380",
        Alpha4=1.0,
        FracMode=false,
    }
}
FogEffect.depth=-10003
FogEffect.fillColor = function(room, entity)
    local color=utils.getColor("EEEEEE")
    return {color[1] * 0.3, color[2] * 0.3, color[3] * 0.3, 0.6}
end

FogEffect.borderColor = function(room, entity)
    local color=utils.getColor("FFFFFF")
    return {color[1] * 0.8, color[2] * 0.8, color[3] * 0.8, 0.8}
end

return FogEffect