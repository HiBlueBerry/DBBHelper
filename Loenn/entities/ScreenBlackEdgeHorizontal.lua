local drawableSpriteStruct=require("structs.drawable_sprite")
local ScreenBlackEdgeHorizontal={}
ScreenBlackEdgeHorizontal.name="DBBHelper/ScreenBlackEdgeHorizontal"
Allstyle=
{   
    "Linear",
    "easeInSin","easeOutSin","easeInOutSin",
    "easeInCubic","easeOutCubic","easeInOutCubic",
    "easeInQuard","easeOutQuard","easeInOutQuard",
    "easeInBack","easeOutBack","easeInOutBack"
}
ScreenBlackEdgeHorizontal.fieldInformation={
    Proportion={
        fieldType="number",
        minimumValue=1.8
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
ScreenBlackEdgeHorizontal.fieldOrder={
    "x","y","InStyle","OutStyle","Proportion","Color"
}
ScreenBlackEdgeHorizontal.placements={
    name="ScreenBlackEdgeHorizontal",
    data={    
        Proportion=2.35,
        Color="000000",
        InStyle="easeInOutSin",
        OutStyle="easeInOutSin",
    }
}
ScreenBlackEdgeHorizontal.depth=-10002
function ScreenBlackEdgeHorizontal.sprite(room, entity)
    local texture_inner="objects/DBB_Items/DBBHelperLogo/ScreenBlackEdgeHorizontalLogoInner"
    local texture_profile="objects/DBB_Items/DBBHelperLogo/ScreenBlackEdgeHorizontalLogoProfile"
    local sprite_inner=drawableSpriteStruct.fromTexture(texture_inner,entity)
    local sprite_profile=drawableSpriteStruct.fromTexture(texture_profile,entity)
    sprite_inner:setScale(0.33,0.33)
    sprite_inner.depth=-10002
    sprite_profile:setScale(0.33,0.33)
    sprite_profile:setColor(entity.Color)
    sprite_profile.depth=-10002
    return {sprite_inner,sprite_profile}
end
return ScreenBlackEdgeHorizontal