using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
using System;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/AnalogNoiseGlitchEffect")]
    [TrackedAs(typeof(DBBGeneralHDpostProcessing))]
    //模拟噪点故障
    public class AnalogNoiseGlitchEffect : DBBGeneralGlitch
    {
        public float velocity = 1.0f;//特效变化速度
        public float strength = 0.25f;//特效强度
        public float jitter_velocity = 1.0f;//灰度抖动的速度
        public float jitter_threshold = 0.5f;//灰度抖动的阈值，当随机产生的值达到阈值时发生灰度抖动，此值越高越难发生抖动
        private bool grey_jitter = false;//灰度抖动模式
        private int grey_jitter_for_shader = 0;
        private float time = 1.1f;
        private float jitter_time = 0.01f;

        public AnalogNoiseGlitchEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            velocity = data.Float("Velocity");
            strength = data.Float("Strength");
            jitter_velocity = data.Float("JitterVelocity");
            jitter_threshold = data.Float("JitterThreshold");
            grey_jitter = data.Bool("GreyJitter");
            grey_jitter_for_shader = grey_jitter == true ? 1 : 0;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Update()
        {
            base.Update();
            time += Engine.DeltaTime * velocity;
            jitter_time += Engine.DeltaTime * jitter_velocity;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["DBBEffect_AnalogNoiseGlitch"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AnalogNoiseGlitch"].Parameters["time"].SetValue(time);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AnalogNoiseGlitch"].Parameters["strength"].SetValue(strength);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AnalogNoiseGlitch"].Parameters["jitter_time"].SetValue((int)jitter_time);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AnalogNoiseGlitch"].Parameters["jitter_threshold"].SetValue(jitter_threshold);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AnalogNoiseGlitch"].Parameters["grey_jitter"].SetValue(grey_jitter_for_shader);
        }
        public override void GlitchRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_AnalogNoiseGlitch"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}