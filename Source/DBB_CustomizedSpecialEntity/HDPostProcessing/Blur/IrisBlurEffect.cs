using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/IrisBlurEffect")]
    //光圈模糊
    public class IrisBlurEffect : DBBGeneralBlur
    {
        private float stride = 0.02f;//对角线模糊的采样步长
        private float offset = 0.0f;//控制光圈区域的中心位置，0.0为光圈区域在中心
        private float iris_area = 1.0f;//控制光圈区域的面积，取值为0到1
        private float spread = 1.0f;//控制光圈的发散程度，此值可正可负，负数时反相
        private bool mask_mode = false;//掩码模式，此模式方便调试，掩码的黑色区域即为清晰的区域
        private int mask_mode_for_shader = 0;
        public IrisBlurEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            stride = data.Float("Stride");
            this.offset = data.Float("Offset");
            iris_area = data.Float("Area");
            spread = data.Float("Spread");
            mask_mode = data.Bool("MaskMode");
            mask_mode_for_shader = mask_mode == true ? 1 : 0;
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
            DBBEffectSourceManager.DBBEffect["DBBEffect_IrisBlur"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["BlurMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_IrisBlur"].Parameters["stride"].SetValue(stride);
            DBBEffectSourceManager.DBBEffect["DBBEffect_IrisBlur"].Parameters["offset"].SetValue(offset);
            DBBEffectSourceManager.DBBEffect["DBBEffect_IrisBlur"].Parameters["area"].SetValue(iris_area);
            DBBEffectSourceManager.DBBEffect["DBBEffect_IrisBlur"].Parameters["spread"].SetValue(spread);
            DBBEffectSourceManager.DBBEffect["DBBEffect_IrisBlur"].Parameters["mask_mode"].SetValue(mask_mode_for_shader);
        }
        public override void BlurRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_IrisBlur"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}