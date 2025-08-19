using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/LineGlitchEffect")]
    //错位线故障
    public class LineGlitchEffect:DBBGeneralGlitch
    {
        public float velocity=1.0f;//故障线条变化速度
        public float num_level=4.0f;//故障块基本层级数，此值越高故障线条块越多
        public float detail=2.0f;//故障线条块的细节，此值越高，每个线条块会出现更多细节
        public float strength=0.01f;//故障特效的强度
        private bool vertical=false;//故障线条是横线还是竖线
        private int vertical_for_shader = 0;
        private bool rgb_split = false;//故障是否为RGB分离特效的形式
        private int rgb_split_for_shader = 0;
        private float time = 0.0f;
        public LineGlitchEffect(EntityData data, Vector2 offset) : base(data, offset)
        {
            velocity = data.Float("Velocity");
            num_level = data.Float("NumLevel");
            detail = data.Float("Detail");
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
            time+=Engine.DeltaTime*velocity;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public override void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"].Parameters["mask_tex"].SetValue(DBBGamePlayBuffers.DBBRenderTargets["GlitchMask"]);
            DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"].Parameters["num_level"].SetValue(num_level);
            DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"].Parameters["detail"].SetValue(detail);
            DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"].Parameters["strength"].SetValue(strength);
            DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"].Parameters["time"].SetValue(time);
            DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"].Parameters["vertical"].SetValue(vertical_for_shader);
            DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"].Parameters["rgb_split"].SetValue(rgb_split_for_shader);
        }
        public override void GlitchRender()
        {
            SetAllParameter();
            Draw.SpriteBatch.Begin(0,BlendState.AlphaBlend,SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone,DBBEffectSourceManager.DBBEffect["DBBEffect_LineGlitch"],Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer,transformed_global_clip_area,transformed_global_clip_area,Color.White);
            Draw.SpriteBatch.End();
        }
    }
}