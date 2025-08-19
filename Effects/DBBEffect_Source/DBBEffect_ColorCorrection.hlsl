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

//应用色调
float3 ApplyTintColor(float3 color ,float3 tint_color,float tint_strength)
{
    return lerp(color,color*tint_color,tint_strength);
}
//应用饱和度
float3 ApplySaturation(float3 color,float saturation)
{
    float grayColor = dot(color, float3(0.2126, 0.7152, 0.0722));
    return lerp(float3(grayColor,grayColor,grayColor),color,saturation);
}
//应用HDR
float3 ApplyHDR(float3 hdrColor, float exposure, float gamma)
{
    //曝光色调映射
    float3 mapped=hdrColor/(1.0+(1.0-exposure)*hdrColor);
    //Gamma校正 
    mapped=pow(mapped,float3(1.0/gamma,1.0/gamma,1.0/gamma));
    return mapped;
}
//应用对比度
float3 ApplyContrast(float3 color,float contrast)
{
    float3 avgColor=float3(0.5,0.5,0.5);
    return avgColor*(1.0-contrast)+color*contrast;
}

float3 tint_color=float3(1.0,1.0,1.0);//色调颜色，白色为正常图像
float tint_strength=0.0f;//色调强度，0.0为无色调调节的效果
float saturation=1.0f;//饱和度，1.0为正常图像
float exposure=1.0f;//曝光度，1.0为正常图像
float gamma=1.0f;//gamma修正，1.0为正常图像
float contrast=1.0f;//对比度，1.0为正常图像

//颜色矫正
float4 main(pInput pin):SV_TARGET
{
    float4 origin=tex2D(sp,pin.UV);
    //按照色调、饱和度、HDR的顺序进行调节
    float3 result=ApplyTintColor(origin.rgb,tint_color,tint_strength);
    result=ApplySaturation(result,saturation);
    result=ApplyContrast(ApplyHDR(result,exposure,gamma),contrast);
    return float4(result,origin.a);
}
technique ColorCorrection{
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}