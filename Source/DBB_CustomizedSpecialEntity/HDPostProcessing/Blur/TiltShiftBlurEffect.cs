using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{

    [CustomEntity("DBBHelper/TiltShiftBlurEffect")]
    [TrackedAs(typeof(DBBGeneralHDpostProcessing))]
    //移轴模糊
    public class TiltShiftBlurEffect : DBBGeneralBlur
    {
        private float stride = 0.02f;//模糊半径
        private float spread = 1.0f;//控制清晰的长条区域的发散程度，应该大于0
        private float offset = 0.0f;//控制长条区域的中心位置，0.0为长条区域在中心
        private float tilt_shift_area = 1.0f;//控制清晰的长条区域的面积，取值为0到1
        private bool mask_mode = false;//掩码模式，此模式方便调试，掩码的黑色区域即为清晰的长条区域
        private int mask_mode_for_shader = 0;
        private bool reverse = false;//掩码的黑色区域是否反转
        private int reverse_for_shader = 0;
        public TiltShiftBlurEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            stride = data.Float("Stride");
            spread = data.Float("Spread");
            this.offset = data.Float("Offset");
            tilt_shift_area = data.Float("Area");
            mask_mode = data.Bool("MaskMode");
            reverse = data.Bool("Reverse");
            mask_mode_for_shader = mask_mode == true ? 1 : 0;
            reverse_for_shader = reverse == true ? 1 : 0;
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
            DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["BlurMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"].Parameters["stride"].SetValue(stride);
            DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"].Parameters["spread"].SetValue(spread);
            DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"].Parameters["offset"].SetValue(offset);
            DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"].Parameters["area"].SetValue(tilt_shift_area);
            DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"].Parameters["mask_mode"].SetValue(mask_mode_for_shader);
            DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"].Parameters["reverse"].SetValue(reverse_for_shader);
        }
        public override void BlurRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_TiltShiftBlur"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}