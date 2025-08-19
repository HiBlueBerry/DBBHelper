local drawableSpriteStruct=require("structs.drawable_sprite")
local LaserGeneratorTranslate={}
LaserGeneratorTranslate.name="DBBHelper/LaserGeneratorTranslate"
Allstyle=
{       "Linear",
        "easeInSin","easeOutSin","easeInOutSin",
        "easeInCubic","easeOutCubic","easeInOutCubic",
        "easeInQuard","easeOutQuard","easeInOutQuard",
}
Alldirection=
{
        "up","down","right","left"
}
LaserGeneratorTranslate.nodeLimits = {1, 1}
LaserGeneratorTranslate.nodeLineRenderType = "line"
LaserGeneratorTranslate.fieldInformation={
    thickness={
        fieldType="number",
        minimumValue=0.01,
        maximumValue=2.5
    },
    speed={
        fieldType="number",
        minimumValue=0.3,
        maximumValue=10.0
    },
    direction={
        options=Alldirection,
        editable=false
    },
    style={
        options=Allstyle,
        editable=false
    },
    acceleration={
        minimumValue=0.1
    }
}
LaserGeneratorTranslate.justification = {0.5, 0.5}
LaserGeneratorTranslate.placements={
    name="LaserGeneratorTranslate",
    data={
        thickness=1.0,
        speed=1.0,
        direction="down",
        style="Linear",
        label="",
        acceleration=1.0,
        revert=false
    },
}

function LaserGeneratorTranslate.sprite(room, entity)
    local texture="objects/DBB_Items/LaserGenerator/laser_generator00"
    local sprite=drawableSpriteStruct.fromTexture(texture,entity)
    if(entity.direction=="right")then
        sprite:setScale(-1.0,1.0)
    elseif(entity.direction=="up") then
        sprite.rotation=math.pi/2.0
    elseif(entity.direction=="down")then
        sprite.rotation=-math.pi/2.0
    end

    return sprite
end

return LaserGeneratorTranslate
