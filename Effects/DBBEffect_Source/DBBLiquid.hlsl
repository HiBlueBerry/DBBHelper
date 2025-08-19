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
texture image_tex:register(t0);//用于原图像的纹理采样
CREATE_SAMPLER(sp,<image_tex>,0);

float2 RandWithUV(float2 p)
{
    float3 p3=frac(float3(p.xyx)*float3(0.1031,0.1030,0.0973));
    p3=p3+dot(p3,p3.yzx+19.19);
    return -1.0+2.0*frac((p3.xx+p3.yz)*p3.zy);
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

    float2 bottom_left_val=RandWithUV(bottom_left);
    float2 top_left_val=RandWithUV(top_left);
    float2 bottom_right_val=RandWithUV(bottom_right);
    float2 top_right_val=RandWithUV(top_right);
    //立方插值，用于平滑噪声图
    float2 final_f=f*f*(3.0-2.0*f);
    float commit1=dot(bottom_left_val,f);
    float commit2=dot(top_left_val,f-float2(0,1));
    float commit3=dot(bottom_right_val,f-float2(1,0));
    float commit4=dot(top_right_val,f-float2(1,1));
    //对uv点使用网格的四个角进行双线性插值
    float p1=commit1*(1.0-final_f.x)+commit3*final_f.x;
    float p2=commit2*(1.0-final_f.x)+commit4*final_f.x;
    return p1*(1.0-final_f.y)+p2*final_f.y;
}

//普通分形，每次叠加更高频率更低幅度的噪声来形成分形
float FBM2D(float2 t,float scale,int num,float amp,float fre,bool frac_mode)
{
    float value=0.0;
    float f_amp=amp;
    float f_fre=fre;
    for (int i = 0; i < num; i++) {
        value+=f_amp*Noise(t,scale);
        t*=fre;
        f_amp*=0.5;
    }
    if(frac_mode==true)
    {
        value=frac(value);
    }
    return value;
}
//湍流分形
float FBM2DT(float2 t,float scale,int num,float amp,float fre,bool frac_mode)
{
    float value=0.0;
    float f_amp=amp;
    float f_fre=fre;
    for (int i = 0; i < num; i++) {
        value+=f_amp*abs(Noise(t,scale));
        t*=fre;
        f_amp*=0.5;
    }
    if(frac_mode==true)
    {
        value=frac(value);
    }
    return value;
}

float FBM2DC(float2 t,float scale,int num,float amp,float fre)
{
    float value=FBM2DT(t,scale,num,amp,fre,false);
    value=1.0-value;
    value=value*value;
    return value;
}


float4 Mix(float4 c1,float4 c2,float p)
{
    return c1*(1.0-p)+c2*p;
}

//控制液体的整体效果
float scale=2.0;//缩放等级
float amp=1.5;//液体幅度
float fre=2.0;//液体细节度
bool frac_mode=false;//frac模式
float4 color1=float4(0.3,0.6,0.4,1.0);//颜色1
float4 color2=float4(0.4,0.95,0.9,1.0);//颜色2

//控制水平黑波范围
float2 horizontal_bottom_and_top=float2(0.0,1.0);//底部的黑边的开始减退的位置，最大为0.5和顶部的黑边的开始渐退的位置,最小为0.5
float black_edge_amp=0.2;//黑波幅度

//控制垂直黑边的范围
float2 vertical_left_and_right=float2(0.1,0.8);//左侧的黑边的开始减退的位置和右侧的黑边的开始减退的位置
float vertical_offset=0.5;//黑边的参考偏移,0.5为正中心，更大的值会让黑边向左，更小的值会让黑边向右

//控制液体的高光
float4 highlight_color1=float4(0.0,0.0,0.0,1.0);//高光颜色1
float4 highlight_color2=float4(1.0,1.0,1.0,1.0);//高光颜色1
float highlight_amp=0.2;//高光的幅度

//控制液体对原图像的影响幅度
float2 wave_influence_amp=float2(0.08,0.0);

//控制液体波动和流动方向
float2 move_dir=float2(1,0);//液体流动方向
float2 wave_dir=float2(1,0);//液体波动方向

float4 main(pInput pin):SV_TARGET
{
    
    //控制液体的整体效果
    float num_level=6;

    float4 black=0;float4 white=1;
    float2 uv=pin.UV;
    float q=Noise(uv+wave_dir,4.0);
    float random_value=black_edge_amp*abs(q);
    float vertical_bottom=smoothstep(horizontal_bottom_and_top.x+random_value,0.5,uv.y);
    float vertical_top=smoothstep(0.5,horizontal_bottom_and_top.y-random_value,uv.y);
    float up_down_mask=0;
    up_down_mask=lerp(black,white,vertical_bottom);
    if(uv.y>0.5)
    {
        up_down_mask=lerp(white,black,vertical_top);
    }

    float vertical=0;


    float q1=FBM2DT(uv+float2(q*0.1,0)+move_dir,scale,num_level,amp,fre,frac_mode);
    float q2=FBM2DC(uv+float2(q*0.1,0)+move_dir,scale*2,num_level,amp,fre*0.8);
    float4 highlight_color=Mix(highlight_color1,highlight_color2,q2);
    float4 final_color1=Mix(color1,color2,q1);

    float distance=abs(uv.x-vertical_offset);
    float horizontal_mask=1.0-smoothstep(vertical_left_and_right.x,vertical_left_and_right.y,distance);
    float4 color=(final_color1+highlight_color*highlight_amp);

    float2 image_uv=pin.UV+float2(wave_influence_amp.x*q1,wave_influence_amp.y*q1);
    float4 image_color=tex2D(sp,float2(image_uv.x,image_uv.y));

    float4 final_color=color+image_color;
    final_color.rgb=final_color.rgb*horizontal_mask*up_down_mask;
    
    return final_color;
}

technique DBBLiquid {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}