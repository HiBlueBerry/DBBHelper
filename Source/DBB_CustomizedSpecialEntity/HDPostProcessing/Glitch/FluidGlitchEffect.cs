using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/FluidGlitchEffect")]
    //RGB颜色分离故障(流体)
    public class FluidGlitchEffect:DBBGeneralGlitch
    {
        public float velocity=1.0f;//特效变化速度
        public float strength=0.02f;//特效强度
        private bool vertical=false;//特效波动方向是水平还是竖直
        private int vertical_for_shader = 0;
        private bool rgb_split = false;//特效是否为RGB分离效果
        private int rgb_split_for_shader = 0;
        private float time = 0.0f;
        public FluidGlitchEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            velocity = data.Float("Velocity");
            strength = data.Float("Strength");
            vertical = data.Bool("Vertical");
            rgb_split = data.Bool("RGBSplit");
            vertical_for_shader = vertical == true ? 1 : 0;
            rgb_split_for_shader = rgb_split == true ? 1 : 0;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Update()
        {
            base.Update();
            time += Engine.DeltaTime * velocity;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["DBBEffect_FluidGlitch"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FluidGlitch"].Parameters["time"].SetValue(time);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FluidGlitch"].Parameters["strength"].SetValue(strength);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FluidGlitch"].Parameters["vertical"].SetValue(vertical_for_shader);
            DBBEffectSourceManager.DBBEffect["DBBEffect_FluidGlitch"].Parameters["rgb_split"].SetValue(rgb_split_for_shader);
        }
        public override void GlitchRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_FluidGlitch"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}