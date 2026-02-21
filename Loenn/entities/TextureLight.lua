local drawableSprite=require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local draw=require("utils.drawing")
local drawLine = require("structs.drawable_line")
local TextureLight={}
TextureLight.name="DBBHelper/TextureLight"

Allstyle=
{       "Instant","Linear",
        "easeInSin","easeOutSin","easeInOutSin",
        "easeInCubic","easeOutCubic","easeInOutCubic",
        "easeInQuard","easeOutQuard","easeInOutQuard",
}
RefPath=
{
    "objects/DBB_Items/DBBLightTexture/default_texture",
    "objects/DBB_Items/DBBLightTexture/sphere_gradient",
    "objects/DBB_Items/DBBLightTexture/rectangle_gradient",
    "objects/DBB_Items/DBBLightTexture/white_circle",
    "objects/DBB_Items/DBBLightTexture/white_square",
    "objects/DBB_Items/DBBLightTexture/white_star",
}



TextureLight.fieldInformation={
    TexturePath={
        options=RefPath,
        editable=true
    },
    Scale={
        fieldType="number",
    },
    Rotation={
        fieldType="number",
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

TextureLight.depth=-9000
TextureLight.fieldOrder={
    "x", "y", 
    "TexturePath","TintColor",
    "ScaleX","ScaleY",
    "Rotation","LightAmplify",
    "LevelInStyle","LevelOutStyle",
    "OnlyEnableOriginalLight","DebugMask","ShowInLoenn"
}
TextureLight.placements={
    name="TextureLight",
    data={
        --贴图属性相关
        TexturePath="objects/DBB_Items/DBBLightTexture/default_texture",
        ScaleX=1.0,
        ScaleY=1.0,
        Rotation=0.0,
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
TextureLight.justification={0.5,0.5}

TextureLight.depth=-10001
function TextureLight.sprite(room, entity)
    local texture_path=entity.TexturePath
    if entity.ShowInLoenn==false then
        texture_path="objects/DBB_Items/DBBLightTexture/default_texture"
    end
    local sprite1=drawableSprite.fromTexture(texture_path,entity)
    if sprite1==nil then
        sprite1=drawableSprite.fromTexture("objects/DBB_Items/DBBHelperLogo/TextureNotFound",entity)
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

return TextureLight












