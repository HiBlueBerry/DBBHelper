local drawableSprite=require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local draw=require("utils.drawing")
local drawLine = require("structs.drawable_line")
local GodLight2D={}
GodLight2D.name="DBBHelper/GodLight2D"

GodLight2D.fieldInformation={
    Velocity={
        fieldType="number",
    },
    Scale={
        fieldType="number",
        minimumValue=0.0,
    },
    BaseStrength={
        fieldType="number",
        minimumValue=0.0,
    },
    DynamicStrength={
        fieldType="number",
        minimumValue=0.0,
    },
    LightRadius={
        fieldType="number",
        minimumValue=0.05
    },
    Iter={
        fieldType="integer",
        minimumValue=1
    },
    ConcentrationFactor={
        fieldType="number",
        minimumValue=0.01,
        maximumValue=0.99
    },
    ExtingctionFactor={
        fieldType="number",
        minimumValue=0.01,
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
}

GodLight2D.depth=-9000
GodLight2D.fieldOrder={
    "x", "y", 
    "Velocity","Scale",
    "BaseStrength","DynamicStrength",
    "LightRadius","Iter",
    "ConcentrationFactor","ExtingctionFactor",
    "Color","Alpha",
    "EmitScrollX","EmitScrollY",
    "ProbeScrollX","ProbeScrollY",
    "BrightnessAmplify","Label",
    "OnlyEnableOriginalLight"
}
GodLight2D.placements={
    name="GodLight2D",
    data={
        --光基础强度相关
        Velocity=0.1,
        Scale=2.0,
        BaseStrength=0.5,
        DynamicStrength=0.3,
        LightRadius=0.5,
        --光的迭代次数和光衰减
        Iter=2,
        ConcentrationFactor=0.6,
        ExtingctionFactor=2.0,
        --光的颜色和透明度
        Color="809966",
        Alpha=1.0,
        --光的Scroll效果
        EmitScrollX=0.0,
        EmitScrollY=0.0,
        ProbeScrollX=0.0,
        ProbeScrollY=0.0,
        --光对原版光照的亮度增幅
        BrightnessAmplify=1.0,
        Label="Default",
        OnlyEnableOriginalLight=false,
    },
}
GodLight2D.nodeLimits = {1,1}
GodLight2D.nodeLineRenderType = "line"
GodLight2D.justification={0.5,0.5}

local function getTexture(anchor)
    if anchor then
        return "objects/DBB_Items/DBBHelperLogo/GodLight2DAnchorLogo"
    else
        return "objects/DBB_Items/DBBHelperLogo/GodLight2DEmitLogo"
    end
end

GodLight2D.depth=-10001
function GodLight2D.sprite(room, entity)

    local node = entity.nodes[1]
    local start={entity.x, entity.y}
    local stop={node.x, node.y}
    local control={(start[1]+stop[1])/2,(start[2]+stop[2])/2}

    local texture=getTexture(false)
    local sprite1=drawableSprite.fromTexture(texture,entity)
    sprite1.depth=-10001
    sprite1:setColor(entity.Color)
    sprite1:setScale(0.5,0.5)

    local points=draw.getSimpleCurve(start,stop,control,4)
    local curve=drawLine.fromPoints(points,entity.Color,1)
    curve.depth=-10001
    local texture=getTexture(true)
    local sprite2 = drawableSprite.fromTexture(texture)
    sprite2.depth=-10001
    sprite2:setPosition(node.x, node.y)
    sprite2:setColor(entity.Color)
    sprite2:setScale(0.5,0.5)
    
    return {sprite1,curve,sprite2}
end

function GodLight2D.nodeSprite(room, entity, node, nodeIndex, viewport)
    local texture=getTexture(true)
    local sprite = drawableSprite.fromTexture(texture)
    sprite:setPosition(entity.nodes[nodeIndex].x, entity.nodes[nodeIndex].y)
    sprite:setColor(entity.Color)
    sprite:setScale(0.5,0.5)
    return sprite
end
return GodLight2D












