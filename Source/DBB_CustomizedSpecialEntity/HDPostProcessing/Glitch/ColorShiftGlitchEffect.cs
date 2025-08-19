using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/ColorShiftGlitchEffect")]

    //RGB颜色分离故障(基础)
    public class ColorShiftGlitchEffect:DBBGeneralGlitch
    {
        public float split_amount=0.01f;//RGB特效分离程度
        public float angle=0.0f;//RGB特效分离方向的角度，弧度制
        public float velocity = 0.0f;//特效变化速度
        public bool uv_mode = false;//RGB特效是否使用UV随机数模式
        private int uv_mode_for_shader = 0;
        private float time = 0.0f;
        public ColorShiftGlitchEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            split_amount = data.Float("SplitAmount");
            angle = data.Float("Angle");
            velocity = data.Float("Velocity");
            uv_mode = data.Bool("UVMode");
            uv_mode_for_shader = uv_mode == true ? 1 : 0;
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
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorShiftGlitch"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorShiftGlitch"].Parameters["split_amount"].SetValue(split_amount);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorShiftGlitch"].Parameters["angle"].SetValue(angle);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorShiftGlitch"].Parameters["uv_mode"].SetValue(uv_mode_for_shader);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ColorShiftGlitch"].Parameters["seed"].SetValue((int)time);
        }
        
        public override void GlitchRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0,BlendState.AlphaBlend,SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone,DBBEffectSourceManager.DBBEffect["DBBEffect_ColorShiftGlitch"],Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer,transformed_global_clip_area,transformed_global_clip_area,Color.White);
            Draw.SpriteBatch.End();
        }
    }
}