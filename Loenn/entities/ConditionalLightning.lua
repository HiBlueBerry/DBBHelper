local ConditionalLightning = {}

ConditionalLightning.name ="DBBHelper/ConditionalLightning"
ConditionalLightning.depth = -1000100
ConditionalLightning.fillColor = {0.55, 0.97, 0.96, 0.4}
ConditionalLightning.borderColor = {0.99, 0.96, 0.47, 1.0}
ConditionalLightning.nodeLineRenderType = "line"
ConditionalLightning.nodeLimits = {0, 1}
ConditionalLightning.ignoredFields={"perLevel","_name","_id"}
ConditionalLightning.placements = {
    name = "ConditionalLightning",
    data = {
        width=8,
        height=8,
        perLevel=false,
        label="",
        moveTime=5.0,
        permanent=false
    }
}
return ConditionalLightning