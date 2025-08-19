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
texture glitch_tex:register(t0);
texture mask_tex:register(t1);
CREATE_SAMPLER(sp,<glitch_tex>,0);//用于原图像的纹理采样
CREATE_SAMPLER(mask_sp,<mask_tex>,1);//用于掩码图像的纹理采样

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
float FBM(float2 uv,int num,float amp,float fre,float detail,bool frac_mode)
{
    float value=0.0;
    float2 f_uv=uv;
    float f_amp=amp;
    for (int i = 0; i < num; i++) {
        value+=f_amp*Noise(f_uv,detail);
        f_uv*=fre;
        f_amp*=0.5;
    }
    if(frac_mode==true)
    {
        value=frac(value);
    }
    return value;
}
float time=0.0;//时间参数，用于产生动态特效
float strength=0.02;//特效强度
int vertical=0;//波动方向是水平还是垂直
int rgb_split=1;//特效是否为RGB分离效果
float4 main(pInput pin):SV_TARGET
{
    float4 mask=0;
    mask=tex2D(mask_sp,pin.UV);
    mask.a=0.299*mask.r+0.587*mask.g+0.114*mask.b;//计算掩码图像亮度
    
    float amp=0.5;
    float4 final_color=1;
    float fre=2.5;
    float detail=1.8;
    float frac_mode=false;
    int num=4;
    float2 dir=lerp(float2(1,0),float2(0,1),vertical);
    float final_val=FBM(pin.UV+dir*time,num,amp,fre,detail,frac_mode);
    float V=FBM(pin.UV+final_val,num,amp,fre,detail,frac_mode);
    V=FBM(pin.UV+V,num,amp,fre,detail,frac_mode);
    
    float4 color1=tex2D(sp,pin.UV);
    float4 color2=tex2D(sp,pin.UV-dir*V*strength);
    float4 color3=tex2D(sp,pin.UV+dir*V*strength);
    final_color.r=color1.r;
    final_color.g=color2.g;
    final_color.b=color3.b;
    final_color=lerp((color1+color2+color3)/3.0f,final_color,rgb_split);
    final_color.a=1.0f;
    
    return lerp(tex2D(sp,pin.UV),final_color,mask.a);//使用掩码来控制哪些地方有特效，哪些地方没有特效

}
technique FluidGlitch {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}