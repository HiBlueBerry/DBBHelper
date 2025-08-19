local drawableSpriteStruct=require("structs.drawable_sprite")
local FanBumper={}
FanBumper.name="DBBHelper/FanBumper"
FanBumper.fieldOrder={
    "x","y","radius","interval","startTheta","deltaTheta","rotateSpeed"
}
FanBumper.fieldInformation={
    deltaTheta={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=2.0
    },
}
FanBumper.placements={
    name="FanBumper",
    data={    
        radius=24.0,
        interval=12.0,
        startTheta=0.0,
        deltaTheta=0.5,
        rotateSpeed=0.0
    }

}
function FanBumper.sprite(room, entity)
    local texture="objects/DBB_Items/SpecialBumper/BumperLogo"
    local sprite=drawableSpriteStruct.fromTexture(texture,entity)
    return sprite
end
return FanBumper