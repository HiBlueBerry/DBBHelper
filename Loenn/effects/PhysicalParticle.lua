local PhysicalParticle={}
PhysicalParticle.name="DBBHelper/PhysicalParticle"
PhysicalParticle.canBackground=true
PhysicalParticle.canForeground=true

PhysicalParticle.fieldInformation={
    Amount={
        fieldType="integer",
        minimumValue=0,
    },
    Alpha={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },
    Color={
        fieldType="color"
    },
    TurbulenceChangeInterval={
        fieldType="number",
        minimumValue=0.0,
    },
    WindAmplifyCoeficientX={
        fieldType="number",
        minimumValue=0.0,
    },
    WindAmplifyCoeficientY={
        fieldType="number",
        minimumValue=0.0,
    },
    MaxVelocity={
        fieldType="number",
        minimumValue=0.0,
    },
    Damping={
        fieldType="number",
        minimumValue=0.0,
        maximumValue=1.0
    },

}
PhysicalParticle.fieldOrder={
    "only","exclude",
    "tag",
    "flag","notflag",
    

    "Amount",
    "ParticleTexture","Color","Alpha",
    "ScrollX","ScrollY",
    "ExtX","ExtY",

    "BasicScaleX","BasicScaleY",
    "BasicScaleRandomAmpX","BasicScaleRandomAmpY",

    "InitVelocityX","InitVelocityY",
    "InitVelocityRandomAmpX","InitVelocityRandomAmpY",
    "ConstantAccelerationX","ConstantAccelerationY",
    "TurbulenceAmplificationX","TurbulenceAmplificationY","TurbulenceChangeInterval",
    "WindAmplifyCoeficientX","WindAmplifyCoeficientY",
    "MaxVelocity","Damping",

    "UniformScaleMode",
    "GenerateTurbulenceWhenInit",
    
    
}
PhysicalParticle.defaultData={
    Amount=240,
    ParticleTexture="particles/snow",Color="FFFFFF",Alpha=1.0,
    ScrollX=0.0,ScrollY=0.0,
    ExtX=0.0,ExtY=0.0,

    BasicScaleX=1.0,BasicScaleY=1.0,
    BasicScaleRandomAmpX=0.1,BasicScaleRandomAmpY=0.1,

    InitVelocityX=0.0,InitVelocityY=0.0,
    InitVelocityRandomAmpX=0.0,InitVelocityRandomAmpY=0.0,
    ConstantAccelerationX=0.0,ConstantAccelerationY=980.0,
    TurbulenceAmplificationX=0.0,TurbulenceAmplificationY=0.0,TurbulenceChangeInterval=0.5,
    WindAmplifyCoeficientX=1.0,WindAmplifyCoeficientY=1.0,
    MaxVelocity=360.0,Damping=0.0,

    UniformScaleMode=false,
    GenerateTurbulenceWhenInit=true,
}

return PhysicalParticle