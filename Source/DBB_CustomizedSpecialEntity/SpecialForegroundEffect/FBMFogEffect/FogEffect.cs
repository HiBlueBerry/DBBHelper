using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using FMOD;
using Celeste.Mod.DBBHelper.Mechanism;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/FogEffect")]
    [DBBCustomEntity(4,true)]
    //高清流体效果
    public class FogEffect : Entity
    {
        //用于绘制雾效的掩码
        public static BlendState DBBFogMaskAlphaBlend = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaBlendFunction = BlendFunction.Add
        };
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
        //特效的通用设置
        private Rectangle area = new Rectangle();//特效所在的区域
        private MTexture m_texture = new MTexture();
        private Texture2D shader_mask_texture;//控制shader的掩码纹理
        public RectangleF local_clip_area = new RectangleF();//局部的原始裁切区域
        public Rectangle transformed_global_clip_area = new Rectangle();//全局变换后的裁切区域的全局坐标以及其宽高
        //------------------与掩码区域的居中缩放有关------------------
        public float scaleX = 1.0f;//掩码图像的X缩放
        public float scaleY = 1.0f;//掩码图像的Y缩放
        public string label = "Default";//特效的标签，用于FogEffectControl控制特效
        //------------------与特效的性质相关------------------
        public float velocity_x = 0.1f;//流体X流速
        public float velocity_y = 0.0f;//流体Y流速
        public float amplify = 0.5f;//流体强度
        public float frequency = 2.0f;//流体细节度
        public float light_influence_coefficient = 1.0f;//光照影响系数，用于控制光照对流体的影响强度
        private int num_level = 6;//分形次数，性能损耗项,过大的数值会导致屏幕卡顿，不建议随意改动
        private bool frac_mode = false;//余数模式，采用此模式在流体强度较大时会产生分割效果

        //以下为构建流体的四个颜色，其中颜色1和颜色2进行一次插值得到C1，颜色3和颜色4进行一次插值得到C2，最终C1和C2插值得到最终颜色
        //当流体强度接近1时整体会趋近颜色4，当流体强度接近0时整体会趋近颜色1，为得到最佳颜色效果，建议流体强度保持0.5
        private Vector2 time = Vector2.Zero;//时间参数，应当不断变化以产生变化的效果

        //------------------与特效的颜色相关------------------

        //四个颜色，用于控制分形的颜色，其中color1和color2插值得到final1，color3和color4插值得到final2，最后final1和final2插值得到最终颜色值
        private Vector4 color1;//颜色1
        private Vector4 color2;//颜色2
        private Vector4 color3;//颜色3
        private Vector4 color4;//颜色4
        private Vector4 tint;

        public FogEffect(EntityData data, Vector2 offset)
        {
            //获取Loenn传来的参数
            m_texture = GFX.Game[data.Attr("MaskTexture")];
            if (m_texture == null)
            {
                m_texture = GFX.Game["objects/DBB_Items/DBBGeneralEffectTexture/AllPassMask"];
            }
            shader_mask_texture = m_texture.Texture.Texture;

            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);
            label = data.Attr("Label");
            scaleX = data.Float("ScaleX",1.0f);
            scaleY = data.Float("ScaleY",1.0f);
            velocity_x = data.Float("VelocityX");
            velocity_y = data.Float("VelocityY");
            amplify = data.Float("Amplify");
            frequency = data.Float("Frequency");
            num_level = data.Int("NumLevel");
            light_influence_coefficient = data.Float("LightInfluenceCoefficient");
            frac_mode = data.Bool("FracMode");
            //获取Loenn传来的颜色
            var tmp_color = Calc.HexToColor(data.Attr("Color1"));
            color1 = new Vector4(tmp_color.R / 255.0f, tmp_color.G / 255.0f, tmp_color.B / 255.0f, data.Float("Alpha1"));
            tmp_color = Calc.HexToColor(data.Attr("Color2"));
            color2 = new Vector4(tmp_color.R / 255.0f, tmp_color.G / 255.0f, tmp_color.B / 255.0f, data.Float("Alpha2"));
            tmp_color = Calc.HexToColor(data.Attr("Color3"));
            color3 = new Vector4(tmp_color.R / 255.0f, tmp_color.G / 255.0f, tmp_color.B / 255.0f, data.Float("Alpha3"));
            tmp_color = Calc.HexToColor(data.Attr("Color4"));
            color4 = new Vector4(tmp_color.R / 255.0f, tmp_color.G / 255.0f, tmp_color.B / 255.0f, data.Float("Alpha4"));

            tmp_color = Calc.HexToColor(data.Attr("Tint"));
            tint = new Vector4(tmp_color.R / 255.0f, tmp_color.G / 255.0f, tmp_color.B / 255.0f, 1.0f);

            //特效启用
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            //向DBBCustomEntityManager添加该实体
            DBBCustomEntityManager.Added(this);
            TransitionListener handle_active = new TransitionListener();
            handle_active.OnInBegin = delegate ()
            {
                Active = true;
            };
            handle_active.OnOutBegin = delegate ()
            {
                Active = false;
            };
            Add(handle_active);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            //从DBBCustomEntityManager删除该实体
            DBBCustomEntityManager.Removed(this);
        }
        //这里仅更新时间参数
        public override void Update()
        {
            base.Update();
            time.X += Engine.DeltaTime * velocity_x;
            time.Y += Engine.DeltaTime * velocity_y;
        }

        //获取给定点在相机空间中的局部坐标，倘若要绘制的缓冲区的坐标定义就是在相机空间中，那么这个局部坐标就是点绘制在缓冲区中的位置
        //例如，倘若矩形(依次为XY坐标、长宽)(320,-248,120,72)经变换后得到(依次为XY坐标，长宽)(0,-22,120,72)，缓冲区为320*180大小，则就是在这个320*180大小的缓冲区内(左上角(0,0))绘制这个矩形(0,-22,120,72)
        //此时矩形超出缓冲区的部分会被裁切
        //为shader设置参数
        public void SetAllParameter()
        {
            //将全局掩码贴图送入对应参数供后续使用
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["HDFogMask"]);

            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["amp"].SetValue(amplify);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["fre"].SetValue(frequency);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["light_influence_coefficient"].SetValue(light_influence_coefficient);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["num_level"].SetValue(num_level);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["frac_mode"].SetValue(frac_mode);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["color1"].SetValue(color1);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["color2"].SetValue(color2);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["color3"].SetValue(color3);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["color4"].SetValue(color4);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["tint"].SetValue(tint);
            //是否水平翻转采样
            Vector2 tmp_time = SaveData.Instance.Assists.MirrorMode ? new Vector2(-time.X, time.Y) : time;
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["time"].SetValue(tmp_time);
        }
        //更新裁剪区域
        private void UpdateClipArea()
        {
            //1.依据源矩阵从纹理中进行裁切
            //2.设置纹理空间中的局部中心坐标origin
            //3.依据设置的rotation,scale对裁切的纹理进行局部变换
            //4.将局部中心坐标origin定位到全局坐标position，裁切部分的各像素的坐标顺势移动到全局坐标中
            //5.对全局坐标系下的所有坐标进行matrix变换，所得到的坐标就是最终的屏幕坐标
            //额外注意：前两次求矩形必须要注意误差
            local_clip_area = Get_Original_Local_ClipArea();
            RectangleF original_global_clip_area = Get_Original_Global_ClipArea(local_clip_area);
            transformed_global_clip_area = Get_Trasformed_Global_ClipArea(original_global_clip_area);
        }
        //获取局部的原始裁切区域
        private RectangleF Get_Original_Local_ClipArea()
        {
            //将实体坐标转换为纹理空间中的局部坐标
            Vector2 local_pos = Vector2.Transform(Position, GameplayRenderer.instance.Camera.Matrix);
            //在这里可以居于原矩形的宽高提供一些中心缩放
            Vector2 center = local_pos + 0.5f*new Vector2(area.Width,area.Height);
            Vector2 width_height_scaled = new Vector2(area.Width * scaleX, area.Height * scaleY);
            RectangleF local_Rect = new RectangleF(center-width_height_scaled*0.5f,width_height_scaled);
            

            return local_Rect;
        }
        //获取局部的变换后的裁切区域的全局坐标以及其宽高
        private RectangleF Get_Original_Global_ClipArea(RectangleF origin)
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
        private Rectangle Get_Trasformed_Global_ClipArea(RectangleF origin)
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

        //将局部掩码绘制到全局掩码上
        public void FogGlobalMaskRender()
        {
            UpdateClipArea();
            Draw.SpriteBatch.Draw(shader_mask_texture, transformed_global_clip_area, shader_mask_texture.Bounds, Color.White, 0f, Vector2.Zero,SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        //绘制雾效
        public void FogRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"], Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture"], transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
        //这里不使用原版的Render
        public override void Render()
        {
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Position, area.Width, area.Height, Color.SkyBlue);
        }


        public static void Load()
        {
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create += Init_Buffer;
            //需要时调整缓冲区的大小
            DBBEffectSourceManager.Adjust_SomeBuffers_Before_Render += Adjust_Some_Buffers_WH;
            DBBEffectSourceManager.Draw_Something_On_TmpGameContent += Draw_HDFog;
            DBBEffectSourceManager.Redraw_Something_On_DefaultBuffer += Draw_HDFog_On_Default;
        }
        public static void UnLoad()
        {
            DBBEffectSourceManager.Draw_Something_On_TmpGameContent -= Draw_HDFog;
            DBBEffectSourceManager.Redraw_Something_On_DefaultBuffer -= Draw_HDFog_On_Default;
            DBBEffectSourceManager.Adjust_SomeBuffers_Before_Render -= Adjust_Some_Buffers_WH;
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create -= Init_Buffer;
        }
        private static void Init_Buffer()
        {
            DBBGamePlayBuffers.Create("HDFog", 1922, 1082);//渲染雾效
            DBBGamePlayBuffers.Create("HDFogMask", 1922, 1082);//雾效的掩码区域
            DBBGamePlayBuffers.Create("LightMask", 1922, 1082);//光照贴图
            ref_resolution_vector = new Vector2(1922, 1082);
        }
        //调整自定义缓冲区的大小来与原版对应的缓冲区对齐，主要为了与扩展镜头兼容
        private static void Adjust_Some_Buffers_WH()
        {

            //如果当前分辨率与上一次的分辨率相同，则不用调整
            if (ref_resolution_vector == DBBEffectSourceManager.ref_resolution_vector)
            {
                return;
            }
            //否则调整缓冲大小
            ref_resolution_vector = DBBEffectSourceManager.ref_resolution_vector;
            DBBGamePlayBuffers.ReloadBuffer("HDFog", ref_resolution_vector);//调整雾效的大小
            DBBGamePlayBuffers.ReloadBuffer("HDFogMask", ref_resolution_vector);//调整掩码区域的大小
            DBBGamePlayBuffers.ReloadBuffer("LightMask", ref_resolution_vector);//调整光照贴图的大小

        }
        //更新光照贴图
        private static void UpdateLightMask()
        {
            //确定目前是否在游戏场景中
            Scene scene = Engine.Scene;
            Level tmp_level =(scene is Level)?(Level)scene : null;
            if(tmp_level==null){return;}

            Vector2 vector = new Vector2(320f, 180f);
            Vector2 vector2 = vector / tmp_level.ZoomTarget;
            Vector2 vector3 = (tmp_level.ZoomTarget != 1f) ? ((tmp_level.ZoomFocusPoint - vector2 / 2f) / (vector - vector2) * vector) : Vector2.Zero;
            Vector2 vector4 = new Vector2(tmp_level.ScreenPadding, tmp_level.ScreenPadding * 0.5625f);
            Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;
            if (SaveData.Instance.Assists.MirrorMode)
            {
                vector4.X = -vector4.X;
                vector3.X = 320.0f - vector3.X;
            }
            float scale = tmp_level.Zoom * ((320f - tmp_level.ScreenPadding * 2f) / 320f);
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["LightMask"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            Draw.SpriteBatch.Draw(GameplayBuffers.Light, vector3 + vector4, GameplayBuffers.Level.Bounds, Color.White, 0f, vector3, scale, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            Draw.SpriteBatch.End();
            
        }
        //绘制HDFog
        private static void Draw_HDFog()
        {
            //获取实体列表
            List<Entity> FogList;
            DBBCustomEntityManager.TrackEntityList(typeof(FogEffect), out FogList);
            //如果该实体没有被注册或者实体列表为空，则直接结束
            if (FogList == null || FogList.Count == 0)
            {
                Engine.Instance.GraphicsDevice.SetRenderTargets(
                    new RenderTargetBinding(DBBGamePlayBuffers.DBBRenderTargets["LightMask"]),
                    new RenderTargetBinding(DBBGamePlayBuffers.DBBRenderTargets["HDFogMask"]),
                    new RenderTargetBinding(DBBGamePlayBuffers.DBBRenderTargets["HDFog"])
                );
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
                return;
            }
            //刷新一次默认纹理缓冲
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["DefaultTexture"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            //更新光照贴图，有一次RenderTarget的转换开销
            UpdateLightMask();
            DBBEffectSourceManager.DBBEffect["DBBEffect_FBMFluid"].Parameters["light_mask"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["LightMask"]);
            //设置全局掩码缓冲区，有一次RenderTarget的转换开销
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["HDFogMask"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);
            //绘制全局掩码
            Draw.SpriteBatch.Begin(0, DBBFogMaskAlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            foreach (var item in FogList)
            {
                if (item.Visible && item.Scene != null)
                {
                    (item as FogEffect).FogGlobalMaskRender();
                }
            }
            Draw.SpriteBatch.End();
            //设置Fog缓冲区,有一次RenderTarget的转换开销
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["HDFog"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
           
            //绘制雾效
            foreach (var item in FogList)
            {
                if (item.Visible && item.Scene != null)
                {
                    (item as FogEffect).FogRender();
                }
            }   
        }
        //将HDFog绘制到默认屏幕上
        private static void Draw_HDFog_On_Default()
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, DBBFogMaskAlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(DBBGamePlayBuffers.DBBRenderTargets["HDFog"], Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            
        }
    }
}