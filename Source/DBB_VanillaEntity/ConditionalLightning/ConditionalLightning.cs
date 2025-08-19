using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.DBBHelper.Entities {
    [CustomEntity("DBBHelper/ConditionalLightning")]
  
    class ConditionalLightning:Lightning
    {
        public string its_flag="";
        private bool permanent=false;
        public ConditionalLightning(EntityData data, Vector2 offset):base(data,offset)
        {
            its_flag=data.Attr("label");
            permanent=data.Bool("permanent");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if((Scene as Level).Session.GetFlag(its_flag)&&permanent==true)
            {
                List<ConditionalLightning> blocks = Scene.Entities.FindAll<ConditionalLightning>();
                foreach(var item in blocks)
                {
                    if(item==this)
                    {
                        blocks.Remove(item);
                        item.RemoveSelf();
                        break;
                    }
                }
            }
         
        }
         public override void Update()
         {

            if (Collidable && base.Scene.OnInterval(0.25f, toggleOffset))
            {
                ToggleCheck();
            }
            if (!Collidable && base.Scene.OnInterval(0.05f, toggleOffset))
            {
                ToggleCheck();
            }
            base.Update();
         }
        public static IEnumerator ConditionalRemoveRoutine(Level level,string label,Action onComplete=null)
        {
            List<ConditionalLightning> blocks = level.Entities.FindAll<ConditionalLightning>();
            foreach (ConditionalLightning item in new List<ConditionalLightning>(blocks))
            {
                if(item.its_flag==label&&(item.Scene as Level).Session.GetFlag(item.its_flag))
                {
                    item.disappearing = true;
                }
                if (item.Right<level.Bounds.Left||item.Bottom<level.Bounds.Top||item.Left>level.Bounds.Right||item.Top>level.Bounds.Bottom)
                {
                    item.disappearing = true;
                    blocks.Remove(item);
                    item.RemoveSelf();
                }
            }
            LightningRenderer entity=level.Tracker.GetEntity<LightningRenderer>();
            if(level.Entities.FindAll<Lightning>().Count==0)
            {
                entity.UpdateSeeds = false;
                entity.StopAmbience();
            }
            for (float t2=0.0f;t2<1.0f;t2+=Engine.DeltaTime*4.0f)
            {
                SetBreakValue(level,t2);
                yield return null;
            }
            SetBreakValue(level,1.0f);
            level.Shake();
            for(int num=blocks.Count-1;num>=0;num--)
            {
                blocks[num].Shatter();
            }
            for(float t2=0.0f;t2<1.0f;t2+=Engine.DeltaTime*8.0f)
            {
                SetBreakValue(level,1.0f-t2);
                yield return null;
            }
            SetBreakValue(level,0.0f);
            foreach (ConditionalLightning item2 in blocks)
            {
                if(item2.its_flag==label&&(item2.Scene as Level).Session.GetFlag(item2.its_flag))
                {
                    item2.RemoveSelf();
                }
            }
            FlingBird flingBird = level.Entities.FindFirst<FlingBird>();
            if (flingBird!=null)
            {
                flingBird.LightningRemoved=true;
            }
            onComplete?.Invoke();
        }

    }

}