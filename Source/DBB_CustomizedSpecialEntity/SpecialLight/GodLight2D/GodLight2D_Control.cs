using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/GodLight2DControl")]
    public class GodLight2DControl : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式
        private string label = "Default";//区域所控制的特效是哪一个

        //------------------可以更改的特效性质------------------
        private float base_strength_start = 0.5f;
        private float base_strength_end = 0.5f;
        private string base_strength_control_mode = "Linear";
        private float base_strength_tmp = 0.5f;

        private float concentration_factor_start = 0.6f;
        private float concentration_factor_end = 0.6f;
        private string concentration_factor_control_mode = "Linear";
        private float concentration_factor_tmp = 0.6f;

        private float extingction_factor_start = 1.0f;
        private float extingction_factor_end = 1.0f;
        private string extingction_factor_control_mode = "Linear";
        private float extingction_factor_tmp = 1.0f;

       

        public GodLight2DControl(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");
            label = data.Attr("Label");

            base_strength_start = data.Float("BaseStrengthStart");
            base_strength_end = data.Float("BaseStrengthEnd");
            base_strength_control_mode = data.Attr("BaseStrengthControlMode");

            concentration_factor_start = data.Float("ConcentrationFactorStart");
            concentration_factor_end = data.Float("ConcentrationFactorEnd");
            concentration_factor_control_mode = data.Attr("ConcentrationFactorControlMode");

            extingction_factor_start = data.Float("ExtingctionFactorStart");
            extingction_factor_end = data.Float("ExtingctionFactorEnd");
            extingction_factor_control_mode = data.Attr("ExtingctionFactorControlMode");
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
                    base_strength_tmp = Calc_tmpValue(x_proportion, base_strength_control_mode, base_strength_start, base_strength_end);
                    concentration_factor_tmp = Calc_tmpValue(x_proportion, concentration_factor_control_mode, concentration_factor_start, concentration_factor_end);
                    extingction_factor_tmp = Calc_tmpValue(x_proportion, extingction_factor_control_mode, extingction_factor_start, extingction_factor_end);      
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    base_strength_tmp = Calc_tmpValue(y_proportion, base_strength_control_mode, base_strength_start, base_strength_end);
                    concentration_factor_tmp = Calc_tmpValue(y_proportion, concentration_factor_control_mode, concentration_factor_start, concentration_factor_end);
                    extingction_factor_tmp = Calc_tmpValue(y_proportion, extingction_factor_control_mode, extingction_factor_start, extingction_factor_end);                   
                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    base_strength_tmp = base_strength_end;
                    concentration_factor_tmp = concentration_factor_end;
                    extingction_factor_tmp = extingction_factor_end;
                }
                List<Entity> GodLight2DList;
                if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(GodLight2D), out GodLight2DList) || GodLight2DList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in GodLight2DList)
                {
                    var item = entity_item as GodLight2D;
                    //控制器控制和它的标签相同且已经被激活的雾效，而且它们应该在同一个房间里
                    if (item.label == label && item.Active == true)
                    {
                        item.base_strength = base_strength_tmp;
                        item.concentration_factor = concentration_factor_tmp;
                        item.extingction_factor = extingction_factor_tmp;
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