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

float time=0.0;//噪点故障的时间参数，应当是一个不断变化的值以产生不断的变化故障效果
float strength=0.25;//故障强度
float jitter_time=0.0;//灰度抖动的时间参数，应该是一个不断变化的值，但若想产生周期性的灰度抖动效果，应该将其离散化(例如令其等于floor(time))
float jitter_threshold=0.5;//灰度抖动的阈值，当随机产生的值达到阈值时发生灰度抖动，此值越高越难发生抖动
int grey_jitter=0;//灰度抖动模式

float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float4 final_color=0;
    float2 offset=float2(1,0);
    float noiseX=randomNoise(pin.UV,time);
    float noiseY=randomNoise(pin.UV+offset,time);
    float noiseZ=randomNoise(pin.UV-offset,time);
    float grey=dot(float3(noiseX,noiseY,noiseZ),float3(0.22, 0.707, 0.071));

    float4 noise_color=float4(noiseX,noiseY,noiseZ,1);
    float4 tex_color=tex2D(sp,pin.UV);

    float lumi_rand=RandWithSeed(jitter_time,1.0);
    //避免分支
    float useGrey=step(jitter_threshold, lumi_rand)*grey_jitter;
    noise_color=lerp(noise_color,float4(grey, grey, grey, 1),useGrey);
    /*
        if(grey_jitter==true&&lumi_rand>jitter_threshold)
        {
            noise_color=float4(grey,grey,grey,1);   
        }
    */
    final_color=tex_color;
    final_color+=strength*noise_color-0.5*strength;//此代码保持平均亮度不变
    final_color.w=1;

    return lerp(tex2D(sp,pin.UV),final_color,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique AnalogNoiseGlitch {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}