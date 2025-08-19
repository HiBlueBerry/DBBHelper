local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local ConditionalLightningBreakerBox = {}

ConditionalLightningBreakerBox.name ="DBBHelper/ConditionalLightningBreakerBox"
ConditionalLightningBreakerBox.depth = -10550
ConditionalLightningBreakerBox.texture = "objects/breakerBox/Idle00"
ConditionalLightningBreakerBox.fieldInformation = {
    music_progress = {
        fieldType = "integer",
    },
}
ConditionalLightningBreakerBox.ignoredFields={"flag","_name","_id"}
ConditionalLightningBreakerBox.placements = {
    name = "ConditionalLightningBreakerBox",
    data = {
        flipX=false,
        param="progress",
        param_value=-1.0,
        music="",
        label="",
        flag=false,
        param_session=false,
        permanent=false
    }
}

function ConditionalLightningBreakerBox.scale(room, entity)
    local scaleX = entity.flipX and -1 or 1
    return scaleX, 1
end

function ConditionalLightningBreakerBox.justification(room, entity)
    local flipX = entity.flipX
    return flipX and 0.75 or 0.25, 0.25
end

return ConditionalLightningBreakerBox