local drawableSprite=require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local CloudZipper={}
CloudZipper.name="DBBHelper/CloudZipper"

--云自身的运动不能设置easeBack，不然会有问题
--云移动也不要设置easeBack，不然GP会非常奇怪
Allstyle=
{       "Linear",
        "easeInSin","easeOutSin","easeInOutSin",
        "easeInCubic","easeOutCubic","easeInOutCubic",
        "easeInQuard","easeOutQuard","easeInOutQuard",
}
--这里新建一个域供placements里面的data使用
CloudZipper.fieldInformation={
    gostyle={
        options=Allstyle,
        editable=false
    },
    backstyle={
        options=Allstyle,
        editable=false
    },
    downstyle={
        options=Allstyle,
        editable=false
    },
    upstyle={
        options=Allstyle,
        editable=false
    },
    respawntime={
        fieldType="number",
        minimumValue=0.45
    },
    gotime={
        fieldType="number",
        minimumValue=0.1
    },
    backtime={
        fieldType="number",
        minimumValue=0.1
    },
    CloudUpdateRate={
        fieldType="number",
        minimumValue=0.5,
        maximumValue=2.0
    },
    TrailPointNum={
        fieldType="integer",
        minimumValue=0
    },
    TrailPointBloomRadius={
        fieldType="number",
        minimumValue=0.0
    },
    TrailPointLightStartFade={
        fieldType="integer",
        minimumValue=0
    },
    TrailPointLightEndFade={
        fieldType="integer",
        minimumValue=0
    }
}
CloudZipper.fieldOrder={
    "x", "y", "gostyle","gotime","backstyle","backtime","downstyle","upstyle","CloudUpdateRate","respawntime",
    "TrailPointNum","TrailPointBloomRadius","TrailPointLightStartFade","TrailPointLightEndFade",
    "fragile","small"
}

--respawntime 大于0.45，不然连重生动画都播放不完
--go/backtime 大于0.1，即便如此我仍然会觉得0.5对于云的移动而言已经是一个相当考验反应力和手速的速度了
--CloudUpdateRate 0.5到2.0，对于CloudUpdateRate过大的情况，云本身的弹跳几乎就没有可玩性，你几乎可以把云当成普通的移动平台，因此我限制了最大值
CloudZipper.placements={
    name="CloudZipper",
    data={
        --云的运动参数
        respawntime=2.0,
        gotime=1.0,
        backtime=1.0,
        gostyle="easeInOutSin",
        backstyle="easeInOutSin",
        downstyle="easeOutQuard",
        upstyle="easeInSin",
        CloudUpdateRate=1.0,
        fragile=false,
        small=false,
        --轨迹装饰参数
        TrailPointNum=16,
        TrailPointBloomRadius=12.0,
        TrailPointLightStartFade=12,
        TrailPointLightEndFade=16,
        --弹跳模式
        jumpMode=true
    },
}
CloudZipper.nodeLimits = {1, -1}
CloudZipper.nodeLineRenderType = "line"
CloudZipper.justification={0.5,0.5}

function CloudZipper.texture(room, entity)
    local texture="objects/clouds/cloud00"
    if(entity.fragile==true)then
        texture="objects/clouds/fragile00"
    end
    return texture
end

local function getTexture(entity)
    local fragile = entity.fragile
    if fragile then
        return "objects/clouds/fragile00"
    else
        return "objects/clouds/cloud00"
    end
end

function CloudZipper.sprite(room, entity)

    local texture=getTexture(entity)
    local sprite=drawableSprite.fromTexture(texture,entity)
    local small=entity.small
    if(small==true)then
        sprite:setScale(29/35,1.0)
    end 

    return sprite
end

function CloudZipper.nodeSprite(room, entity, node, nodeIndex, viewport)
    local texture=getTexture(entity)
    local sprite = drawableSprite.fromTexture(texture)
    local small=entity.small
    if(small==true)then
        sprite:setScale(29/35,1.0)
    end 
    sprite:setPosition(entity.nodes[nodeIndex].x, entity.nodes[nodeIndex].y)
    return sprite
end

return CloudZipper












