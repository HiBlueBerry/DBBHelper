using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/ScanLineJitterGlitchEffect")]
    //扫描线抖动故障
    public class ScanLineJitterGlitchEffect:DBBGeneralGlitch
    {
        public float velocity=1.0f;
        public float strength=0.05f;
        public float angle=0.0f;
        private float time=0.0f;
        private bool velocity_continuization = false;
        public ScanLineJitterGlitchEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            velocity = data.Float("Velocity");
            strength = data.Float("Strength");
            angle = data.Float("Angle");
            velocity_continuization = data.Bool("VelocityContinuization", false);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Update()
        {
            base.Update();
            if (velocity_continuization)
            {
                time += Engine.RawDeltaTime;
            }
            else
            {
                time += Engine.DeltaTime * velocity;
            }
            
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void SetAllParameter()
        {
            float tmp_time=time;
            if (!velocity_continuization)
            {
                tmp_time = (int)time;
            }
            DBBEffectSourceManager.DBBEffect["DBBEffect_ScanLineJitterGlitch"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ScanLineJitterGlitch"].Parameters["seed"].SetValue(tmp_time);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ScanLineJitterGlitch"].Parameters["angle"].SetValue(angle);
            DBBEffectSourceManager.DBBEffect["DBBEffect_ScanLineJitterGlitch"].Parameters["strength"].SetValue(strength);
        }
        public override void GlitchRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0,BlendState.AlphaBlend,SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone,DBBEffectSourceManager.DBBEffect["DBBEffect_ScanLineJitterGlitch"],Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer,transformed_global_clip_area,transformed_global_clip_area,Color.White);
            Draw.SpriteBatch.End();
        }
    }
}