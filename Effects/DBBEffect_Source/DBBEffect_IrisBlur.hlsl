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
texture blur_tex:register(t0);//用于原图像的纹理采样
texture mask_tex:register(t1);//用于掩码图像的纹理采样
CREATE_SAMPLER(sp,<blur_tex>,0);
CREATE_SAMPLER(mask_sp,<mask_tex>,1);

float IrisMask(float2 uv,float offset,float area,float spread)
{
    float2 center=uv*2.0-1.0+offset; //[0,1]->[-1,1] 
    return pow(dot(center,center)*area,spread);
}

//光圈模糊
float stride=0.02;//控制Kawase模糊(对角线采样)的采样步长
float offset=0.0;//控制光圈区域的中心位置，0.0为光圈区域在中心
float area=1.0;//控制光圈区域的面积，取值为0到1
float spread=1.0;//控制光圈的发散程度，此值可正可负，负数时反相
int mask_mode=0;//掩码模式，此模式方便调试，掩码的黑色区域即为清晰的区域
float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float4 final_color=0;
    for(int i=1;i<5;i++)
    {
        float4 tmp_color=0;
        float tmp=stride*i;
        float2 uv1=pin.UV+float2(0.05,0.05)*tmp;
        float2 uv2=pin.UV+float2(0.05,-0.05)*tmp;
        float2 uv3=pin.UV+float2(-0.05,0.05)*tmp;
        float2 uv4=pin.UV+float2(-0.05,-0.05)*tmp;
        tmp_color+=tex2D(sp,uv1);
        tmp_color+=tex2D(sp,uv2);
        tmp_color+=tex2D(sp,uv3);
        tmp_color+=tex2D(sp,uv4);
        final_color+=0.25*tmp_color;
    }
    final_color=0.25*final_color;
    float proption=clamp(IrisMask(pin.UV,offset,area,spread),0.0,1.0);
    float4 final_color1=(1.0-proption)*tex2D(sp,pin.UV)+proption*final_color;
    float4 final_color2=proption*final_color;
    final_color2.w=1;
    final_color=lerp(final_color1,final_color2,mask_mode);
    return lerp(tex2D(sp,pin.UV),final_color,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique IrisBlur {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}