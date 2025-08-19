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
    [CustomEntity("DBBHelper/GrainyBlurEffectControl")]
    public class GrainyBlurEffectControl : Entity
    {
        //------------------与控制特效相关------------------
        private Rectangle area = new Rectangle();//控制特效区域的范围
        private string area_control_mode = "Left_to_Right";//区域的控制方式
        private string label = "Default";//区域所控制的特效是哪一个
        //------------------可以更改的特效性质------------------
        private float blur_radius_start = 0.001f;
        private float blur_radius_end = 0.001f;
        private string blur_radius_control_mode = "Linear";
        private float blur_radius_tmp = 0.001f;
        public GrainyBlurEffectControl(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);

            area_control_mode = data.Attr("AreaControlMode");
            label = data.Attr("Label");

            blur_radius_start = data.Float("BlurRadiusStart");
            blur_radius_end = data.Float("BlurRadiusEnd");
            blur_radius_control_mode = data.Attr("BlurRadiusControlMode");
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
                    float time1 = DBBMath.MotionMapping(x_proportion, blur_radius_control_mode);
                    blur_radius_tmp = (float)DBBMath.Linear_Lerp(time1, blur_radius_start, blur_radius_end);
                }
                //从下到上模式
                else if (area_control_mode == "Bottom_to_Top")
                {
                    float time1 = DBBMath.MotionMapping(y_proportion, blur_radius_control_mode);
                    blur_radius_tmp = (float)DBBMath.Linear_Lerp(time1, blur_radius_start, blur_radius_end);

                }
                //立即数模式
                else if (area_control_mode == "Instant")
                {
                    blur_radius_tmp = blur_radius_end;
                }
                List<Entity> GrainyBlurList;
                if (!DBBCustomEntityManager.TrackEntityOf_SubType(typeof(GrainyBlurEffect), out GrainyBlurList) || GrainyBlurList.Count == 0)
                {
                    return;
                }
                foreach (var entity_item in GrainyBlurList)
                {
                    var item = entity_item as GrainyBlurEffect;
                    //控制器控制和它的标签相同且已经被激活的雾效，而且它们应该在同一个房间里
                    if (item.label == label && item.Active == true)
                    {
                        item.blur_radius = blur_radius_tmp;
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
            Draw.HollowRect(Position,area.Width,area.Height,Color.WhiteSmoke);
        }
    }
}