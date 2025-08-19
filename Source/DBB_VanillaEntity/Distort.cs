using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities {

    [CustomEntity("DBBHelper/Distort")]
    public class Distort:Entity
    {
        //------------一些通用参数------------
        private float distort_strength=1.0f;//扭曲效果的力度
        private float wave_strength=1.0f;//波动效果的力度
        private float scale=1.0f;//图片缩放比例
        private float rotation=0.0f;//图片旋转角度
        private bool draw_mask=false;//实际绘制时是否应当把掩码图片也绘制出来
        private bool draw_mask_when_debug=false;//Debug时是否应当把掩码图片绘制出来
        //------------一些内部参数------------
        private string path="";//掩码图片的路径
        private MTexture distort_texture=null;//贴图
        public Distort(EntityData data,Vector2 offset)
        {
            Position=data.Position+offset;

            path = data.Attr("path");
            distort_strength = data.Float("distortStrength");
            wave_strength = data.Float("waveStrength");
            scale = data.Float("scale");
            rotation = -data.Float("rotation") / 180.0f * (float)Math.PI;
            draw_mask = data.Bool("DrawMask");
            draw_mask_when_debug = data.Bool("DebugMask");
            distort_texture = GFX.Game[path];
            if(GFX.Game.Has(path)==false)
            {
                distort_texture=GFX.Game["objects/DBB_Items/Distort/sphere_gradient"];
            }
            Depth=-100;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if(distort_texture!=null)
            {
                Add(new DisplacementRenderHook(RenderDisplacement));
            }
        }
        public void RenderDisplacement()
        {
            distort_texture.DrawCentered(Position,new Color(wave_strength,wave_strength,wave_strength,distort_strength),scale,rotation);   
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if(draw_mask_when_debug==true)
            {
                distort_texture.DrawOutlineCentered(Position,new Color(1.0f,1.0f,1.0f,1.0f),scale,rotation);  
            }
        }
        public override void Render()
        {
            base.Render();
            if(draw_mask==true)
            {
                distort_texture.DrawCentered(Position,new Color(1.0f,1.0f,1.0f,1.0f),scale,rotation);
            }
        }
    }


}