using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [DBBCustomEntity(3, true)]
    [Tracked]
    public class DBBGeneralHDpostProcessing : Entity
    {
        //这里需要一个更精确的矩形
        public struct RectangleF
        {
            public float X = 0.0f;
            public float Y = 0.0f;
            public float Width = 0.0f;
            public float Height = 0.0f;
            public RectangleF(float x, float y, float width, float height)
            {
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
            }
            public RectangleF(Vector2 left_top, Vector2 width_height)
            {
                this.X = left_top.X;
                this.Y = left_top.Y;
                this.Width = width_height.X;
                this.Height = width_height.Y;
            }
            public void Log(string tag, LogLevel level = LogLevel.Warn)
            {
                Logger.Log(level, tag, "X: " + X.ToString() + " Y: " + Y.ToString() + " Width: " + Width.ToString() + " Height:" + Height.ToString());
            }
        }
        //参考分辨率
        public static Vector2 ref_resolution_vector = new Vector2(1922.0f, 1082.0f);

        //双缓冲区，前端和后端缓冲
        public static RenderTarget2D ContentBuffer = null;//后端
        public static RenderTarget2D CanvasBuffer = null;//前端
        private static bool currentTargetIs1 = true;

        public static Action Draw_Mask = () => { };//提供一个事件用于绘制掩码用
        public static Action Draw_Something_On_HDPostProcessing = () => { };//提供一个事件用于在HDPostProcessing缓冲上绘制什么东西
        public static Action Adjust_SomeBuffers_Before_HDPostProcessing_Render = () => { };//提供一个事件用于调整一些缓冲区的大小

        protected Texture2D shader_mask_texture;//控制shader的掩码纹理
        protected Rectangle area = new Rectangle();//特效所在的区域
        public RectangleF local_clip_area = new RectangleF();//局部的原始裁切区域
        public Rectangle transformed_global_clip_area = new Rectangle();//全局变换后的裁切区域的全局坐标以及其宽高
        public DBBGeneralHDpostProcessing(EntityData data, Vector2 offset)
        {
            //处理特效的区域
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);
            //处理特效的掩码图
            string mask_texture_name = data.Attr("MaskTexture");
            MTexture mask_texture = GFX.Game[mask_texture_name];
            //没找到用户指定的纹理的情况下用默认的全通掩码图
            if (mask_texture == null)
            {
                shader_mask_texture = GFX.Game["objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask"].Texture.Texture;
            }
            else
            {
                shader_mask_texture = mask_texture.Texture.Texture;
            }

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            DBBCustomEntityManager.Added_As_BaseType(this, typeof(DBBGeneralHDpostProcessing));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            DBBCustomEntityManager.Removed_As_BaseType(this, typeof(DBBGeneralHDpostProcessing));
        }
        //获取局部的原始裁切区域
        private RectangleF Get_Original_Local_ClipArea()
        {
            //将实体坐标转换为纹理空间中的局部坐标
            Vector2 local_pos = Vector2.Transform(Position, GameplayRenderer.instance.Camera.Matrix);
            //在这里可以居于原矩形的宽高提供一些中心缩放
            Vector2 center = local_pos + 0.5f * new Vector2(area.Width, area.Height);
            Vector2 width_height_scaled = new Vector2(area.Width, area.Height);
            RectangleF local_Rect = new RectangleF(center - width_height_scaled * 0.5f, width_height_scaled);
            return local_Rect;
        }
        public RectangleF Get_Original_Local_ClipArea(RectangleF origin, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            //将实体坐标转换为纹理空间中的局部坐标
            Vector2 local_pos = Vector2.Transform(new Vector2(origin.X, origin.Y), GameplayRenderer.instance.Camera.Matrix);
            //在这里可以居于原矩形的宽高提供一些中心缩放
            Vector2 center = local_pos + 0.5f * new Vector2(area.Width, area.Height);
            Vector2 width_height_scaled = new Vector2(area.Width * scaleX, area.Height * scaleY);
            RectangleF local_Rect = new RectangleF(center - width_height_scaled * 0.5f, width_height_scaled);
            return local_Rect;
        }
        //获取局部的变换后的裁切区域的全局坐标以及其宽高
        public RectangleF Get_Original_Global_ClipArea(RectangleF origin)
        {
            var tmp_level = Scene as Level;
            Vector2 vector = new Vector2(320f, 180f);
            Vector2 vector2 = vector / tmp_level.ZoomTarget;

            Vector2 vector3 = (tmp_level.ZoomTarget != 1f) ? ((tmp_level.ZoomFocusPoint - vector2 / 2f) / (vector - vector2) * vector) : Vector2.Zero;
            Vector2 vector4 = new Vector2(tmp_level.ScreenPadding, tmp_level.ScreenPadding * 0.5625f);


            float scale = tmp_level.Zoom * ((320f - tmp_level.ScreenPadding * 2f) / 320f);
            //获取局部变换后的矩形的左上角位置和宽高
            Vector2 rect_left_top = new Vector2(origin.X, origin.Y);
            Vector2 rect_width_height = new Vector2(origin.Width, origin.Height);
            //rect_width_height后续将会被缩放，所以需要再次记录下矩形的原始的宽高
            Vector2 origin_width_height = rect_width_height;

            rect_width_height = rect_width_height * scale;
            //这里的缩放和镜像翻转是这样的逻辑：
            //正常情况下是将实体的Position以(0,0)为中心进行缩放，最后添加屏幕黑边的偏移
            //翻转的情况下首先需要将实体的Position改为以160为轴线的轴对称位置得到P，然后将P以(320,0)为中心进行缩放，最后添加屏幕黑边的偏移

            //当没有镜像翻转时，确定左上角顶点的相对于局部中心聚焦点vector3(一直为0)的位置
            rect_left_top = rect_left_top - vector3;
            //如果有翻转
            if (SaveData.Instance.Assists.MirrorMode)
            {

                vector4.X = -vector4.X;
                //首先修改聚焦点的位置到对称的位置处
                //先确定实体中心
                Vector2 tmp_center = rect_left_top + origin_width_height * 0.5f;
                //翻转实体中心到对应位置
                tmp_center.X = 320.0f - tmp_center.X;
                //缩放实体到指定位置
                tmp_center = new Vector2(320.0f, 0.0f) + (tmp_center - new Vector2(320.0f, 0.0f)) * scale;
                //确定实体左上角位置
                rect_left_top = tmp_center - rect_width_height * 0.5f;
            }
            //局部坐标转换到全局坐标
            else
            {
                rect_left_top = rect_left_top * scale;
            }
            //添加偏移
            rect_left_top += vector4;
            //为保证误差，这里必须用浮点数
            return new RectangleF(rect_left_top.X, rect_left_top.Y, rect_width_height.X, rect_width_height.Y);
        }
        //获取全局变换后的裁切区域的全局坐标以及其宽高
        public Rectangle Get_Trasformed_Global_ClipArea(RectangleF origin)
        {
            //Engine.ScreenMatrix只是一个缩放矩阵
            //Matrix matrix=Matrix.CreateScale(6f)*Engine.ScreenMatrix;
            float scale = 6.0f * Engine.ScreenMatrix.M11;
            Vector2 rect_left_top = new Vector2(origin.X, origin.Y);
            Vector2 rect_width_height = new Vector2(origin.Width, origin.Height);
            rect_left_top = rect_left_top * scale;
            rect_width_height = rect_width_height * scale;
            return new Rectangle((int)rect_left_top.X, (int)rect_left_top.Y, (int)rect_width_height.X, (int)rect_width_height.Y);
        }
        //更新裁剪区域
        public void UpdateClipArea()
        {
            //1.依据源矩阵从纹理中进行裁切
            //2.设置纹理空间中的局部中心坐标origin
            //3.依据设置的rotation,scale对裁切的纹理进行局部变换
            //4.将局部中心坐标origin定位到全局坐标position，裁切部分的各像素的坐标顺势移动到全局坐标中
            //5.对全局坐标系下的所有坐标进行matrix变换，所得到的坐标就是最终的屏幕坐标
            local_clip_area = Get_Original_Local_ClipArea();
            RectangleF original_global_clip_area = Get_Original_Global_ClipArea(local_clip_area);
            transformed_global_clip_area = Get_Trasformed_Global_ClipArea(original_global_clip_area);
        }
        //这里提供一个可以自行使用的函数，支持矩形的缩放
        public Rectangle UpdateClipArea(RectangleF custom_rectangleF, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            local_clip_area = Get_Original_Local_ClipArea(custom_rectangleF, scaleX, scaleY);
            RectangleF original_global_clip_area = Get_Original_Global_ClipArea(local_clip_area);
            transformed_global_clip_area = Get_Trasformed_Global_ClipArea(original_global_clip_area);
            return transformed_global_clip_area;
        }
        //将局部掩码绘制到全局掩码上
        public void GlobalMaskRender()
        {
            UpdateClipArea();
            Draw.SpriteBatch.Draw(shader_mask_texture, transformed_global_clip_area, shader_mask_texture.Bounds, Color.White, 0f, Vector2.Zero, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        //设置特效参数用
        public virtual void SetAllParameter()
        {

        }
        public override void Update()
        {
            base.Update();
        }
        public override void Render()
        {
            base.Render();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
        }

        public static void Load()
        {
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create += Init_Buffers;
            //首先切换到HDPostProcessing，为在HDPostProcessing绘制特效做准备
            Draw_Something_On_HDPostProcessing += Set_HDPostProcessing_Buffer;

            //先故障后模糊
            LoadHDPostprocessing();

            //需要时调整缓冲区的大小
            DBBEffectSourceManager.Adjust_SomeBuffers_Before_Render += Adjust_Some_Buffers_WH;
            //在Draw_Something_On_TmpGameContent事件末尾挂载上两个事件来获得HDPostProcessing的内容
            DBBEffectSourceManager.Draw_Something_On_TmpGameContent += Draw_Mask;
            DBBEffectSourceManager.Draw_Something_On_TmpGameContent += Draw_Something_On_HDPostProcessing;

            //最终将HDPostProcessing缓冲的内容绘制到默认屏幕上
            DBBEffectSourceManager.Redraw_Something_On_DefaultBuffer += Draw_HDPostProcessing_On_Default;
        }
        public static void UnLoad()
        {

            DBBEffectSourceManager.Redraw_Something_On_DefaultBuffer -= Draw_HDPostProcessing_On_Default;

            DBBEffectSourceManager.Draw_Something_On_TmpGameContent -= Draw_Something_On_HDPostProcessing;
            DBBEffectSourceManager.Draw_Something_On_TmpGameContent -= Draw_Mask;
            DBBEffectSourceManager.Adjust_SomeBuffers_Before_Render -= Adjust_Some_Buffers_WH;

            UnLoadHDPostprocessing();

            Draw_Something_On_HDPostProcessing -= Set_HDPostProcessing_Buffer;
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create -= Init_Buffers;
        }
        public static void LoadHDPostprocessing()
        {
            //FBMLiquid.LiquidLoad();
            DBBGeneralGlitch.GlitchLoad();
            DBBGeneralBlur.BlurLoad();
        }
        public static void UnLoadHDPostprocessing()
        {
            DBBGeneralBlur.BlurUnLoad();
            DBBGeneralGlitch.GlitchUnLoad();
            //FBMLiquid.LiquidUnLoad();
        }
        //初始化缓冲区
        private static void Init_Buffers()
        {
            DBBGamePlayBuffers.Create("HDPostProcessing1", 1922, 1082);//用于HD后处理
            DBBGamePlayBuffers.Create("HDPostProcessing2", 1922, 1082);//用于HD后处理
            ref_resolution_vector = new Vector2(1922.0f, 1082.0f);
            currentTargetIs1 = true;
        }
        //交换缓冲内容，用于双缓冲区
        public static void Swap_Buffer()
        {
            // 获取后端和前端缓冲
            ContentBuffer = currentTargetIs1 ? DBBGamePlayBuffers.DBBRenderTargets["HDPostProcessing1"] : DBBGamePlayBuffers.DBBRenderTargets["HDPostProcessing2"];
            CanvasBuffer = currentTargetIs1 ? DBBGamePlayBuffers.DBBRenderTargets["HDPostProcessing2"] : DBBGamePlayBuffers.DBBRenderTargets["HDPostProcessing1"];
            // 设置渲染目标
            Engine.Instance.GraphicsDevice.SetRenderTarget(CanvasBuffer);
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            // 切换目标
            currentTargetIs1 = !currentTargetIs1;
        }

        //调整缓冲区的大小来和屏幕适配
        private static void Adjust_Some_Buffers_WH()
        {
            //如果当前分辨率与上一次的分辨率不同，则调整
            if (ref_resolution_vector != DBBEffectSourceManager.ref_resolution_vector)
            {
                ref_resolution_vector = DBBEffectSourceManager.ref_resolution_vector;
                DBBGamePlayBuffers.ReloadBuffer("HDPostProcessing1", ref_resolution_vector);//调整缓冲区的大小
                DBBGamePlayBuffers.ReloadBuffer("HDPostProcessing2", ref_resolution_vector);//调整缓冲区的大小
            }
            Adjust_SomeBuffers_Before_HDPostProcessing_Render();
        }
        //初始化HDPostProcessing双缓冲区的内容
        private static void Set_HDPostProcessing_Buffer()
        {
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["HDPostProcessing2"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["TmpGameContent"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["HDPostProcessing1"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["TmpGameContent"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

        //将当前前端缓冲区绘制到默认屏幕上
        private static void Draw_HDPostProcessing_On_Default()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(CanvasBuffer, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

    }
}