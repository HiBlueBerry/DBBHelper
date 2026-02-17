using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/AdvancedBlockGlitchEffect")]
    //错位块故障(高级)
    public class AdvancedBlockGlitchEffect : DBBGeneralGlitch
    {
        public float velocity_first = 1.0f;//第一错位块的变化速度
        public float velocity_second = 1.0f;//第二错位块的变化速度
        public Vector2 size_first = new Vector2(4.0f, 4.0f);//第一错位块的规格，其中X值为每行的错位块的基准数目，Y值为每列的错位块的基准数目
        public Vector2 size_second = new Vector2(4.0f, 4.0f);//第二错位块的规格，参数作用与上述同理
        public float strength = 1.0f;//错位块故障强度
        private bool rgb_split = false;//错位块故障是否为RGB分离特效的形式
        private int rgb_split_for_shader = 0;

        //两个时间参数，用于控制特效的动态变化
        private float time1 = 0.0f;
        private float time2 = 0.0f;
        public AdvancedBlockGlitchEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            velocity_first = data.Float("VelocityFirst");
            velocity_second = data.Float("VelocitySecond");
            size_first = new Vector2(data.Float("SizeFirstX"), data.Float("SizeFirstY"));
            size_second = new Vector2(data.Float("SizeSecondX"), data.Float("SizeSecondY"));
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
            time1 += Engine.DeltaTime * velocity_first;
            time2 += Engine.DeltaTime * velocity_second;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"].Parameters["time1"].SetValue(time1);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"].Parameters["time2"].SetValue(time2);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"].Parameters["size1_uv"].SetValue(size_first);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"].Parameters["size2_uv"].SetValue(size_second);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"].Parameters["strength"].SetValue(strength);
            DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"].Parameters["rgb_split"].SetValue(rgb_split_for_shader);
        }
        public override void GlitchRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBEffect_AdvancedBlockGlitch"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
    }
}