struct pInput
{
    vector position : SV_POSITION;
    float4 Color    : COLOR;
    float2 UV       : TEXCOORD;
};
float2 RandWithUV2(float2 st){
    st=float2( dot(st,float2(127.1,311.7)), dot(st,float2(269.5,183.3)));
    return -1.0 + 2.0*frac(sin(st)*43758.5453);
}
//简单的二维Perlin梯度噪声，scale为噪声图缩放，strength为噪声强度
//一种优化方式：Simplex噪声，生成噪声图的时间由O(2^N)转变为O(N^2)，其中N为维数
float Perlin(float2 uv,float scale,float strength)
{
    //依据scale划分网格,scale越大划分的网格越密集
    float2 f=frac(uv*scale);
    float2 bottom_left=floor(uv*scale);
    float2 top_left=bottom_left+float2(0,1.0);
    float2 bottom_right=bottom_left+float2(1.0,0);
    float2 top_right=bottom_left+float2(1.0,1.0);
    //生成uv点所对应网格的四个角的随机方向向量
    float2 bottom_left_val=RandWithUV2(bottom_left);
    float2 top_left_val=RandWithUV2(top_left);
    float2 bottom_right_val=RandWithUV2(bottom_right);
    float2 top_right_val=RandWithUV2(top_right);
    //立方插值，用于平滑噪声图
    float2 final_f=f*f*(3.0-2.0*f);
    
    //对uv点使用网格的四个角进行双线性插值
    float p1=dot(bottom_left_val,f)*(1.0-final_f.x)+dot(bottom_right_val,f-float2(1,0))*final_f.x;
    float p2=dot(top_left_val,f-float2(0,1))*(1.0-final_f.x)+dot(top_right_val,f-float2(1,1))*final_f.x;
    return clamp(strength*(p1*(1.0-final_f.y)+p2*final_f.y),0.0,1.0);
}
float scale=1.0;//噪声图缩放
float strength=2.0;//噪声强度
float4 main(pInput pin):SV_TARGET
{
   float4 final_color=0;
   final_color=Perlin(pin.UV,scale,strength);
   final_color.a=1.0;
   return final_color;
}
technique PerlinNoise {
	pass pass0 {
		PixelShader=compile ps_3_0 main();
	}
}