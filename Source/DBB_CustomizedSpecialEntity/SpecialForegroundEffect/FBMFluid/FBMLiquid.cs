using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.DBBHelper.Mechanism;
using Celeste.Mod.Entities;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/FBMLiquid")]
    public class FBMLiquid : DBBGeneralHDpostProcessing
    {
        public static List<Entity> LiquidList;

        //控制液体的整体效果
        float scale = 2.0f;//缩放等级
        float amplify = 1.5f;//液体幅度
        float frequency = 2.0f;//液体细节度
        bool frac_mode = false;//frac模式
        Vector4 color1 = new Vector4(0.3f, 0.6f, 0.4f, 1.0f);//颜色1
        Vector4 color2 = new Vector4(0.4f, 0.95f, 0.9f, 1.0f);//颜色2

        //控制水平黑波范围
        Vector2 horizontal_bottom_and_top = new Vector2(0.0f, 1.0f);//底部的黑边的开始减退的位置，最大为0.5和顶部的黑边的开始渐退的位置,最小为0.5
        float black_edge_amp = 0.2f;//黑波幅度

        //控制垂直黑边的范围
        Vector2 vertical_left_and_right = new Vector2(0.1f, 0.8f);//左侧的黑边的开始减退的位置和右侧的黑边的开始减退的位置
        float vertical_offset = 0.5f;//黑边的参考偏移,0.5为正中心，更大的值会让黑边向左，更小的值会让黑边向右

        //控制液体的高光
        Vector4 highlight_color1 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);//高光颜色1
        Vector4 highlight_color2 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);//高光颜色2
        float highlight_amp = 0.2f;//高光的幅度

        //控制液体对原图像的影响幅度
        Vector2 wave_influence_amp = new Vector2(0.08f, 0.0f);

        //控制液体波动和流动方向
        Vector2 move_velocity = new Vector2(0.1f, 0);//液体流动速度
        Vector2 wave_velocity = new Vector2(0.1f, 0);//液体波动速度

        Vector2 move_time = Vector2.Zero;
        Vector2 wave_time = Vector2.Zero;
        public FBMLiquid(EntityData data, Vector2 offset) : base(data, offset)
        {

            Position = data.Position + offset;
            //Collider = new Hitbox(area.Width, area.Height);
            //控制液体的整体效果
            scale = data.Float("Scale");
            amplify = data.Float("Amplify");
            frequency = data.Float("Frequency");
            frac_mode = data.Bool("FracMode");
            string[] all_base_color = data.Attr("BaseColor").Split([',']);
            color1 = DBBMath.ConvertColor(all_base_color[0]);
            color2 = DBBMath.ConvertColor(all_base_color[1]);
            all_base_color = data.Attr("BaseColorAlpha").Split([',']);
            color1.W = float.Parse(all_base_color[0]);
            color2.W = float.Parse(all_base_color[1]);

            //控制水平黑波范围
            string[] all_value = data.Attr("HorizontalBlackBounds").Split([',']);
            horizontal_bottom_and_top = new Vector2(float.Parse(all_value[0]), float.Parse(all_value[1]));
            black_edge_amp = data.Float("HorizontalBlackAmp");

            //控制垂直黑边的范围
            all_value = data.Attr("VerticalBlackBounds").Split([',']);
            vertical_left_and_right = new Vector2(float.Parse(all_value[0]), float.Parse(all_value[1]));
            vertical_offset = data.Float("VerticalBlackOffest");

            //控制液体的高光
            string[] all_highlight_color = data.Attr("HighLightColor").Split([',']);
            highlight_color1 = DBBMath.ConvertColor(all_highlight_color[0]);
            highlight_color2 = DBBMath.ConvertColor(all_highlight_color[1]);
            all_highlight_color = data.Attr("HighLightColorAlpha").Split([',']);
            highlight_color1.W = float.Parse(all_highlight_color[0]);
            highlight_color2.W = float.Parse(all_highlight_color[1]);
            highlight_amp = data.Float("HighLightAmplify");

            //控制液体对原图像的影响幅度
            all_value = data.Attr("WaveInfluenceAmp").Split([',']);
            wave_influence_amp = new Vector2(float.Parse(all_value[0]), float.Parse(all_value[1]));

            //控制液体波动和流动方向
            all_value = data.Attr("FluidMoveVelocity").Split([',']);
            move_velocity = new Vector2(float.Parse(all_value[0]), float.Parse(all_value[1]));
            all_value = data.Attr("FluidWaveVelocity").Split([',']);
            wave_velocity = new Vector2(float.Parse(all_value[0]), float.Parse(all_value[1]));

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TransitionListener handle_active = new TransitionListener();
            handle_active.OnInBegin = delegate ()
            {
                Active = true;
            };
            handle_active.OnOutBegin = delegate ()
            {
                Active = false;
            };
            Add(handle_active);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public override void Update()
        {
            base.Update();
            move_time += move_velocity * Engine.DeltaTime;
            wave_time += wave_velocity * Engine.DeltaTime;
        }
        public override void SetAllParameter()
        {
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["scale"].SetValue(scale);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["amp"].SetValue(amplify);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["fre"].SetValue(frequency);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["frac_mode"].SetValue(frac_mode);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["color1"].SetValue(color1);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["color2"].SetValue(color2);

            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["horizontal_bottom_and_top"].SetValue(horizontal_bottom_and_top);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["black_edge_amp"].SetValue(black_edge_amp);

            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["vertical_left_and_right"].SetValue(vertical_left_and_right);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["vertical_offset"].SetValue(vertical_offset);

            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["highlight_color1"].SetValue(highlight_color1);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["highlight_color2"].SetValue(highlight_color2);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["highlight_amp"].SetValue(highlight_amp);

            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["wave_influence_amp"].SetValue(wave_influence_amp);

            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["move_dir"].SetValue(move_time);
            DBBEffectSourceManager.DBBEffect["DBBLiquid"].Parameters["wave_dir"].SetValue(wave_time);
        }
        public void LiquidRender()
        {
            SetAllParameter();
            UpdateClipArea();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, DBBEffectSourceManager.DBBEffect["DBBLiquid"], Matrix.Identity);
            Draw.SpriteBatch.Draw(ContentBuffer, transformed_global_clip_area, transformed_global_clip_area, Color.White);
            Draw.SpriteBatch.End();
        }
        public static void LiquidLoad()
        {
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing += Swap_Buffer;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing += Draw_Liquid_On_HDPostProcessing;
        }
        public static void LiquidUnLoad()
        {
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing -= Draw_Liquid_On_HDPostProcessing;
            DBBGeneralHDpostProcessing.Draw_Something_On_HDPostProcessing -= Swap_Buffer;
        }
        private static void Draw_Liquid_On_HDPostProcessing()
        {
            //如果没有任何Glitch类型被注册，则返回
            if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(FBMLiquid), out LiquidList) || LiquidList.Count == 0)
            {
                return;
            }
            foreach (var item in LiquidList)
            {
                if (item.Visible)
                {
                    (item as FBMLiquid).LiquidRender();
                }
            }
        }
        
    }
    
}