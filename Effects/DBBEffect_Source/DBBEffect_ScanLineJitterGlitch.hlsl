#define CREATE_SAMPLER(name, texture,index) \
sampler name:register(s##index)= sampler_state \
{ \
    Texture = texture; \
    MinFilter = Linear; \
    MagFilter = Linear; \
    MipFilter = Linear; \
    AddressU = Wrap; \
    AddressV = Wrap; \
}  
struct pInput
{
    vector position : SV_POSITION;
    float2 UV       : TEXCOORD0;
};
texture glitch_tex:register(t0);//用于原图像的纹理采样
texture mask_tex:register(t1);//用于掩码图像的纹理采样
CREATE_SAMPLER(sp,<glitch_tex>,0);
CREATE_SAMPLER( mask_sp,<mask_tex>,1);

float seed=0.0;//随机化种子
float angle=0.0;//抖动方向,弧度制
float strength=0.05;//抖动强度

float RandWithSeed(float seed, float y){return frac(sin(dot(float2(seed, y), float2(12.9898, 78.233)))*12345.6789);}

float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float4 final_color=0;
    float2 direction=float2(cos(angle),sin(angle));
    float jitter=RandWithSeed(pin.UV.y,seed)*2.0-1.0;//(0,1)->(-1,1)
    final_color=tex2D(sp,frac(pin.UV+direction*strength*jitter));

    return lerp(tex2D(sp,pin.UV),final_color,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique ScanLineJitterGlitch {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}