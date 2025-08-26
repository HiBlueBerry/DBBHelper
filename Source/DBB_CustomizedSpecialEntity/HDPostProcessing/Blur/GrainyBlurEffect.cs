using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/GrainyBlurEffect")]
    [TrackedAs(typeof(DBBGeneralHDpostProcessing))]
    //粒状模糊
    public class GrainyBlurEffect : DBBGeneralBlur
    {
        public float blur_radius = 0.001f;//粒状模糊的模糊半径

        public GrainyBlurEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            blur_radius = data.Float("BlurRadius");
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
            DBBEffectSourceManager.DBBEffect["DBBEffect_GrainyBlur"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["BlurMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_GrainyBlur"].Parameters["blur_radius"].SetValue(blur_radius);
        }
        public override void BlurRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_GrainyBlur"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}