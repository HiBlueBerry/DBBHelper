local drawableSpriteStruct=require("structs.drawable_sprite")
local ScreenBlackEdgeVertical={}
ScreenBlackEdgeVertical.name="DBBHelper/ScreenBlackEdgeVertical"
Allstyle=
{   
    "Linear",
    "easeInSin","easeOutSin","easeInOutSin",
    "easeInCubic","easeOutCubic","easeInOutCubic",
    "easeInQuard","easeOutQuard","easeInOutQuard",
    "easeInBack","easeOutBack","easeInOutBack"
}
ScreenBlackEdgeVertical.fieldInformation={
    Proportion={
        fieldType="number",
        maximumValue=1.76,
    },
    Color={
        fieldType="color",
    },
    InStyle={
        options=Allstyle,
        editable=false
    },
    OutStyle={
        options=Allstyle,
        editable=false
    }
}
ScreenBlackEdgeVertical.fieldOrder={
    "x","y","InStyle","OutStyle","Proportion","Color"
}
ScreenBlackEdgeVertical.placements={
    name="ScreenBlackEdgeVertical",
    data={    
        Proportion=1.33,
        Color="000000",
        InStyle="easeInOutSin",
        OutStyle="easeInOutSin",
    }
}
ScreenBlackEdgeVertical.depth=-10002
function ScreenBlackEdgeVertical.sprite(room, entity)
    local texture_inner="objects/DBB_Items/DBBHelperLogo/ScreenBlackEdgeVerticalLogoInner"
    local texture_profile="objects/DBB_Items/DBBHelperLogo/ScreenBlackEdgeVerticalLogoProfile"
    local sprite_inner=drawableSpriteStruct.fromTexture(texture_inner,entity)
    local sprite_profile=drawableSpriteStruct.fromTexture(texture_profile,entity)
    sprite_inner:setScale(0.33,0.33)
    sprite_inner.depth=-10002
    sprite_profile:setScale(0.33,0.33)
    sprite_profile:setColor(entity.Color)
    sprite_inner.depth=-10002
    return {sprite_inner,sprite_profile}
end
return ScreenBlackEdgeVertical