using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/BlockGlitchEffect")]
    //错位块故障(基础)
    public class BlockGlitchEffect:DBBGeneralGlitch
    {
        public float velocity=1.0f;//错位块变化速度
        public float block_num=4.0f;//每行的错位块基准数目，可以不是整数
        public float strength=1.0f;//错位块故障强度
        private bool rgb_split=false;//错位块故障是否为RGB分离特效的形式
        private int rgb_split_for_shader = 0;
        private float time = 0.0f;
        public BlockGlitchEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            velocity = data.Float("Velocity");
            block_num = data.Float("BlockNum");
            strength = data.Float("Strength");
            rgb_split = data.Bool("RGBSplit");
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
            DBBEffectSourceManager.DBBEffect["DBBEffect_BlockGlitch"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_BlockGlitch"].Parameters["time"].SetValue(time);
            DBBEffectSourceManager.DBBEffect["DBBEffect_BlockGlitch"].Parameters["block_num"].SetValue(block_num);
            DBBEffectSourceManager.DBBEffect["DBBEffect_BlockGlitch"].Parameters["strength"].SetValue(strength);
            DBBEffectSourceManager.DBBEffect["DBBEffect_BlockGlitch"].Parameters["rgb_split"].SetValue(rgb_split_for_shader);
        }
        public override void GlitchRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0,BlendState.AlphaBlend,SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone,DBBEffectSourceManager.DBBEffect["DBBEffect_BlockGlitch"],Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer,transformed_global_clip_area,transformed_global_clip_area,Color.White);
            Draw.SpriteBatch.End();
        }
    }
}