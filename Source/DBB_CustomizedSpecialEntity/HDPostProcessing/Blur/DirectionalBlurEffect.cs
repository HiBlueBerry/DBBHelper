using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/DirectionalBlurEffect")]
    //方向模糊
    public class DirectionalBlurEffect : DBBGeneralBlur
    {
        public float blur_radius = 0.02f;//模糊半径
        public float angle = 0.5f;//模糊向量的角度，弧度制
        private int iter = 5;//模糊迭代次数，次数越多效果越强烈
        public DirectionalBlurEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            blur_radius = data.Float("BlurRadius");
            angle = data.Float("Angle");
            iter = data.Int("Iter");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Update()
        {
            base.Update();
        }
        public override void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["DBBEffect_DirectionalBlur"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["BlurMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_DirectionalBlur"].Parameters["blur_radius"].SetValue(blur_radius);
            DBBEffectSourceManager.DBBEffect["DBBEffect_DirectionalBlur"].Parameters["angle"].SetValue(angle);
            DBBEffectSourceManager.DBBEffect["DBBEffect_DirectionalBlur"].Parameters["iter"].SetValue(iter);

        }
        public override void BlurRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_DirectionalBlur"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}