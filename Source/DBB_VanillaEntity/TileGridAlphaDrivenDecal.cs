using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Data.Common;
using System.Linq;
namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/TileGridAlphaDrivenDecal")]
    public class TileGridAlphaDrivenDecal : Entity
    {
        //以下为常规参数
        public MTexture decal = null;//装饰
        public float alpha = 1.0f;//透明度
        public Vector4 color = Vector4.One;//颜色
        public Vector2 scale = Vector2.One;//缩放
        public float rotation = 0.0f;//旋转, 角度制
        //以下与协变有关
        private bool first_detected = true;
        private TileGrid covariant_tilegrid = null;
        public float current_alpha = 1.0f;//当前decal的透明度，这个是最终用于显示的透明度
        public Vector2 detected_position=Vector2.Zero;//检测位置，decal应该随着哪个tilegrid的alpha变化而变化
        public TileGridAlphaDrivenDecal(EntityData data, Vector2 offset)
        {
            Position = data.Position + offset;
            detected_position = data.Nodes[0] + offset;
            //以下为基础参数
            decal = GFX.Game[data.String("Decal")];
            alpha = data.Float("Alpha", 1.0f);
            current_alpha = alpha;
            color = DBBMath.ConvertColor(data.String("Color", "FFFFFF"));
            scale = new Vector2(data.Float("ScaleX", 1.0f), data.Float("ScaleY", 1.0f));
            rotation = data.Float("Rotation", 0.0f);
            Depth = data.Int("Depth");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        public override void Update()
        {
            base.Update();
            //更新时截获
            if (first_detected)
            {
                first_detected = false;
                var eneity_list = Scene.Entities;
                //检测对应的墙体并截获其透明度
                for (int i = 0; i < eneity_list.Count; i++)
                {
                    if (eneity_list[i].Collidable && eneity_list[i].CollidePoint(detected_position))
                    {
                        var tile_grid = eneity_list[i].Components.Get<TileGrid>();
                        if (tile_grid != null)
                        {
                            covariant_tilegrid = tile_grid;
                            break;
                        }
                    }
                }
            }
            if (covariant_tilegrid != null)
            {
                current_alpha = alpha * covariant_tilegrid.Alpha;
            }
        }
        public override void Render()
        {
            //如果decal存在的话则绘制
            if (decal != null)
            {
                Color tmp_color = new Color(color) * current_alpha;
                decal.DrawCentered(Position, tmp_color, scale, MathHelper.ToRadians(rotation));
            }
        }

    }
    
    
}