local drawableSpriteStruct=require("structs.drawable_sprite")
local AlignedText={}
AlignedText.name="DBBHelper/AlignedText"
--文字对齐模式
AlignedStyle=
{       
    "Left","Middle","Right"
}
--文字消失的方式
AllFadeStyle=
{       "Linear",
        "easeInSin","easeOutSin","easeInOutSin",
        "easeInCubic","easeOutCubic","easeInOutCubic",
        "easeInQuard","easeOutQuard","easeInOutQuard",
}
AlignedText.fieldInformation={
    StrokeWidth={
        fieldType="number",
        minimumValue=0.0,
    },
    TextColor={
        fieldType="color"
    },
    TextAlpha={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    StrokeColor={
        fieldType="color"
    },
    StrokeAlpha={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    TextLine={
        fieldType="integer",
        minimumValue=0,
    },
    TextMaxNum={
        fieldType="integer",
        minimumValue=0,
    },
    PaddingProportion={
        fieldType="number",
    },
    ParallaxProportion={
        fieldType="number",
    },
    AlignedStyle={
        fieldType="string",
        options=AlignedStyle,
        editable=false
    },
    FadeStartDistance={
        fieldType="number",
        minimumValue=0.0,
    },
    FadeEndDistance={
        fieldType="number",
        minimumValue=0.0,
    },
    FadeStyle={
        fieldType="string",
        options=AllFadeStyle,
        editable=false,
    }
}
AlignedText.fieldOrder={
    "x", "y", 
    "TextID","AlignedStyle",
    "TextScale","StrokeWidth",
    "TextColor","TextAlpha",
    "StrokeColor","StrokeAlpha",
    "TextLine","TextMaxNum",
    "PaddingProportion","ParallaxProportion",
    "FadeStartDistance","FadeEndDistance",
    "FadeStyle","FadeSwitch",
    "EllipsisMode","DebugMode"
   
}
AlignedText.placements={
    name="AlignedText",
    data={
        TextID="DBBHelper_AlignedText_Default",
        AlignedStyle="Left",
        TextScale=1.0,
        StrokeWidth=2.0,
        TextColor="FFFFFF",
        TextAlpha=1.0,
        StrokeColor="000000",
        StrokeAlpha=1.0,
        TextLine=0,
        TextMaxNum=30,
        PaddingProportion=1.0,
        ParallaxProportion=0.0,
        FadeStartDistance=0.0,
        FadeEndDistance=128.0,
        FadeStyle="Linear",
        FadeSwitch=false,
        EllipsisMode=false,
        DebugMode=true
    }
}
AlignedText.depth=-10005
function AlignedText.sprite(room, entity)
    local texture="objects/DBB_Items/DBBHelperLogo/LeftAlignedText"
    local dot="objects/DBB_Items/DBBHelperLogo/Dot"
    if entity.AlignedStyle=="Middle" then
        texture="objects/DBB_Items/DBBHelperLogo/MiddleAlignedText"
    elseif entity.AlignedStyle=="Right" then
        texture="objects/DBB_Items/DBBHelperLogo/RightAlignedText"
    end

    local sprite=drawableSpriteStruct.fromTexture(texture,entity)
    local Dot=drawableSpriteStruct.fromTexture(dot,entity)
    sprite:setScale(0.1,0.1)
    sprite.depth=-10005
    Dot:setScale(0.08,0.08)
    Dot.depth=-10005
    return {sprite,Dot}
end
return AlignedText