local drawableSpriteStruct=require("structs.drawable_sprite")
local TileGridAlphaDrivenDecal={}
TileGridAlphaDrivenDecal.name="DBBHelper/TileGridAlphaDrivenDecal"
TileGridAlphaDrivenDecal.nodeLimits = {1, 1}
TileGridAlphaDrivenDecal.nodeLineRenderType = "line"
TileGridAlphaDrivenDecal.justification={0.5,0.5}
Preset={
    10000,
    9500,
    9000,
    8000,
    5000,
    2000,
    1000,
    100,
    0,
    -50,
    -100,
    -200,
    -8000,
    -8500,
    -9000,
    -10000,
    -10500,
    -11000,
    -11500,
    -12000,
    -12500,
    -13000,
    -50000,
    -1000000,
    -2000000,
}
TileGridAlphaDrivenDecal.fieldOrder={
    "x","y","Decal","Alpha","ScaleX","ScaleY","Rotation","Color","Depth"
}
TileGridAlphaDrivenDecal.fieldInformation={
    Alpha={
        minimumValue=0.0,
        maximumValue=1.0
    },
    Color={
        fieldType="color",
    },
    Depth={
        fieldType="integer",
        options=Preset,
        editable=true
    },
}
TileGridAlphaDrivenDecal.placements={
    name="TileGridAlphaDrivenDecal",
    data={
        Decal="objects/DBB_Items/TileGridAlphaDrivenDecal/sprite_not_found_global",
        Alpha=1.0,
        ScaleX=1.0,
        ScaleY=1.0,
        Rotation=0.0,
        Color="FFFFFF",
        Depth=9000
    }
}
function TileGridAlphaDrivenDecal.sprite(room, entity)
    local texture1=entity.Decal
    local sprite1=drawableSpriteStruct.fromTexture(texture1,entity)
    if sprite1==nil then
        sprite1=drawableSpriteStruct.fromTexture("objects/DBB_Items/TileGridAlphaDrivenDecal/sprite_not_found_global",entity)
        sprite1.depth=entity.Depth
        sprite1:setColor("FF0000")
        sprite1:setScale(entity.ScaleX,entity.ScaleY)
    else
        sprite1.depth=entity.Depth
        sprite1:setColor(entity.Color)
        sprite1:setScale(entity.ScaleX,entity.ScaleY)
    end
    sprite1.rotation=entity.Rotation/180.0*math.pi
    
    return {sprite1}
end

function TileGridAlphaDrivenDecal.nodeSprite(room, entity, node, nodeIndex, viewport)
    local sprite = drawableSpriteStruct.fromTexture("objects/DBB_Items/TileGridAlphaDrivenDecal/node_select",entity)
    sprite.depth=entity.Depth
    sprite:setColor(entity.Color)
    sprite:setScale(0.5,0.5)
    sprite:setPosition(entity.nodes[nodeIndex].x, entity.nodes[nodeIndex].y)
    return sprite
end
return TileGridAlphaDrivenDecal