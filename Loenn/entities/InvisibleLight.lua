local drawableSpriteStruct=require("structs.drawable_sprite")
local InvisibleLight={}
InvisibleLight.name="DBBHelper/InvisibleLight"

InvisibleLight.fieldOrder={
    "x","y","lightColor","bloomRadius","startFade","endFade"
}

InvisibleLight.fieldInformation={

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
    lightColor={
        fieldType="color",
    }
}
InvisibleLight.placements={
    name="InvisibleLight",
    data={
        bloomRadius=12.0,
        startFade=16,
        endFade=32,
        lightColor="FFFFFF"
    },
}
function InvisibleLight.sprite(room, entity)
    local texture="objects/DBB_Items/DBBHelperLogo/LightLogo"
    local sprite=drawableSpriteStruct.fromTexture(texture,entity)
    sprite:setScale(0.33,0.33)
    sprite:setColor(entity.lightColor)
    return sprite
end

return InvisibleLight