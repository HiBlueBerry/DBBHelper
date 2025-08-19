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
CREATE_SAMPLER(mask_sp,<mask_tex>,1);

float randomNoise(float2 seed,float time)
{
	return frac(sin(dot(seed*floor(time), float2(12.9898, 78.233)))*12345.6789);
}

float num_level=4.0;//线条块基本层级数，此值越高故障线条快越多
float detail=2.0;//故障线条块的细节，此值越高每个线条块会出现更多细节
float strength=0.01;//故障特效强度
float time=0.0;//时间参数，此值应当不断变化以获得随机的故障效果
int vertical=0;//故障线条是横的还是竖的
int rgb_split=1;//是否为rgb分离故障

float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度
    float4 final_color=0;
    float2 dir=lerp(float2(1,0),float2(0,1),vertical);
    float2 uv=lerp(pin.UV.yy,pin.UV.xx,vertical);

    float pic_strength=0;
    float pic_detail=randomNoise(floor((uv+float2(2,2))*detail),time);
    pic_strength=randomNoise(floor(pic_detail+(uv+float2(1,1))*num_level),time);
    pic_strength*=strength;
    pic_strength=1.0-clamp(pic_strength,0,1);

    float4 r=tex2D(sp,pin.UV);
    float4 g=tex2D(sp,pin.UV+dir*pic_strength);
    float4 b=tex2D(sp,pin.UV-dir*pic_strength);
    final_color=float4(r.r,g.g,b.b,1);
    final_color=lerp((r+b+g)/3.0,float4(r.r,g.g,b.b,1),rgb_split);
    return lerp(tex2D(sp,pin.UV),final_color,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique LineGlitch {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}