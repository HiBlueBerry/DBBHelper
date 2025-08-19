using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.DBBHelper.Entities {
    [CustomEntity("DBBHelper/InvisibleLight")]
    public class InvisibleLight : Entity
    {
        private VertexLight m_light;
        private BloomPoint m_bloom;
        private Color color=Color.White;
        public InvisibleLight(Vector2 position,VertexLight light,BloomPoint bloom):base(position)
        {
            m_bloom=bloom;
            m_light=light;
            Add(m_light);
            Add(m_bloom);
        }
        public InvisibleLight(Vector2 position,VertexLight light):base(position)
        {
            m_bloom=new BloomPoint(0.0f,0.0f);
            m_light=light;
            Add(m_light);
            Add(m_bloom);
        }
        public InvisibleLight(EntityData data,Vector2 offset)
        {
            Position=data.Position+offset;
            float bloomradius=data.Float("bloomRadius");
            int light_startfade=data.Int("startFade");
            int light_endfade=data.Int("endFade");
            color=data.HexColor("lightColor");
            m_light=new VertexLight(color,1.0f,light_startfade,light_endfade);
            m_bloom=new BloomPoint(1.0f,bloomradius);
            TransitionListener handle_alpha=new TransitionListener();
            handle_alpha.OnIn=delegate(float f)
            {
                float time=(float)DBBMath.MotionMapping(f,"easeInOutSin");
                m_light.Color.R=(byte)(time*color.R);
                m_light.Color.G=(byte)(time*color.G);
                m_light.Color.B=(byte)(time*color.B);
            };
            handle_alpha.OnOut=delegate(float f)
            {
                float time=(float)DBBMath.Linear_Lerp(DBBMath.MotionMapping(f,"easeInOutSin"),1.0f,0.0f);
                m_light.Color.R=(byte)(time*color.R);
                m_light.Color.G=(byte)(time*color.G);
                m_light.Color.B=(byte)(time*color.B);
            };
            Add(handle_alpha);
            Add(m_light);
            Add(m_bloom);
            
            
        }
    }

}