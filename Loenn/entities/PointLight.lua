local drawableSprite=require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local draw=require("utils.drawing")
local drawLine = require("structs.drawable_line")
local PointLight={}
PointLight.name="DBBHelper/PointLight"

PointLight.fieldInformation={
    Extinction={
        fieldType="number",
        minimumValue=0.01,
    },
    SphereRadius={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=0.5
    },
    EdgeWidth={
        fieldType="number",
        minimumValue=0.0,
    },
    FresnelCoefficient={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    Color={
        fieldType="color",
    },
    Alpha={
        minimumValue=0.0,
        maximumValue=1.0
    },
    BrightnessAmplify={
        minimumValue=0.0
    },
    AspectRatioProportion={
        minimumValue=0.01
    },
    
}

PointLight.depth=-9000
PointLight.fieldOrder={
    "x", "y", 
    "Extinction","SphereRadius",
    "EdgeWidth","FresnelCoefficient",
    "Color","Alpha",
    "BrightnessAmplify","AspectRatioProportion",
    "CameraZ","Label",
    "OnlyEnableOriginalLight",
}
PointLight.placements={
    name="PointLight",
    data={
        --光基础强度相关
        Extinction=10.0,
        SphereRadius=0.1,
        EdgeWidth=5.0,
        FresnelCoefficient=1.0,
        --光的颜色相关
        Color="FFFFFF",
        Alpha=1.0,
        --光对原版光照的亮度增幅
        BrightnessAmplify=1.0,
        AspectRatioProportion=1.0,
        CameraZ=0.5,
        Label="Default",
        OnlyEnableOriginalLight=false,
    },
}
PointLight.justification={0.5,0.5}

local function getTexture(profile)
    if profile==true then
        return "objects/DBB_Items/DBBHelperLogo/ProfileWhiteLogo"
    else
        return "objects/DBB_Items/DBBHelperLogo/GodLight2DEmitLogo"
    end
end

PointLight.depth=-10001
function PointLight.sprite(room, entity)
    local texture=getTexture(false)
    local sprite1=drawableSprite.fromTexture(texture,entity)
    sprite1.depth=-10001
    sprite1:setColor(entity.Color)
    sprite1:setScale(0.5,0.5)
    local texture_profile=getTexture(true)
    local sprite2=drawableSprite.fromTexture(texture_profile,entity)
    sprite2.depth=-10001
    sprite2:setColor(entity.Color)
    sprite2:setScale(0.33,0.33)
    return {sprite1,sprite2}
end

return PointLight












