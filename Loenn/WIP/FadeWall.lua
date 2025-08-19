local fadeTilesHelper = require("helpers.fake_tiles")

local DBBFadeWall = {}
DBBFadeWall.name="DBBHelper/DBBFadeWall"
DBBFadeWall.placements={
    name="DBBFadeWall",
    data={
        width=8,
        height=8,
        tiletype=fadeTilesHelper.getPlacementMaterial(),
        StartAlpha=1.0,
        EndAlpha=0.0,
        FadeVelocity=1.0,
    }
}
DBBFadeWall.fieldOrder={
    "x","y","width","height","tiletype","StartAlpha","EndAlpha","FadeVelocity"
}
local otherInfo={
    StartAlpha={
        minimumValue=0.0,
        maximumValue=1.0
    },
    EndAlpha={
        minimumValue=0.0,
        maximumValue=1.0
    },
    FadeVelocity={
        minimumValue=0.1,
        maximumValue=5.0
    },
}
DBBFadeWall.sprite=fadeTilesHelper.getEntitySpriteFunction("tiletype",true,"tilesFg",{1.0, 1.0, 1.0, 0.7})
DBBFadeWall.fieldInformation=fadeTilesHelper.addTileFieldInformation(otherInfo,"tiletype")

return DBBFadeWall