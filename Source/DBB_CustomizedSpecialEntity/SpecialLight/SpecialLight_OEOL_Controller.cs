using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.DBBHelper.Mechanism;

namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/SpecialLight_OEOL_Controller")]
    public class SpecialLight_OEOL_Controller : Entity
    {
        //控制区域的范围
        private Rectangle area = new Rectangle();
        //------------------可以更改的局内设置------------------
        private int specialLight_OnlyEnableOriginalLight = 0;

        public SpecialLight_OEOL_Controller(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            area = new Rectangle((int)Position.X, (int)Position.Y, data.Width, data.Height);
            //处理光效层启用情况
            string tmp_label = data.Attr("OnlyEnableOriginalLight", "Special and original light render");
            specialLight_OnlyEnableOriginalLight = tmp_label == "Special and original light render" ? 0 : 1;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        private void UpdateParameter()
        {
            if (Active == false || Scene == null)
            {
                return;
            }
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null || player.Dead)
            {
                return;
            }
            Vector2 offset = player.Position - Position;
            //获取X比例和Y比例，首先只有人物中心在区域之内才能有效果
            float x_proportion = offset.X / area.Width;
            float y_proportion = offset.Y / area.Height;
            if (x_proportion >= 0.0f && x_proportion <= 1.0f && y_proportion >= 0.0f && y_proportion <= 1.0f)
            {
                //告诉全局设置关卡内控制器已接管参数并设置参数值
                DBBSettings.SpecialLightMenu.OnlyEnableOriginalLight_InLevelControled = true;
                DBBGlobalSettingManager.SpecialLight_OnlyEnableOriginalLight = specialLight_OnlyEnableOriginalLight;
            }

        }
        public override void Update()
        {
            base.Update();
            UpdateParameter();
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Position, area.Width, area.Height, Color.DeepSkyBlue);
        }
    }
}