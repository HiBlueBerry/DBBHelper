using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Data.Common;


namespace Celeste.Mod.DBBHelper.Entities
{
    [CustomEntity("DBBHelper/DBBFadeWall")]
    public class DBBFadeWall : Entity
    {
        private bool current_check = false;

        private char fillTile;

        private TileGrid tiles;

        private EffectCutout cutout;

        private float start_alpha=1.0f;
        private float end_alpha = 0.0f;
        private float current_alpha = 1.0f;
        private float time = 0.0f;
        private float fade_velocity = 1.0f;

        public DBBFadeWall(EntityData data, Vector2 offset)
        {
            this.Position = data.Position + offset;
            fillTile = data.Char("tiletype", '3');

            start_alpha = data.Float("StartAlpha");
            end_alpha = data.Float("EndAlpha");
            current_alpha = start_alpha;
            fade_velocity = data.Float("FadeVelocity");

            base.Collider = new Hitbox(data.Width, data.Height);
            base.Depth = -14002;
            Add(cutout = new EffectCutout());
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)base.X / 8 - tileBounds.Left;
            int y = (int)base.Y / 8 - tileBounds.Top;
            tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;

            Add(tiles);
            Add(new TileInterceptor(tiles, highPriority: false));

            TransitionListener val = new TransitionListener();
            //出场时应渐退
            val.OnOut = delegate (float f)
            {
                tiles.Alpha = (float)DBBMath.Linear_Lerp(f, current_alpha, start_alpha);
                cutout.Alpha = tiles.Alpha;
            };
            Add(val);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Update()
        {
            base.Update();

            Player player = CollideFirst<Player>();

            if (player != null && !player.Dead)
            {
                current_check = true;
            }
            else
            {
                current_check = false;
            }
            //只要在物体里面，就尽可能去接近end_alpha
            if (current_check == true)
            {
                time += fade_velocity * Engine.DeltaTime;
                if (time > 1.0f)
                {
                    time = 1.0f;
                }
            }
            //只要在物体外面，就尽可能去接近start_alpha
            else
            {
                time -= fade_velocity * Engine.DeltaTime;
                if (time < 0.0f)
                {
                    time = 0.0f;
                }
            }

            tiles.Alpha = (float)DBBMath.Linear_Lerp(time, start_alpha, end_alpha);
            current_alpha = tiles.Alpha;
            cutout.Alpha = current_alpha;
            //Logger.Log(LogLevel.Warn,"asd",time.ToString());

        }
        public override void Render()
        {
            tiles.Render();
        }

    }
    
    
}