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
    public class DBBGeneralGlitch : DBBGeneralHDpostProcessing
    {
        public static Vector2 ref_resolution_vector_glitch = new Vector2(1922.0f, 1082.0f);
        public static List<Entity> GlitchList;//存储所有被添加到当前场景的Glitch
        public string label = "Default";
        public DBBGeneralGlitch(EntityData data, Vector2 offset) : base(data, offset)
        {
            label = data.Attr("Label");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void Update()
        {
            base.Update();
        }
        //渲染用
        public virtual void GlitchRender()
        {

        }
        //这里不使用原版的Render
        public override void Render()
        {

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(area.X, area.Y, area.Width, area.Height, Color.Pink);
        }
        public static void GlitchLoad()
        {
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create += Init_BlurBuffer;
            DBBGeneralHDpostProcessing.Adjust_SomeBuffers_Before_HDPostProcessing_Render += Adjust_Some_GlitchBuffers_WH;
            DBBGeneralHDpostProcessing.Draw_Mask += Draw_GlitchMask;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing += Swap_Buffer;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing += Draw_GlitchEffect_On_HDPostProcessing;
        }
        public static void GlitchUnLoad()
        {
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing -= Draw_GlitchEffect_On_HDPostProcessing;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing -= Swap_Buffer;
            DBBGeneralHDpostProcessing.Draw_Mask -= Draw_GlitchMask;
            DBBGeneralHDpostProcessing.Adjust_SomeBuffers_Before_HDPostProcessing_Render -= Adjust_Some_GlitchBuffers_WH;
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create -= Init_BlurBuffer;
        }
        private static void Init_BlurBuffer()
        {
            DBBGamePlayBuffers.Create("GlitchMask", 1922, 1082);//用于失真的掩码区域
            ref_resolution_vector_glitch = new Vector2(1922.0f, 1082.0f);
        }
        private static void Draw_GlitchMask()
        {
            //如果没有任何Glitch类型被注册，则返回
            if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(DBBGeneralGlitch), out GlitchList) || GlitchList.Count == 0)
            {
                return;
            }
            //设置模糊的全局掩码缓冲区，有一次RenderTarget的转换开销
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);
            //绘制全局掩码
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            foreach (var item in GlitchList)
            {
                if (item.Visible)
                {
                    (item as DBBGeneralHDpostProcessing).GlobalMaskRender();
                }
            }
            Draw.SpriteBatch.End();
        }
        private static void Draw_GlitchEffect_On_HDPostProcessing()
        {
            if (GlitchList == null || GlitchList.Count == 0)
            {
                return;
            }
            //绘制失真
            foreach (var item in GlitchList)
            {
                if (item.Visible)
                {
                    (item as DBBGeneralGlitch).GlitchRender();
                }
            }
        }
        private static void Adjust_Some_GlitchBuffers_WH()
        {
            //如果当前分辨率与上一次的分辨率不同，则调整
            if (ref_resolution_vector_glitch != ref_resolution_vector)
            {
                ref_resolution_vector_glitch = ref_resolution_vector;
                DBBGamePlayBuffers.ReloadBuffer("GlitchMask", ref_resolution_vector_glitch);//调整缓冲区的大小
            }
            //Logger.Log(LogLevel.Warn,"ref_resolution_vector_glitch",ref_resolution_vector_glitch.ToString());
        }
        
    }
}