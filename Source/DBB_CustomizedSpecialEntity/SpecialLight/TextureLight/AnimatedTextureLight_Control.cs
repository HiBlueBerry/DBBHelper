using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;


namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/AnimatedTextureLightControl")]
    public class AnimatedTextureLightControl : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式
        private string label = "Default";//区域所控制的特效是哪一个

        //------------------可以更改的特效性质------------------
        private Color tint_color_start = Color.White;
        private Color tint_color_end = Color.White;
        private string tint_color_control_mode = "Linear";
        private Color tint_color_tmp = Color.White;

        private float scaleX_start = 1.0f;
        private float scaleX_end = 1.0f;
        private string scaleX_control_mode = "Linear";
        private float scaleX_tmp = 1.0f;

        private float scaleY_start = 1.0f;
        private float scaleY_end = 1.0f;
        private string scaleY_control_mode = "Linear";
        private float scaleY_tmp = 1.0f;

        private float rotation_start = 0.0f;
        private float rotation_end = 0.0f;
        private string rotation_control_mode = "Linear";
        private float rotation_tmp = 0.0f;

        private float delay_time_start = 0.01f;
        private float delay_time_end = 0.01f;
        private string delay_time_control_mode = "Linear";
        private float delay_time_tmp = 0.01f;

        public AnimatedTextureLightControl(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");
            label = data.Attr("Label");

            tint_color_start = Calc.HexToColor(data.Attr("TintColorStart"));
            tint_color_end = Calc.HexToColor(data.Attr("TintColorEnd"));
            tint_color_control_mode = data.Attr("TintColorControlMode");

            scaleX_start = data.Float("ScaleXStart");
            scaleX_end = data.Float("ScaleXEnd");
            scaleX_control_mode = data.Attr("ScaleXControlMode");

            scaleY_start = data.Float("ScaleYStart");
            scaleY_end = data.Float("ScaleYEnd");
            scaleY_control_mode = data.Attr("ScaleYControlMode");

            rotation_start = data.Float("RotationStart");
            rotation_end = data.Float("RotationEnd");
            rotation_control_mode = data.Attr("RotationControlMode");

            delay_time_start = data.Float("DelayTimeStart");
            delay_time_end = data.Float("DelayTimeEnd");
            delay_time_control_mode = data.Attr("DelayTimeControlMode");
            
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TransitionListener handle_value = new TransitionListener();
            handle_value.OnIn = delegate (float f)
            {
                UpdateParameter();
            };
            Add(handle_value);
        }
        private float Calc_tmpValue(float t, string mode, float start, float end)
        {
            float time = DBBMath.MotionMapping(t, mode);
            return (float)DBBMath.Linear_Lerp(time, start, end);
        }
        private Color Calc_tmpValue(float t, string mode, Color start, Color end)
        {
            float time = DBBMath.MotionMapping(t, mode);
            Color tmp_color = start;
            DBBMath.Linear_Lerp(time, start.R, end.R);
            tmp_color.R = (byte)DBBMath.Linear_Lerp(time, start.R, end.R);
            tmp_color.G = (byte)DBBMath.Linear_Lerp(time, start.G, end.G);
            tmp_color.B = (byte)DBBMath.Linear_Lerp(time, start.B, end.B);
            tmp_color.A = (byte)DBBMath.Linear_Lerp(time, start.A, end.A);
            return tmp_color;
        }
        //尝试更新参数
        private void UpdateParameter()
        {
            if (Scene == null)
            {
                return;
            }
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                return;
            }
            Vector2 offset = player.Position - Position;
            //获取X比例和Y比例，首先只有人物中心在区域之内才能有效果，其次要根据控制方式来确定是X生效还是Y生效
            float x_proportion = offset.X / area.Width;
            float y_proportion = offset.Y / area.Height;
            if (x_proportion >= 0.0f && x_proportion <= 1.0f && y_proportion >= 0.0f && y_proportion <= 1.0f)
            {
                //从左到右模式
                if (area_control_mode == "Left_to_Right")
                {
                    tint_color_tmp = Calc_tmpValue(x_proportion, tint_color_control_mode, tint_color_start, tint_color_end);
                    scaleX_tmp = Calc_tmpValue(x_proportion, scaleX_control_mode, scaleX_start, scaleX_end);
                    scaleY_tmp = Calc_tmpValue(x_proportion, scaleY_control_mode, scaleY_start, scaleY_end);
                    rotation_tmp = Calc_tmpValue(x_proportion, rotation_control_mode, rotation_start, rotation_end);
                    delay_time_tmp = Calc_tmpValue(x_proportion, delay_time_control_mode, delay_time_start, delay_time_end);
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    tint_color_tmp = Calc_tmpValue(y_proportion, tint_color_control_mode, tint_color_start, tint_color_end);
                    scaleX_tmp = Calc_tmpValue(y_proportion, scaleX_control_mode, scaleX_start, scaleX_end);
                    scaleY_tmp = Calc_tmpValue(y_proportion, scaleY_control_mode, scaleY_start, scaleY_end);
                    rotation_tmp = Calc_tmpValue(y_proportion, rotation_control_mode, rotation_start, rotation_end);
                    delay_time_tmp = Calc_tmpValue(y_proportion, delay_time_control_mode, delay_time_start, delay_time_end);
                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    tint_color_tmp = tint_color_end;
                    scaleX_tmp = scaleX_end;
                    scaleY_tmp = scaleY_end;
                    rotation_tmp = rotation_end;
                    delay_time_tmp = delay_time_end;
                }
                List<Entity> AnimatedTextureLightList;
                if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(AnimatedTextureLight), out AnimatedTextureLightList) || AnimatedTextureLightList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in AnimatedTextureLightList)
                {
                    var item = entity_item as AnimatedTextureLight;
                    //控制器控制和它的标签相同且已经被激活的雾效，而且它们应该在同一个房间里
                    if (item.label == label && item.Active == true)
                    {
                        item.tint_color = tint_color_tmp;
                        item.ref_tint_color = tint_color_tmp;
                        item.scaleX = scaleX_tmp;
                        item.scaleY = scaleY_tmp;
                        item.rotation = rotation_tmp;
                        item.delay_time = delay_time_tmp;
                        item.light_sprite.currentAnimation.Delay = delay_time_tmp;
                    }
                }
            }
            return;
        }
        public override void Update()
        {
            base.Update();
            UpdateParameter();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Position, area.Width, area.Height, Color.WhiteSmoke);
        }
    }
}