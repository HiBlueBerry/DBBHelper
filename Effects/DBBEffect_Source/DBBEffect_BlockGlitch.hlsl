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

float RandWithSeed(float seed, float y)
{
    return frac(sin(dot(float2(seed, y), float2(12.9898, 78.233)))*12345.6789);
}

float randomNoise(float2 seed,float time)
{
	return frac(sin(dot(seed*floor(time), float2(12.9898, 78.233)))*12345.6789);
}

float time=0.0;//用于控制图块故障的时间参数，应当为一个不断变化的值
float block_num=4;//每行的块数目
float strength=1.0;//用于控制故障特效的强度
int rgb_split=1;//是否为rgb分离故障

float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float val=randomNoise(floor(pin.UV*block_num),time);

    float displacement=pow(val,8.0)*pow(val,3.0);

    float4 final_color=0;
    float4 r=tex2D(sp,pin.UV);
    float4 g=tex2D(sp,pin.UV+float2(RandWithSeed(7.0,2)*displacement*strength,0));
    float4 b=tex2D(sp,pin.UV-float2(RandWithSeed(13.0,2)*displacement*strength,0));
    final_color=float4(r.r,g.g,b.b,1);
    float final_color_another = (r + g + b) / 3.0;//rgb分离特效禁用的情况
    final_color = lerp(final_color_another, final_color, rgb_split);
    
    return lerp(tex2D(sp,pin.UV),final_color,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique BlockGlitch {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}