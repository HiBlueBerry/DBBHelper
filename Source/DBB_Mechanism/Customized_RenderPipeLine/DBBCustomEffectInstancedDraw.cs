/*
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Celeste.Mod.DBBHelper.Mechanism
{
    //渲染模式为延迟渲染
    public class DBBCustomEffectInstancedDraw
    {
        //精灵信息
        private struct SpriteInfo
        {
            //纹理索引
            public int textureHash;

            //以下为XNA提供的渲染方案
            //1.依据源矩阵从纹理中进行裁切
            //2.设置纹理空间中的局部中心坐标origin
            //3.依据设置的rotation,scale对裁切的纹理进行局部变换
            //4.将局部中心坐标origin定位到全局坐标position，裁切部分的各像素的坐标顺势移动到全局坐标中
            //5.对全局坐标系下的所有坐标进行matrix变换，所得到的坐标就是最终的屏幕坐标


            //源矩形的左上角以及其宽高
            public float sourceX;

            public float sourceY;

            public float sourceW;

            public float sourceH;

            //目标矩形的左上角以及其宽高
            public float destinationX;

            public float destinationY;

            public float destinationW;

            public float destinationH;
            //精灵整体的染色
            public Color color;

            //精灵在局部坐标系(纹理空间中)的中心的位置
            public float originX;

            public float originY;

            //精灵绕局部坐标系的中心的旋转的角度
            public float rotationSin;

            public float rotationCos;

            //精灵深度
            public float depth;

            //精灵所用到的特效
            public byte effects;
        }
        //设备
        GraphicsDevice graphicsDevice = null;
        //顶点缓冲，存储顶点相关的数据
        private DynamicVertexBuffer vertexBuffer;
        //索引缓冲，用于定义三角形如何连接
        private IndexBuffer indexBuffer;
        //实例信息
        private object[] instanceInfos;
        //精灵信息
        private SpriteInfo[] spriteInfos;
        

    }
}
*/