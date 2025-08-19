using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/RadialBlurEffect")]
    //径向模糊
    public class RadialBlurEffect:DBBGeneralBlur
    {
        private Vector2 center=new Vector2(0.5f,0.5f);//径向模糊的聚焦点，(0.5,0.5)为中心
        private float blur_radius=0.01f;//模糊半径，正数时呈现向外发散的效果，负数时呈现向内收缩的效果
        private int iter=5;//模糊次数，次数越多径向模糊效果越重

        public RadialBlurEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            float x = data.Float("CenterX");
            float y = data.Float("CenterY");
            center = new Vector2(x, y);
            blur_radius = data.Float("BlurRadius");
            iter = data.Int("BlurIter");
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
            DBBEffectSourceManager.DBBEffect["DBBEffect_RadialBlur"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["BlurMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_RadialBlur"].Parameters["center"].SetValue(center);
            DBBEffectSourceManager.DBBEffect["DBBEffect_RadialBlur"].Parameters["blur_radius"].SetValue(blur_radius);
            DBBEffectSourceManager.DBBEffect["DBBEffect_RadialBlur"].Parameters["iter"].SetValue(iter);
        }
        public override void BlurRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0,BlendState.AlphaBlend,SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone,DBBEffectSourceManager.DBBEffect["DBBEffect_RadialBlur"],Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer,transformed_global_clip_area,transformed_global_clip_area,Color.White);
            Draw.SpriteBatch.End();
        }
    }
}