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
texture color_spectrum:register(t0);
CREATE_SAMPLER(sp,<color_spectrum>,0);
float3 HSVToRGB(float3 hsv)
{
    float h = hsv.x;
    float s = hsv.y;
    float v = hsv.z;
    
    float c = v * s;
    float x = c * (1 - abs(fmod(h * 6, 2) - 1));
    float m = v - c;
    
    float3 rgb;
    if (h < 1.0 / 6.0) {
        rgb = float3(c, x, 0);
    } else if (h < 2.0 / 6.0) {
        rgb = float3(x, c, 0);
    } else if (h < 3.0 / 6.0) {
        rgb = float3(0, c, x);
    } else if (h < 4.0 / 6.0) {
        rgb = float3(0, x, c);
    } else if (h < 5.0 / 6.0) {
        rgb = float3(x, 0, c);
    } else {
        rgb = float3(c, 0, x);
    }
    
    rgb += m;
    return rgb;
}

//给色谱条用的
float4 main(pInput pin):SV_TARGET
{
    float hue=pin.UV.y;//色相，范围从0到1
    // 饱和度
    float S = 1.0;
    // 明度/亮度
    float V = 1.0;
    // 创建HSV颜色
    float3 hsv = float3(hue, S, V);
    // 转换为RGB
    float3 rgb = HSVToRGB(hsv);
    return float4(rgb, 1);
}

technique ColorSpectrum {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}