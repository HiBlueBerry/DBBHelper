local drawableSpriteStruct=require("structs.drawable_sprite")
local DarkIn={}
DarkIn.name="DBBHelper/DarkIn"

DarkIn.fieldInformation={
    fadetime={
        fieldType="number",
        minimumValue=1.0
    },
    ascendRate={
        fieldType="number",
        minimumValue=0.5
    },
    descendRate={
        fieldType="number",
        minimumValue=0.5
    }
}
DarkIn.fieldOrder={
    "x", "y", "fadetime","ascendRate","descendRate"
   
}
DarkIn.placements={
    name="DarkIn",
    data={
        fadetime=5.0,
        ascendRate=2.0,
        descendRate=1.0
    }
}
function DarkIn.texture(room, entity)
    local texture="objects/DBB_Items/DBBHelperLogo/DarkInLogo"
    return texture
end
function DarkIn.scale(room, entity)
    return {0.33,0.33}
end
return DarkIn