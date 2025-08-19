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
    float4 Color    : COLOR;
    float2 UV       : TEXCOORD;
};
texture glitch_tex:register(t0);//用于原图像的纹理采样
texture mask_tex:register(t1);//用于掩码图像的纹理采样
CREATE_SAMPLER(sp,<glitch_tex>,0);
CREATE_SAMPLER(mask_sp,<mask_tex>,1);

float seed=0.0;//随机数种子
float split_amount=0.01;//特效分离程度
float angle=0.0;//特效分离方向的角度，弧度制
int uv_mode=0;//此值为真时使用RandWithUV，否则使用RandWithSeed

float RandWithSeed(float seed, float y)
{
    return frac(sin(dot(float2(seed, y), float2(12.9898, 78.233)))*12345.6789);
}
float RandWithUV(float2 uv)
{
    return sin(dot(uv, half2(1233.224, 1743.335)));
}
float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float colr=0;float colg=0;float colb=0;
    float2 random=0;
    float2 direction=float2(cos(angle),sin(angle));
    random=lerp(direction*RandWithSeed(seed,2)*split_amount,direction*RandWithUV(pin.UV)*split_amount,uv_mode);

    colr=tex2D(sp,pin.UV+random).x;
    colg=tex2D(sp,pin.UV).y;
    colb=tex2D(sp,pin.UV-random).z;
    return lerp(tex2D(sp,pin.UV),float4(colr,colg,colb,1),mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique ColorShiftGlitch {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}