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

//相较于基础故障特效，高级故障特效使用了两种不同规格的故障块，同时它们有单独的时间参数
float time1=0.0;//用于控制故障块1的时间参数，应当为一个不断变化的值
float time2=0.0;//用于控制故障块2的时间参数，应当为一个不断变化的值
float2 size1_uv=float2(4,4);//故障块1的规格
float2 size2_uv=float2(4,4);//故障块2的规格
float strength=1.0;//用于控制故障特效的强度
int rgb_split=1;//是否为rgb分离故障

float2 offset=float2(0,0);

float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float val1=randomNoise(floor(pin.UV*size1_uv),time1);
    float val2=randomNoise(floor(pin.UV*size2_uv),time2);
    float displacement=pow(val1,8.0)*pow(val2,3.0);
    float4 final_color=0;
    float4 r=tex2D(sp,pin.UV);
    float4 g=tex2D(sp,pin.UV+float2(RandWithSeed(7.0,2)*displacement*strength,0));
    float4 b=tex2D(sp,pin.UV-float2(RandWithSeed(13.0,2)*displacement*strength,0));

    float4 final_color1=float4(r.r,g.g,b.b,1);//RGB分离启用
    float4 final_color2=(r+g+b)/3.0;//RGB分离禁用
    final_color=rgb_split*final_color1+(1-rgb_split)*final_color2;
    
    return lerp(tex2D(sp,pin.UV),final_color,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique AdvancedBlockGlitch {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}