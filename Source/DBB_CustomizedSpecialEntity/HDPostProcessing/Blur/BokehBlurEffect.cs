using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/BokehBlurEffect")]
    //散景模糊
    public class BokehBlurEffect:DBBGeneralBlur
    {
        private float inner_radius=0.0f;//散景光圈内半径
        private float interval=0.01f;//散景光圈内外半径间距
        private int iter=8;//散景光圈的螺旋线条数
        public BokehBlurEffect(EntityData data, Vector2 offset):base(data, offset)
        {
            inner_radius=data.Float("InnerRadius");
            interval=data.Float("Interval");
            iter=data.Int("Iter");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Update()
        {
            base.Update();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["DBBEffect_BokehBlur"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["BlurMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_BokehBlur"].Parameters["inner_radius"].SetValue(inner_radius);
            DBBEffectSourceManager.DBBEffect["DBBEffect_BokehBlur"].Parameters["interval"].SetValue(interval);
            DBBEffectSourceManager.DBBEffect["DBBEffect_BokehBlur"].Parameters["iter"].SetValue(iter);
        }
        public override void BlurRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0,BlendState.AlphaBlend,SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone,DBBEffectSourceManager.DBBEffect["DBBEffect_BokehBlur"],Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer,transformed_global_clip_area,transformed_global_clip_area,Color.White);
            Draw.SpriteBatch.End();
        }
    }
}