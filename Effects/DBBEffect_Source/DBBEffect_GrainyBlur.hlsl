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

float blur_radius=0.001;//粒状模糊的模糊半径

texture blur_tex:register(t0);
texture mask_tex:register(t1);
CREATE_SAMPLER(sp,<blur_tex>,0);//用于原图像的纹理采样
CREATE_SAMPLER(mask_sp,<mask_tex>,1);//用于掩码图像的纹理采样
//随机数生成器
float Rand(float2 n)
{
    return sin(dot(n, half2(1233.224, 1743.335)));
}
//像素着色器
float4 main(pInput pin):SV_TARGET
{

    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float ran=Rand(pin.UV);
    float2 randomOffset=float2(0.0, 0.0);
	float4 finalColor=float4(0.0, 0.0, 0.0, 0.0);
    int iter=10;
    for (int k=0; k<iter;k++)
    {
        ran=frac(43758.5453*ran+0.61432);;
        randomOffset.x =(ran-0.5)*2.0;
        ran=frac(43758.5453 * ran + 0.61432);
        randomOffset.y=(ran-0.5)*2.0;
        finalColor+=tex2D(sp,pin.UV+randomOffset*blur_radius);
    }
    return lerp(tex2D(sp,pin.UV),finalColor/iter,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique GrainyBlur {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}