/*
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Celeste.Mod.DBBHelper.Mechanism
{
    //自定义精灵绘制
    public class DBBCustomSpriteBatch
    {
        //设备
        GraphicsDevice graphicsDevice = null;
        //包含4个顶点的位置、颜色、纹理数据的结构
        private struct VertexPositionColorTexture4 : IVertexType
        {
            //步长
            public const int RealStride = 96;

            //顶点0
            public Vector3 Position0;

            public Color Color0;

            public Vector2 TextureCoordinate0;

            //顶点1
            public Vector3 Position1;

            public Color Color1;

            public Vector2 TextureCoordinate1;

            //顶点2
            public Vector3 Position2;

            public Color Color2;

            public Vector2 TextureCoordinate2;

            //顶点3
            public Vector3 Position3;

            public Color Color3;

            public Vector2 TextureCoordinate3;


            VertexDeclaration IVertexType.VertexDeclaration
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

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

        //比较纹理索引
        private class TextureComparer : IComparer<nint>
        {
            public unsafe int Compare(nint i1, nint i2)
            {
                return ((SpriteInfo*)i1)->textureHash.CompareTo(((SpriteInfo*)i2)->textureHash);
            }
        }

        //从后往前比较两个精灵的深度
        private class BackToFrontComparer : IComparer<nint>
        {
            public unsafe int Compare(nint i1, nint i2)
            {
                return ((SpriteInfo*)i2)->depth.CompareTo(((SpriteInfo*)i1)->depth);
            }
        }
        //从前往后比较两个精灵的深度
        private class FrontToBackComparer : IComparer<nint>
        {
            public unsafe int Compare(nint i1, nint i2)
            {
                return ((SpriteInfo*)i1)->depth.CompareTo(((SpriteInfo*)i2)->depth);
            }
        }

        //最大的精灵数目
        private const int MAX_SPRITES = 2048;
        //最大的顶点数目
        private const int MAX_VERTICES = 8192;
        //最大的索引数目
        private const int MAX_INDICES = 12288;
        //最大的数组字节数
        private const int MAX_ARRAYSIZE = 699050;
        private static readonly float[] axisDirectionX = new float[4] { -1f, 1f, -1f, 1f };

        private static readonly float[] axisDirectionY = new float[4] { -1f, -1f, 1f, 1f };

        private static readonly float[] axisIsMirroredX = new float[4] { 0f, 1f, 0f, 1f };

        private static readonly float[] axisIsMirroredY = new float[4] { 0f, 0f, 1f, 1f };

        private static readonly float[] CornerOffsetX = new float[4] { 0f, 1f, 0f, 1f };

        private static readonly float[] CornerOffsetY = new float[4] { 0f, 0f, 1f, 1f };
        //----以下为精灵和几何数据的信息----
        //顶点缓冲，存储顶点数据（如位置、颜色、纹理坐标、法线等）
        private DynamicVertexBuffer vertexBuffer;
        //索引缓冲，用于定义三角形如何连接
        private IndexBuffer indexBuffer;
        //精灵信息
        private SpriteInfo[] spriteInfos;
        //排好序的精灵信息
        private nint[] sortedSpriteInfos;
        //顶点信息
        private VertexPositionColorTexture4[] vertexInfo;
        //纹理信息
        private Texture2D[] textureInfo;
        //精灵绘制的特效
        private Effect spriteEffect;
        //精灵的矩阵变换
        private nint spriteMatrixTransform;
        //特效
        private EffectPass spriteEffectPass;
        //----以下为一些绘制时的设置----

        //是否已经开始绘制了
        private bool beginCalled;
        //精灵绘制顺序：
        //SpriteSortMode.Deferred：延迟排序，所有精灵按 SpriteBatch.Draw 调用顺序绘制，性能最高
        //SpriteSortMode.Immediate：每调用一次 Draw 立即提交到 GPU，适合需要实时切换渲染状态，性能较低
        //SpriteSortMode.Texture：按 纹理哈希 排序，减少纹理切换，提高渲染效率
        //SpriteSortMode.BackToFront：按 图层深度（LayerDepth） 从后到前排序
        //SpriteSortMode.FrontToBack：按 图层深度（LayerDepth） 从前到后排序
        private SpriteSortMode sortMode;
        //颜色混合方式
        private BlendState blendState;
        //纹理采样方式
        //SamplerState.LinearClamp：线性过滤，纹理超出范围时钳制（Clamp）到边缘颜色
        //SamplerState.PointClamp：最近邻采样（像素风），钳制到边缘
        //SamplerState.AnisotropicClamp：各向异性过滤（更高精度，性能开销大）
        //SamplerState.LinearWrap：线性过滤 + 纹理平铺（Wrap）
        private SamplerState samplerState;
        //深度模板测试
        //DepthStencilState.Default：启用深度测试（写深度），适合 3D 场景
        //DepthStencilState.None：禁用深度测试和写入，适合纯 2D 渲染
        //DepthStencilState.DepthRead：仅读取深度（不写入），用于透明物体
        private DepthStencilState depthStencilState;
        //控制光栅化方式
        //RasterizerState.CullNone：禁用背面剔除
        //RasterizerState.CullClockwise：剔除顺时针朝向的面
        //RasterizerState.CullCounterClockwise：剔除逆时针朝向的面
        private RasterizerState rasterizerState;

        private int numSprites;
        private int bufferOffset;

        private bool supportsNoOverwrite;

        private Matrix transformMatrix;

        private Effect customEffect;

        private static readonly byte[] spriteEffectCode = null;

        private static readonly short[] indexData = GenerateIndexArray();

        private static readonly TextureComparer TextureCompare = new TextureComparer();

        private static readonly BackToFrontComparer BackToFrontCompare = new BackToFrontComparer();

        private static readonly FrontToBackComparer FrontToBackCompare = new FrontToBackComparer();


        //初始化精灵批处理器
        public DBBCustomSpriteBatch(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            this.graphicsDevice = graphicsDevice;
            //初始化2048个四边形数据、纹理信息、精灵信息、排序的精灵信息
            vertexInfo = new VertexPositionColorTexture4[2048];
            textureInfo = new Texture2D[2048];
            spriteInfos = new SpriteInfo[2048];
            sortedSpriteInfos = new nint[2048];
            //顶点数据以及其索引仅由CPU写入GPU读取，这里弄了2048个四边形的顶点数据和索引数据
            vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), 8192, BufferUsage.WriteOnly);
            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, 12288, BufferUsage.WriteOnly);
            indexBuffer.SetData(indexData);
            spriteEffect = new Effect(graphicsDevice, spriteEffectCode);
            spriteMatrixTransform = spriteEffect.Parameters["MatrixTransform"].values;
            spriteEffectPass = spriteEffect.CurrentTechnique.Passes[0];
            beginCalled = false;
            numSprites = 0;
            supportsNoOverwrite = FNA3D.FNA3D_SupportsNoOverwrite(graphicsDevice.GLDevice) == 1;
        }
        //产生2048个四边形的索引数组
        private static short[] GenerateIndexArray()
        {
            short[] array = new short[12288];
            int num = 0;
            int num2 = 0;
            while (num < 12288)
            {
                array[num] = (short)num2;
                array[num + 1] = (short)(num2 + 1);
                array[num + 2] = (short)(num2 + 2);
                array[num + 3] = (short)(num2 + 3);
                array[num + 4] = (short)(num2 + 2);
                array[num + 5] = (short)(num2 + 1);
                num += 6;
                num2 += 4;
            }

            return array;
        }
        //如果是立即渲染模式，则立即切换GPU设备的渲染状态
        private unsafe void Immediate_Switch_RenderState()
        {
            graphicsDevice.BlendState = blendState;
            graphicsDevice.SamplerStates[0] = samplerState;
            graphicsDevice.DepthStencilState = depthStencilState;
            graphicsDevice.RasterizerState = rasterizerState;
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;
            Viewport viewport = graphicsDevice.Viewport;
            //水平缩放因子，用于将屏幕的X坐标归一化到[-1, 1]区间上
            float x_scale = (float)(2.0 / (double)viewport.Width);
            //垂直缩放因子，用于将屏幕的Y坐标归一化到[-1, 1]区间上
            //另外游戏内Y轴向下，现在让Y轴向上，所以有个负号
            float y_scale = (float)(-2.0 / (double)viewport.Height);
            //这个矩阵用于将世界坐标转换到屏幕坐标
            float* ptr = (float*)spriteMatrixTransform;
            *ptr = x_scale * transformMatrix.M11 - transformMatrix.M14;
            ptr[1] = x_scale * transformMatrix.M21 - transformMatrix.M24;
            ptr[2] = x_scale * transformMatrix.M31 - transformMatrix.M34;
            ptr[3] = x_scale * transformMatrix.M41 - transformMatrix.M44;
            ptr[4] = y_scale * transformMatrix.M12 + transformMatrix.M14;
            ptr[5] = y_scale * transformMatrix.M22 + transformMatrix.M24;
            ptr[6] = y_scale * transformMatrix.M32 + transformMatrix.M34;
            ptr[7] = y_scale * transformMatrix.M42 + transformMatrix.M44;
            ptr[8] = transformMatrix.M13;
            ptr[9] = transformMatrix.M23;
            ptr[10] = transformMatrix.M33;
            ptr[11] = transformMatrix.M43;
            ptr[12] = transformMatrix.M14;
            ptr[13] = transformMatrix.M24;
            ptr[14] = transformMatrix.M34;
            ptr[15] = transformMatrix.M44;
            //应用Shader的当前的Pass
            spriteEffectPass.Apply();
        }
        //开始渲染用
        public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformationMatrix)
        {
            try
            {
                if (beginCalled)
                {
                    Logger.Log(LogLevel.Error, "DBBHelper/DBBCustomSpriteBatch", "Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called.");
                    throw new InvalidOperationException();
                }
                beginCalled = true;
                this.sortMode = sortMode;//默认SpriteSortMode.Deferred，即按顺序渲染
                this.blendState = blendState ?? BlendState.AlphaBlend;//默认标准的Alpha混合
                this.samplerState = samplerState ?? SamplerState.PointClamp;//默认像素风
                this.depthStencilState = depthStencilState ?? DepthStencilState.None;//默认2D，禁用深度
                this.rasterizerState = rasterizerState ?? RasterizerState.CullNone;//默认2D，禁用裁切
                customEffect = effect;
                transformMatrix = transformationMatrix;
                //如果是立即渲染模式，则立即切换GPU设备的渲染状态
                if (sortMode == SpriteSortMode.Immediate)
                {
                    Immediate_Switch_RenderState();
                }
            }
            catch (Exception)
            {
                Logger.Log(LogLevel.Error, "DBBHelper/DBBCustomSpriteBatch", "Begin failed!");
            }
            
        }
        //产生顶点信息
        //sourceX, sourceY, sourceW, sourceH定义纹理上的哪一部分被绘制
        //destinationX, destinationY, destinationW, destinationH定义在屏幕上的位置和大小
        //originX, originY旋转和缩放的中心点（归一化坐标，(0,0) 是左上角，(1,1) 是右下角）
        //rotationSin, rotationCos精灵的旋转角度（通过正弦和余弦表示）
        //color, depth顶点的颜色和 Z 轴深度
        //effects用于翻转纹理（水平/垂直翻转）
        //最后产生的四个顶点信息，每个信息含有当前顶点的屏幕坐标、当前顶点的UV坐标，当前顶点的深度以及颜色
        private unsafe static void GenerateVertexInfo(VertexPositionColorTexture4* sprite, float sourceX, float sourceY, float sourceW, float sourceH, float destinationX, float destinationY, float destinationW, float destinationH, Color color, float originX, float originY, float rotationSin, float rotationCos, float depth, byte effects)
        {
            //计算四个点的屏幕坐标
            //(0,0)相对于中心点的旋转前的局部坐标
            float num = -originX * destinationW;
            float num2 = -originY * destinationH;
            //计算(0,0)旋转后的顶点坐标
            sprite->Position0.X = -rotationSin * num2 + rotationCos * num + destinationX;
            sprite->Position0.Y = rotationCos * num2 + rotationSin * num + destinationY;
            //其余三个点同理
            num = (1f - originX) * destinationW;
            num2 = -originY * destinationH;
            sprite->Position1.X = (0f - rotationSin) * num2 + rotationCos * num + destinationX;
            sprite->Position1.Y = rotationCos * num2 + rotationSin * num + destinationY;
            num = -originX * destinationW;
            num2 = (1f - originY) * destinationH;
            sprite->Position2.X = (0f - rotationSin) * num2 + rotationCos * num + destinationX;
            sprite->Position2.Y = rotationCos * num2 + rotationSin * num + destinationY;
            num = (1f - originX) * destinationW;
            num2 = (1f - originY) * destinationH;
            sprite->Position3.X = (0f - rotationSin) * num2 + rotationCos * num + destinationX;
            sprite->Position3.Y = rotationCos * num2 + rotationSin * num + destinationY;
            //计算四个点的纹理坐标
            fixed (float* ptr = &CornerOffsetX[0])
            {
                fixed (float* ptr2 = &CornerOffsetY[0])
                {
                    //计算旋转前的(0,0)处对应的纹理坐标值U和V
                    //做异或运算，这里是effects位掩码，通常 0=无翻转，1=水平翻转，2=垂直翻转，3=水平+垂直翻转
                    sprite->TextureCoordinate0.X = ptr[0 ^ effects] * sourceW + sourceX;
                    sprite->TextureCoordinate0.Y = ptr2[0 ^ effects] * sourceH + sourceY;
                    //计算其他三个点旋转前对应的纹理坐标值U和V
                    sprite->TextureCoordinate1.X = ptr[1 ^ effects] * sourceW + sourceX;
                    sprite->TextureCoordinate1.Y = ptr2[1 ^ effects] * sourceH + sourceY;
                    sprite->TextureCoordinate2.X = ptr[2 ^ effects] * sourceW + sourceX;
                    sprite->TextureCoordinate2.Y = ptr2[2 ^ effects] * sourceH + sourceY;
                    sprite->TextureCoordinate3.X = ptr[3 ^ effects] * sourceW + sourceX;
                    sprite->TextureCoordinate3.Y = ptr2[3 ^ effects] * sourceH + sourceY;
                }
            }
            //设置四个顶点的深度和颜色
            sprite->Position0.Z = depth;
            sprite->Position1.Z = depth;
            sprite->Position2.Z = depth;
            sprite->Position3.Z = depth;
            sprite->Color0 = color;
            sprite->Color1 = color;
            sprite->Color2 = color;
            sprite->Color3 = color;
        }

        //更新顶点缓冲，start为起始索引，count为需要更新的顶点数目，返回当前顶点批次写入缓冲后在顶点缓冲区的起始偏移索引，这个索引是精灵的索引
        private unsafe int UpdateVertexBuffer(int start, int count)
        {
            int batch_offset;//当前顶点批次写入缓冲后在顶点缓冲区的起始偏移，该索引是精灵的索引
            SetDataOptions options;//数据更新方式
            //当向缓冲区域再添加count时超过缓冲区配额时，或者不支持NoOverwrite时
            if (bufferOffset + count > 2048 || !supportsNoOverwrite)
            {
                //从缓冲区开头重新写入
                batch_offset = 0;
                //写入数据时直接抛弃掉旧数据
                options = SetDataOptions.Discard;
            }
            //当设置支持NoOverwrite时，同时缓冲区配额仍能容纳新加入的数据时
            else
            {
                //从缓冲区有数据的区域末端继续向后追加数据
                batch_offset = bufferOffset;
                //写入数据时保留未被覆盖的数据
                options = SetDataOptions.NoOverwrite;
            }
            //首先找到数据缓冲区末尾
            fixed (VertexPositionColorTexture4* data = &vertexInfo[start])
            {
                //一组顶点数据为96字节，包含4个顶点数据，每个顶点数据有一个Vector3位置(12byte)、Color(4byte)、Vector2纹理坐标(8byte)
                //在数据缓冲区末尾num * 96字节处按照options的方式从data中写入count * 96字节的数据
                vertexBuffer.SetDataPointerEXT(batch_offset * 96, (nint)data, count * 96, options);
            }
            //更新当前数据缓冲区末尾
            bufferOffset = batch_offset + count;
            return batch_offset;
        }
        //将一批精灵绘制到屏幕上
        //texture为当前批次精灵使用的纹理
        //baseSprite为当前批次在顶点缓冲区的起始精灵索引
        //batchSize为当前批次的精灵数目
        private void DrawPrimitives(Texture texture, int baseSprite, int batchSize)
        {
            //如果有自定义特效
            if (customEffect != null)
            {
                foreach (EffectPass pass in customEffect.CurrentTechnique.Passes)
                {
                    //应用特效
                    pass.Apply();
                    //绑定纹理到0号槽位
                    graphicsDevice.Textures[0] = texture;

                    graphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,// 绘制片元是三角形
                        baseSprite * 4,// 起始顶点偏移，注意一个精灵有四个顶点
                        0,// 最小顶点索引，代表从顶点缓冲区的的哪里开始读取顶点
                        batchSize * 4,// 要读取的顶点总数，注意一个精灵有四个顶点
                        0,// 起始索引，代表从索引缓冲区的哪里开始读取索引
                        batchSize * 2// 要绘制的片元三角形数量，注意一个精灵有两个三角形，这里反映了一次读取6个索引
                    );
                }

                return;
            }
            //否则进行默认绘制
            graphicsDevice.Textures[0] = texture;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite * 4, 0, batchSize * 4, 0, batchSize * 2);
        }

        //按批次绘制精灵
        private unsafe void FlushBatch()
        {
            Immediate_Switch_RenderState();
            //如果精灵数目为0，则直接不渲染
            if (numSprites == 0)
            {
                return;
            }
            //填充顶点数据，包含顶点的屏幕坐标、UV坐标、深度、颜色信息
            //当不是Deferred模式时
            if (sortMode != SpriteSortMode.Deferred)
            {
                //根据模式选择相应的比较器用于精灵排序
                IComparer<nint> comparer = (sortMode == SpriteSortMode.Texture) ? TextureCompare : ((sortMode != SpriteSortMode.BackToFront) ? ((IComparer<nint>)FrontToBackCompare) : ((IComparer<nint>)BackToFrontCompare));
                //使用fixed固定数组指针，避免GC移动内存
                //这里spriteInfos_ptr用于辅助sortedSpriteInfos_ptr进行精灵的排序
                fixed (SpriteInfo* spriteInfos_ptr = &spriteInfos[0])
                {
                    fixed (nint* sortedSpriteInfos_ptr = &sortedSpriteInfos[0])
                    {
                        //这里vertexInfo_ptr用于填充顶点信息，当精灵排好序后按顺序向vertexInfo中填充精灵的顶点信息
                        fixed (VertexPositionColorTexture4* vertexInfo_ptr = &vertexInfo[0])
                        {
                            //初始化sortedSpriteInfos数组为spriteInfos数组
                            for (int i = 0; i < numSprites; i++)
                            {
                                sortedSpriteInfos_ptr[i] = (nint)(spriteInfos_ptr + i);
                            }
                            //基于给定的比较器对(sortedSpriteInfos，textureInfo)序对进行排序，textureInfo跟着sortedSpriteInfos一起排序
                            Array.Sort(sortedSpriteInfos, textureInfo, 0, numSprites, comparer);
                            //对每个精灵生成一组四个顶点数据
                            for (int j = 0; j < numSprites; j++)
                            {
                                SpriteInfo* ptr4 = (SpriteInfo*)sortedSpriteInfos_ptr[j];
                                GenerateVertexInfo(
                                    vertexInfo_ptr + j,
                                    ptr4->sourceX,
                                    ptr4->sourceY,
                                    ptr4->sourceW,
                                    ptr4->sourceH,
                                    ptr4->destinationX,
                                    ptr4->destinationY,
                                    ptr4->destinationW,
                                    ptr4->destinationH,
                                    ptr4->color,
                                    ptr4->originX,
                                    ptr4->originY,
                                    ptr4->rotationSin,
                                    ptr4->rotationCos,
                                    ptr4->depth,
                                    ptr4->effects
                                );
                            }
                        }
                    }
                }
            }

            //提交一批次的精灵的顶点数据进行绘制
            //batch_start_index为当前批次顶点数据在vertexInfo中的起始索引
            int batch_start_index = 0;
            //一个批次的精灵的顶点数据又根据精灵的纹理是否相同划分为了若干小批次进行绘制
            while (true)
            {
                //计算当前批次的精灵数量，一个批次最多2048个精灵
                int batchSize = Math.Min(numSprites, 2048);
                //更新顶点缓冲，batch_start_offset为这一批次顶点在GPU顶点缓冲区域中的起始偏移索引，是精灵的索引
                int batch_start_offset = UpdateVertexBuffer(batch_start_index, batchSize);
                //这一个小批次的精灵的起始索引值
                int current_batch_start_index = 0;
                //按照纹理分批绘制，纹理相同的一批精灵在一起绘制
                //获取当前纹理
                Texture2D current_texture = textureInfo[batch_start_index];
                for (int k = 1; k < batchSize; k++)
                {
                    //获取下一个纹理
                    Texture2D next_texture = textureInfo[batch_start_index + k];
                    //当两个纹理第一次不同时，可以对一小批纹理相同的精灵进行绘制了
                    if (next_texture != current_texture)
                    {
                        DrawPrimitives(current_texture, batch_start_offset + current_batch_start_index, k - current_batch_start_index);
                        //重新设置current_texture和current_batch_start_index来进行下一批次精灵的检测
                        current_texture = next_texture;
                        current_batch_start_index = k;
                    }
                }
                //绘制最后一个小批次的所有精灵
                DrawPrimitives(current_texture, batch_start_offset + current_batch_start_index, batchSize - current_batch_start_index);
                //如果已经处理完了所有精灵，退出循环
                //这里的判断条件是因为逻辑上是先处理一批次精灵，再检测是否还有剩余精灵
                if (numSprites <= 2048)
                {
                    break;
                }
                //如果numSprites大于2048，证明还有其他批次的精灵需要处理，为此处理下一个批次的精灵的顶点数据
                numSprites -= 2048;
                batch_start_index += 2048;
            }
            numSprites = 0;
        }

        private void CheckBegin(string method)
        {
            if (!beginCalled)
            {
                throw new InvalidOperationException(method + " was called, but Begin has not yet been called. Begin must be called successfully before you can call " + method + ".");
            }
        }

        //绘制精灵
        private unsafe void PushSprite(Texture2D texture, float sourceX, float sourceY, float sourceW, float sourceH, float destinationX, float destinationY, float destinationW, float destinationH, Color color, float originX, float originY, float rotationSin, float rotationCos, float depth, byte effects)
        {
            //如果要绘制的精灵数目过多
            if (numSprites >= vertexInfo.Length)
            {
                //如果vertexInfo达到最大容量配置，则强制提交当前批次的绘制
                if (vertexInfo.Length >= 699050)
                {
                    FlushBatch();
                }
                //否则，动态扩大vertexInfo的容量，容量翻倍
                else
                {
                    int newSize = Math.Min(vertexInfo.Length * 2, 699050);
                    Array.Resize(ref vertexInfo, newSize);
                    Array.Resize(ref textureInfo, newSize);
                    Array.Resize(ref spriteInfos, newSize);
                    Array.Resize(ref sortedSpriteInfos, newSize);
                }
            }
            //如果渲染模式为立即渲染
            if (sortMode == SpriteSortMode.Immediate)
            {
                //则直接根据所提供的参数生成顶点数据并上传到GPU
                int baseSprite;
                fixed (VertexPositionColorTexture4* ptr = &vertexInfo[0])
                {
                    GenerateVertexInfo(ptr, sourceX, sourceY, sourceW, sourceH, destinationX, destinationY, destinationW, destinationH, color, originX, originY, rotationSin, rotationCos, depth, effects);
                    if (supportsNoOverwrite)
                    {
                        baseSprite = UpdateVertexBuffer(0, 1);
                    }
                    else
                    {
                        baseSprite = 0;
                        vertexBuffer.SetDataPointerEXT(0, (nint)ptr, 96, SetDataOptions.None);
                    }
                }
                DrawPrimitives(texture, baseSprite, 1);
                return;
            }
            //如果是延迟渲染
            if (sortMode == SpriteSortMode.Deferred)
            {
                //则将该批次的精灵存入缓冲区等待后续渲染
                fixed (VertexPositionColorTexture4* sprite = &vertexInfo[numSprites])
                {
                    GenerateVertexInfo(sprite, sourceX, sourceY, sourceW, sourceH, destinationX, destinationY, destinationW, destinationH, color, originX, originY, rotationSin, rotationCos, depth, effects);
                }

                textureInfo[numSprites] = texture;
                numSprites++;
                return;
            }
            //对于其他渲染模式，存储精灵的元数据，不生成顶点数据，留待 FlushBatch 时排序后处理。
            fixed (SpriteInfo* ptr2 = &spriteInfos[numSprites])
            {
                ptr2->textureHash = texture.GetHashCode();
                ptr2->sourceX = sourceX;
                ptr2->sourceY = sourceY;
                ptr2->sourceW = sourceW;
                ptr2->sourceH = sourceH;
                ptr2->destinationX = destinationX;
                ptr2->destinationY = destinationY;
                ptr2->destinationW = destinationW;
                ptr2->destinationH = destinationH;
                ptr2->color = color;
                ptr2->originX = originX;
                ptr2->originY = originY;
                ptr2->rotationSin = rotationSin;
                ptr2->rotationCos = rotationCos;
                ptr2->depth = depth;
                ptr2->effects = effects;
            }
            textureInfo[numSprites] = texture;
            numSprites++;
        }
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
            //检查是否已经启用Begin
            CheckBegin("Draw");
            float sourceX;//源矩形归一化后的U坐标
            float sourceY;//源矩形归一化后的V坐标
            float source_rectangle_proportion_x;//源矩形归一化后的宽度比例
            float source_rectangle_proportion_y;//源矩形归一化后的高度比例
            //如果指定了源矩形，则将转化为的归一化的UV坐标
            if (sourceRectangle.HasValue)
            {
                sourceX = (float)sourceRectangle.Value.X / (float)texture.Width;
                sourceY = (float)sourceRectangle.Value.Y / (float)texture.Height;
                //宽度比例（带符号）
                // (float)Math.Sign(sourceRectangle.Value.Width)是取符号位
                // Math.Max(Math.Abs(sourceRectangle.Value.Width), MathHelper.MachineEpsilonFloat)是为了防止宽度比例为0
                source_rectangle_proportion_x = (float)Math.Sign(sourceRectangle.Value.Width) * Math.Max(Math.Abs(sourceRectangle.Value.Width), DBBMath.MachineEpsilonFloat) / (float)texture.Width;
                //高度比例（带符号）
                source_rectangle_proportion_y = (float)Math.Sign(sourceRectangle.Value.Height) * Math.Max(Math.Abs(sourceRectangle.Value.Height), DBBMath.MachineEpsilonFloat) / (float)texture.Height;
            }
            //否则使用默认UV
            else
            {
                sourceX = 0f;
                sourceY = 0f;
                source_rectangle_proportion_x = 1f;
                source_rectangle_proportion_y = 1f;
            }
            PushSprite(
                texture,
                sourceX,
                sourceY,
                source_rectangle_proportion_x,
                source_rectangle_proportion_y,
                destinationRectangle.X,
                destinationRectangle.Y,
                destinationRectangle.Width,
                destinationRectangle.Height,
                color,
                origin.X / source_rectangle_proportion_x / texture.Width,
                origin.Y / source_rectangle_proportion_y / texture.Height,
                (float)Math.Sin(rotation),
                (float)Math.Cos(rotation),
                layerDepth,
                (byte)(effects & (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically))
                );
        }
        //结束渲染用
        public void End()
        {
            try
            {
                if (!beginCalled)
                {
                    Logger.Log(LogLevel.Error, "DBBHelper/DBBCustomSpriteBatch", "End was called, but Begin has not yet been called. You must call Begin successfully before you can call End.");
                    throw new InvalidOperationException();
                }
                beginCalled = false;
                if (sortMode != SpriteSortMode.Immediate)
                {
                    FlushBatch();
                }
                customEffect = null;
            }
            catch (Exception)
            {
                Logger.Log(LogLevel.Error, "DBBHelper/DBBCustomSpriteBatch", "End failed!");
            }
        }
    
    }
}
*/