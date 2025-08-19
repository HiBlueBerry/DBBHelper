local drawableSpriteStruct=require("structs.drawable_sprite")
local AdvancedInvisibleLight={}
AdvancedInvisibleLight.name="DBBHelper/AdvancedInvisibleLight"

AdvancedInvisibleLight.fieldOrder={
    "x","y","lightColor1","lightColor2","bloomRadius","startFade","endFade","speed","acceleration","label","revert"
}

AdvancedInvisibleLight.fieldInformation={

    bloomRadius={
        fieldType="number",
        minimumValue=0.0
    },
    startFade={
        fieldType="integer",
        minimumValue=0
    },
    endFade={
        fieldType="integer",
        minimumValue=0
    },
    lightColor1={
        fieldType="color",
    },
    lightColor2={
        fieldType="color",
    },
    speed={
        fieldType="number",
        minimumValue=0.1,
        maximumValue=50.0
    },
    acceleration={
        fieldType="number",
        minimumValue=0.1,
        maximumValue=50.0
    },
}
AdvancedInvisibleLight.placements={
    name="AdvancedInvisibleLight",
    data={
        bloomRadius=12.0,
        startFade=16,
        endFade=32,
        lightColor1="FFFFFF",
        lightColor2="000000",
        speed=1.0,
        acceleration=1.0,
        label="",
        revert=false
    },
}
function AdvancedInvisibleLight.sprite(room, entity)
    local texture1="objects/DBB_Items/DBBHelperLogo/LightAdvancedLogo"
    local texture2="objects/DBB_Items/DBBHelperLogo/ProfileWhiteLogo"
    local sprite1=drawableSpriteStruct.fromTexture(texture1,entity)
    local sprite2=drawableSpriteStruct.fromTexture(texture2,entity)
    sprite1:setScale(0.33,0.33)
    sprite1:setColor(entity.lightColor1)
    sprite2:setScale(0.45,0.45)
    sprite2:setColor(entity.lightColor2);
    return {sprite1,sprite2}
end

return AdvancedInvisibleLight