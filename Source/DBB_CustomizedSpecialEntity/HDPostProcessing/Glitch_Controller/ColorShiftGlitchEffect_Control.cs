using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/ColorShiftGlitchEffectControl")]
    public class ColorShiftGlitchEffectControl : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式
        private string label = "Default";//区域所控制的特效是哪一个

        //------------------可以更改的特效性质------------------
        private float split_amount_start = 0.01f;
        private float split_amount_end = 0.01f;
        private string split_amount_control_mode = "Linear";
        private float split_amount_tmp = 0.01f;

        private float angle_start = 0.0f;
        private float angle_end = 0.0f;
        private string angle_control_mode = "Linear";
        private float angle_tmp = 0.0f;

        private float velocity_start = 0.0f;
        private float velocity_end = 0.0f;
        private string velocity_control_mode = "Linear";
        private float velocity_tmp = 0.0f;

        public ColorShiftGlitchEffectControl(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");
            label = data.Attr("Label");

            split_amount_start = data.Float("SplitAmountStart");
            split_amount_end = data.Float("SplitAmountEnd");
            split_amount_control_mode = data.Attr("SplitAmountControlMode");

            angle_start = data.Float("AngleStart");
            angle_end = data.Float("AngleEnd");
            angle_control_mode = data.Attr("AngleControlMode");

            velocity_start = data.Float("VelocityStart");
            velocity_end = data.Float("VelocityEnd");
            velocity_control_mode = data.Attr("VelocityControlMode");
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
                    split_amount_tmp = Calc_tmpValue(x_proportion, split_amount_control_mode, split_amount_start, split_amount_end);
                    angle_tmp = Calc_tmpValue(x_proportion, angle_control_mode, angle_start, angle_end);
                    velocity_tmp = Calc_tmpValue(x_proportion, velocity_control_mode, velocity_start, velocity_end);
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    split_amount_tmp = Calc_tmpValue(y_proportion, split_amount_control_mode, split_amount_start, split_amount_end);
                    angle_tmp = Calc_tmpValue(y_proportion, angle_control_mode, angle_start, angle_end);
                    velocity_tmp = Calc_tmpValue(y_proportion, velocity_control_mode, velocity_start, velocity_end);
                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    split_amount_tmp = split_amount_end;
                    angle_tmp = angle_end;
                    velocity_tmp = velocity_end;
                }
                List<Entity> ColorShiftGlitchEffectList;
                if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(ColorShiftGlitchEffect), out ColorShiftGlitchEffectList) || ColorShiftGlitchEffectList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in ColorShiftGlitchEffectList)
                {
                    var item = entity_item as ColorShiftGlitchEffect;
                    //控制器控制和它的标签相同且已经被激活的雾效，而且它们应该在同一个房间里
                    if (item.label == label && item.Active == true)
                    {
                        item.split_amount = split_amount_tmp;
                        item.angle = angle_tmp;
                        item.velocity = velocity_tmp;
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