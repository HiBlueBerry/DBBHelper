local xnaColors = require("consts.xna_colors")
local draw=require("utils.drawing")
local drawLine = require("structs.drawable_line")
local utils=require("utils")
local SaddleBrown=xnaColors.SaddleBrown
local Rope={}
Rope.name="DBBHelper/Rope"

Rope.nodeLimits = {1, 1}
Rope.fieldOrder={
    "x","y",
    "Resolution","IterationNumber",
    "GravitityAcceleration","Stiffness","Damping","TensileCoefficient",
    "StartTexturePath","MiddleTexturePath","EndTexturePath","Depth",
    "OuterColor","OuterColorAlpha","InnerColor","InnerColorAlpha",
    "StartPointFixation","EndPointFixation","AbsorptionMode"}
Allstyle={
    "BG(9100)",
    "FG(-10600)"
}
Rope.fieldInformation={
    Resolution={
        fieldType="integer",
        minimumValue=2
    },
    IterationNumber={
        fieldType="integer",
        minimumValue=5
    },

    Stiffness={
        fieldType="number",
        minimumValue=0.01,
        maximumValue=1.0
    },
    GravitityAcceleration={
        fieldType="number",
        minimumValue=0.01,
    },
    Damping={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    TensileCoefficient={
        fieldType="number",
        minimumValue=1.0,
    },
    OuterColor={
        fieldType="color",
    },
    OuterColorAlpha={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    InnerColor={
        fieldType="color",
    },
    InnerColorAlpha={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    Depth={
        options=Allstyle,
        editable=false
    }
}

Rope.placements = {
    name = "Rope",
    data = {
        Resolution=16,
        IterationNumber=20,
        GravitityAcceleration=980,
        Stiffness=1.0,
        Damping=0.0,
        TensileCoefficient=1.2,
        StartTexturePath="objects/DBB_Items/Rope/rope_start00",
        MiddleTexturePath="objects/DBB_Items/Rope/rope_middle00",
        EndTexturePath="objects/DBB_Items/Rope/rope_end00",
        Depth="BG(9100)",
        OuterColor="FFFFFF",
        OuterColorAlpha=1.0,
        InnerColor="BC8F8F",
        InnerColorAlpha=1.0,
        StartPointFixation=true,
        EndPointFixation=true,
        AbsorptionMode=false
    }
}
Rope.canResize={false,false}
Rope.depth=0
function Rope.sprite(room,entity)
    local node = entity.nodes[1]

    local start={entity.x, entity.y}
    local stop={node.x, node.y}
    local control={(start[1]+stop[1])/2,(start[2]+stop[2])/2+18}
    local points=draw.getSimpleCurve(start,stop,control,16)
    local sprite=drawLine.fromPoints(points,SaddleBrown,1)
    if(entity.Depth=="FG(-10600)")then
        sprite.depth=-10004
    end
    return sprite
end

function Rope.selection(room, entity)
    local first=utils.rectangle(entity.x-2, entity.y-2,5,5)
    local node_rectangle = {}
    if entity.nodes then
        for i,node in ipairs(entity.nodes) do
            node_rectangle[i]=utils.rectangle(node.x-2,node.y-2,5,5)
        end
    end
    return first,node_rectangle
end

return Rope
