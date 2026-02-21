local drawableSprite=require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local draw=require("utils.drawing")
local drawLine = require("structs.drawable_line")
local AnimatedTextureLight={}
AnimatedTextureLight.name="DBBHelper/AnimatedTextureLight"

Allstyle=
{       "Instant","Linear",
        "easeInSin","easeOutSin","easeInOutSin",
        "easeInCubic","easeOutCubic","easeInOutCubic",
        "easeInQuard","easeOutQuard","easeInOutQuard",
}
RefPath=
{
    "objects/DBB_Items/DBBLightTexture/AnimatedDefaultSprite/default_sprite",
    "objects/DBB_Items/DBBLightTexture/AnimatedWhiteStar/white_star",
}



AnimatedTextureLight.fieldInformation={
    SpritePath={
        options=RefPath,
        editable=true
    },
    Scale={
        fieldType="number",
    },
    Rotation={
        fieldType="number",
    },
    DelayTime={
        fieldType="number",
        minimumValue=0.001,
    },
    TintColor={
        fieldType="color",
    },
    LightAmplify={
        fieldType="number",
        minimumValue=0.0,
    },
    LevelInStyle={
        options=Allstyle,
        editable=false
    },
    LevelOutStyle={
        options=Allstyle,
        editable=false
    }
}

AnimatedTextureLight.depth=-9000
AnimatedTextureLight.fieldOrder={
    "x", "y", 
    "SpritePath","TintColor",
    "ScaleX","ScaleY",
    "Rotation","DelayTime",
    "LightAmplify",
    "LevelInStyle","LevelOutStyle",
    "OnlyEnableOriginalLight",
    "DebugMask","ShowInLoenn"
}
AnimatedTextureLight.placements={
    name="AnimatedTextureLight",
    data={
        --贴图属性相关
        SpritePath="objects/DBB_Items/DBBLightTexture/AnimatedDefaultSprite/default_sprite",
        ScaleX=1.0,
        ScaleY=1.0,
        Rotation=0.0,
        DelayTime=0.1,
        --贴图颜色相关
        TintColor="FFFFFF",
        LightAmplify=1.0,
        LevelInStyle="easeInOutSin",
        LevelOutStyle="easeInOutSin",
        OnlyEnableOriginalLight=false,
        DebugMask=false,
        ShowInLoenn=true,
    },
}
AnimatedTextureLight.justification={0.5,0.5}

AnimatedTextureLight.depth=-10001
function AnimatedTextureLight.sprite(room, entity)
    local sprite_path=entity.SpritePath.."_global"
    if entity.ShowInLoenn==false then
        sprite_path="objects/DBB_Items/DBBLightTexture/AnimatedDefaultSprite/default_sprite_global"
    end
    local sprite1=drawableSprite.fromTexture(sprite_path,entity)
    if sprite1==nil then
        sprite1=drawableSprite.fromTexture("objects/DBB_Items/DBBLightTexture/AnimatedWarning/sprite_not_found_global",entity)
        sprite1.depth=-10001
        sprite1:setColor("FF0000")
        sprite1:setScale(entity.ScaleX,entity.ScaleY)
        
    else
        sprite1.depth=-10001
        sprite1:setColor(entity.TintColor)
        sprite1:setScale(entity.ScaleX,entity.ScaleY)
    end
    sprite1.rotation=-entity.Rotation/180.0*math.pi
    return {sprite1}
end

return AnimatedTextureLight












