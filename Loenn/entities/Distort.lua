local drawableSpriteStruct=require("structs.drawable_sprite")
local Distort={}
Distort.name="DBBHelper/Distort"

Distort.fieldOrder={
    "x","y","path","scale","rotation","waveStrength","distortStrength","DrawMask","DebugMask"
}
Distort.fieldInformation={

    scale={
        minimumValue=0.02
    },
    distortStrength={
        minimumValue=0.0,
        maximumValue=1.0
    },
    waveStrength={
        minimumValue=0.0,
        maximumValue=1.0
    }

}
Distort.placements={
    name="Distort",
    data={
        path="objects/DBB_Items/Distort/sphere_gradient",
        scale=1.0,
        rotation=0.0,
        distortStrength=0.0,
        waveStrength=1.0,
        DrawMask=false,
        DebugMask=false
    }
}

function Distort.sprite(room, entity)
    local texture1=entity.path
    local texture2="objects/DBB_Items/DBBHelperLogo/ProfileWhiteLogo"

    local sprite1=drawableSpriteStruct.fromTexture(texture1,entity)
    local sprite2=drawableSpriteStruct.fromTexture(texture2,entity)

    sprite1.rotation=-entity.rotation/180.0*math.pi
    sprite2.rotation=-entity.rotation/180.0*math.pi

    sprite1:setScale(entity.scale,entity.scale)
    sprite2:setScale(0.05,0.05)
    return {sprite1,sprite2}
end
return Distort