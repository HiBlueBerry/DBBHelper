using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities {
    [CustomEntity("DBBHelper/ConditionalLightningBreakerBox")]
    public class ConditionalLightningBreakerBox:LightningBreakerBox
    {
        private string its_flag="";
        private bool permanent=false;
        private bool paramStoreInSession=false;
        private string param="";
        private float param_value=0.0f;

        public ConditionalLightningBreakerBox(EntityData e, Vector2 levelOffset):base(e,levelOffset)
        {
            its_flag=e.Attr("label");
            permanent=e.Bool("permanent");
            paramStoreInSession=e.Bool("param_session");
            param=e.Attr("param");
            param_value=e.Float("param_value");

            OnDashCollide=AnotherDashed;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (scene!=null&&(base.Scene as Level).Session.GetFlag(its_flag)&&permanent==true)
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
        }
        private void ConditionalBreak()
        {
            Session session = (base.Scene as Level).Session;
            RumbleTrigger.ManuallyTrigger(base.Center.X, 1.2f);
            base.Tag = Tags.Persistent;
            shakeCounter = 0f;
            shaker.On = false;
            sprite.Play("break");
            Collidable = false;
            DestroyStaticMovers();
            if(!session.GetFlag(its_flag))
            {
                session.SetFlag(its_flag);
            }
            if (paramStoreInSession)
            {
                if (!string.IsNullOrEmpty(music))
                {
                    session.Audio.Music.Event = SFX.EventnameByHandle(music);
                }
                //参数更新保存在小节中
                if (param!=null)
                {
                    if(param_value>=0.0f)
                    {
                        session.Audio.Music.Param(param,param_value);
                    }
                    
                }
                session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            else
            {
                if (!string.IsNullOrEmpty(music))
                {
                    Audio.SetMusic(SFX.EventnameByHandle(music), startPlaying: false);
                }
                if (param!=null)
                {
                    if(param_value>=0.0f)
                    {
                        session.Audio.Music.Param(param,param_value);
                    }
                }
                if (!string.IsNullOrEmpty(music) && Audio.CurrentMusicEventInstance != null)
                {
                    Audio.CurrentMusicEventInstance.start();
                }
            }
            if (pulseRoutine!=null)
            {
                pulseRoutine.Active = false;
            }
            //每敲一个电箱就加一个找特定标签的协程
            Add(new Coroutine(ConditionalLightning.ConditionalRemoveRoutine(SceneAs<Level>(),its_flag,base.RemoveSelf)));
        }
        public DashCollisionResults AnotherDashed(Player player, Vector2 dir)
        {
            if (!SaveData.Instance.Assists.Invincible)
            {
                if (dir == Vector2.UnitX && spikesLeft)
                {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == -Vector2.UnitX && spikesRight)
                {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == Vector2.UnitY && spikesUp)
                {
                    return DashCollisionResults.NormalCollision;
                }
                if (dir == -Vector2.UnitY && spikesDown)
                {
                    return DashCollisionResults.NormalCollision;
                }
            }
            (base.Scene as Level).DirectionalShake(dir);
            sprite.Scale = new Vector2(1f + Math.Abs(dir.Y) * 0.4f - Math.Abs(dir.X) * 0.4f, 1f + Math.Abs(dir.X) * 0.4f - Math.Abs(dir.Y) * 0.4f);
            health--;
            if (health > 0)
            {
                Add(firstHitSfx = new SoundSource("event:/new_content/game/10_farewell/fusebox_hit_1"));
                Celeste.Freeze(0.1f);
                shakeCounter = 0.2f;
                shaker.On = true;
                bounceDir = dir;
                bounce.Start();
                smashParticles = true;
                Pulse();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
            else
            {
                if (firstHitSfx != null)
                {
                    firstHitSfx.Stop();
                }
                Audio.Play("event:/new_content/game/10_farewell/fusebox_hit_2", Position);
                Celeste.Freeze(0.2f);
                player.RefillDash();
                ConditionalBreak();
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                SmashParticles(dir.Perpendicular());
                SmashParticles(-dir.Perpendicular());
            }
            return DashCollisionResults.Rebound;
        }
    }
}