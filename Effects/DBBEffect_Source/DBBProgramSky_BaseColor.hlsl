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
texture effect_tex:register(t0);//用于原图像的纹理采样
CREATE_SAMPLER(sp,<effect_tex>,0);


//RGB颜色并不适合人眼感知，这里用YUV颜色空间进行插值
float4 rgb_to_yuv(float4 rgb)
{
    float r=rgb.x;float g=rgb.y;float b=rgb.z;
    float y=0.299*r+0.587*g+0.114*b;
    float u=-0.147*r-0.289*g+0.436*b;
    float v=0.615*r-0.515*g-0.100*b;
    return float4(y,u,v,1.0f);
}
//对YUV插值
float4 interpolate_yuv(float4 c1, float4 c2, float time)
{
    float4 tmp1=rgb_to_yuv(c1);
    float4 tmp2=rgb_to_yuv(c2);
    float4 yuv_c=lerp(tmp1,tmp2,time);
    //转回RGB
    float y=yuv_c.x;float u=yuv_c.y;float v=yuv_c.z;
    return float4(y+1.140*v, y-0.395*u-0.581*v, y+2.032*u,1.0);
}
//插值
float time=0.0f;
float4 color1_bottom=0;
float4 color1_mid=0;
float4 color1_top=0;

float4 color2_bottom=0;
float4 color2_mid=0;
float4 color2_top=0;
float4 main(pInput pin):SV_TARGET
{

    float4 sky_color=0;
    float4 color1=0;
    float4 color2=0;
    
    if(pin.UV.y<0.5f)
    {
        color1=lerp(color1_bottom,color1_mid,2.0f*pin.UV.y);
        color2=lerp(color2_bottom,color2_mid,2.0f*pin.UV.y);
    }
    else
    {
        color1=lerp(color1_mid,color1_top,2.0f*(pin.UV.y-0.5f));
        color2=lerp(color2_mid,color2_top,2.0f*(pin.UV.y-0.5f));
    }
    sky_color=interpolate_yuv(color1,color2,time);
    return sky_color;
}
technique ProgramSky_BaseColor {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}