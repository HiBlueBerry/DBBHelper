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
texture draw:register(t0);
texture light_mask:register(t1);
texture mask_tex:register(t2);

CREATE_SAMPLER(sp,<draw>,0);//用于原图像的纹理采样
CREATE_SAMPLER(light_sp,<light_mask>,1);//用于光照贴图的纹理采样
CREATE_SAMPLER(mask_sp,<mask_tex>,2);//用于掩码图像的纹理采样

float RandWithUV(float2 uv) {
    return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}
//简单的二维Perlin梯度噪声，scale为噪声图缩放，strength为噪声强度
//一种优化方式：Simplex噪声，生成噪声图的时间由O(2^N)转变为O(N^2)，其中N为维数
float Noise(float2 uv,float scale)
{
    //依据scale划分网格,scale越大划分的网格越密集
    float2 f=frac(uv*scale);
    float2 bottom_left=floor(uv*scale);
    float2 top_left=bottom_left+float2(0,1.0);
    float2 bottom_right=bottom_left+float2(1.0,0);
    float2 top_right=bottom_left+float2(1.0,1.0);
    //生成uv点所对应网格的四个角的随机方向向量
    float bottom_left_val=RandWithUV(bottom_left);
    float top_left_val=RandWithUV(top_left);
    float bottom_right_val=RandWithUV(bottom_right);
    float top_right_val=RandWithUV(top_right);
    //立方插值，用于平滑噪声图
    float2 final_f=f*f*(3.0-2.0*f);
    
    //对uv点使用网格的四个角进行双线性插值
    float p1=bottom_left_val*(1.0-final_f.x)+bottom_right_val*final_f.x;
    float p2=top_left_val*(1.0-final_f.x)+top_right_val*final_f.x;
    return p1*(1.0-final_f.y)+p2*final_f.y;
}
//每次叠加更高频率更低幅度的噪声来形成分形
float FBM(float2 uv,int num,float amp,float fre,bool frac_mode)
{
    float value=0.0;
    float2 f_uv=uv;
    float f_amp=amp;
    float f_fre=fre;
    for (int i = 0; i < num; i++) {
        value+=f_amp*Noise(f_uv,1.0);
        f_uv*=fre;
        f_amp*=0.5;
    }
    return frac_mode ? frac(value) : value;
}
float4 Mix(float4 c1,float4 c2,float p)
{
    return lerp(c1, c2, p);
}
float2 time=0.0;//时间参数，应当不断变化以产生变化的效果
float amp=0.5;//分形强度
float fre=2.0;//分形细节度
float light_influence_coefficient=1.0;//光照影响系数，用于控制光照对流体的影响强度
int num_level=6;//分形次数，性能损耗项
bool frac_mode=false;//取余数模式

//采用四个颜色值来生成流体效果
float4 color1=float4(0.1,0.6,0.7,1.0);//颜色1
float4 color2=float4(0.0,0.0,0.2,1.0);//颜色2
float4 color3=float4(0.7,1.0,1.0,1.0);//颜色3
float4 color4=float4(0.7,0.7,0.5,1.0);//颜色4
//Tint颜色
float4 tint=float4(1.0,1.0,1.0,1.0);

float4 main(pInput pin):SV_TARGET
{
    float4 mask = tex2D(light_sp, pin.UV);
    mask.a = dot(mask.rgb, float3(0.299, 0.587, 0.114));//计算光照贴图的图像亮度

    float4 mask1 = tex2D(mask_sp, pin.UV);
    mask1.a = dot(mask1.rgb, float3(0.299, 0.587, 0.114));//计算掩码图像亮度
    
    float2 q = float2(
        FBM(pin.UV + time, num_level, amp, fre, frac_mode),
        FBM(pin.UV + float2(1.0, 0.0), num_level, amp, fre, frac_mode)
    );
    
    float2 r = float2(
        FBM(pin.UV + q + float2(1.7, 9.2) + 0.15 * time, num_level, amp, fre, frac_mode),
        FBM(pin.UV + q + float2(8.3, 2.8) + 0.126 * time, num_level, amp, fre, frac_mode)
    );

    float f = FBM(pin.UV + r, num_level, amp, fre, frac_mode);
    float4 final_color = lerp(color1, color2, saturate(f * f * 4.0));
    final_color = lerp(final_color, lerp(color3, color4, saturate(length(q))), saturate(length(r)));
    float strength = saturate(1.0 - mask.a * light_influence_coefficient);
    final_color = final_color * tint;
    final_color.a *= strength;
    return lerp(tex2D(sp, pin.UV), final_color, mask1.a);
}
technique FBMFluid {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}