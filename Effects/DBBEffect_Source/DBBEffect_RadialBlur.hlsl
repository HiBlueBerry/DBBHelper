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
CREATE_SAMPLER( mask_sp,<mask_tex>,1);

float2 center=float2(0.5,0.5);//径向模糊的聚焦点，(0.5,0.5)为中心
float blur_radius=0.01;//模糊半径，正数时呈现向外发散的效果，负数时呈现向内收缩的效果
int iter=5;//模糊次数，次数越多径向模糊效果越重

float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度

    float4 final_color=0;    
    float2 blur_vector=(center-pin.UV)*blur_radius;
    float2 uv=pin.UV;
    for(int i=0;i<iter;i++)
    {
        final_color+=tex2D(sp,uv);
        uv+=blur_vector;
    }
    
    return lerp(tex2D(sp,pin.UV),final_color/iter,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效
}
technique RadialBlur {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}