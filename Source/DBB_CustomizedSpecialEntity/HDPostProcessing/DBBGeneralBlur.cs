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
    public class DBBGeneralBlur : DBBGeneralHDpostProcessing
    {
        private static Vector2 ref_resolution_vector_blur = new Vector2(1922.0f, 1082.0f);
        private static List<Entity> BlurList;//存储所有被添加到当前场景的Blur
        public string label = "Default";
        public DBBGeneralBlur(EntityData data, Vector2 offset) : base(data, offset)
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
        public virtual void BlurRender()
        {

        }
        //这里不使用原版的Render
        public override void Render()
        {

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(area.X, area.Y, area.Width, area.Height, Color.SkyBlue);
        }
        public static void BlurLoad()
        {
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create += Init_BlurBuffer;
            DBBGeneralHDpostProcessing.Adjust_SomeBuffers_Before_HDPostProcessing_Render += Adjust_Some_BlurBuffers_WH;
            DBBGeneralHDpostProcessing.Draw_Mask += Draw_BlurMask;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing += Swap_Buffer;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing += Draw_BlurEffect_On_HDPostProcessing;

        }
        public static void BlurUnLoad()
        {
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing -= Draw_BlurEffect_On_HDPostProcessing;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing -= Swap_Buffer;
            DBBGeneralHDpostProcessing.Draw_Mask -= Draw_BlurMask;
            DBBGeneralHDpostProcessing.Adjust_SomeBuffers_Before_HDPostProcessing_Render -= Adjust_Some_BlurBuffers_WH;
            DBBEffectSourceManager.Init_SomeBuffers_When_Level_Create -= Init_BlurBuffer;
            //这里不用手动删除BlurMask缓冲区，它会在UnLoad的最后被DBBGamePlayBuffers删除的
        }
        private static void Init_BlurBuffer()
        {
            DBBGamePlayBuffers.Create("BlurMask", 1922, 1082);//用于模糊的掩码区域
            ref_resolution_vector_blur = new Vector2(1922.0f, 1082.0f);
        }
        private static void Draw_BlurMask()
        {
            //如果没有任何Blur类型被注册，则返回
            if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(DBBGeneralBlur), out BlurList) || BlurList.Count == 0)
            {
                return;
            }
            //设置模糊的全局掩码缓冲区，有一次RenderTarget的转换开销
            Engine.Instance.GraphicsDevice.SetRenderTarget(DBBGamePlayBuffers.DBBRenderTargets["BlurMask"]);
            Engine.Instance.GraphicsDevice.Clear(Color.Black);

            //绘制全局掩码
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            foreach (var item in BlurList)
            {
                if (item.Visible)
                {
                    (item as DBBGeneralHDpostProcessing).GlobalMaskRender();
                }
            }
            Draw.SpriteBatch.End();

        }
        private static void Draw_BlurEffect_On_HDPostProcessing()
        {
            //如果没有任何Blur类型被注册，则返回
            if (BlurList == null || BlurList.Count == 0)
            {
                return;
            }
            //绘制模糊
            foreach (var item in BlurList)
            {
                if (item.Visible)
                {
                    (item as DBBGeneralBlur).BlurRender();
                }
            }
        }
        private static void Adjust_Some_BlurBuffers_WH()
        {
            //如果当前分辨率与上一次的分辨率不同，则调整
            if (ref_resolution_vector_blur != ref_resolution_vector)
            {
                ref_resolution_vector_blur = ref_resolution_vector;
                DBBGamePlayBuffers.ReloadBuffer("BlurMask", ref_resolution_vector_blur);//调整缓冲区的大小
            }

        }

    }
}