using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Celeste.Mod.DBBHelper.Mechanism
{
    public class DBBGamePlayBuffers
    {
        //用于高清特效前景渲染
        public static Dictionary<string,VirtualRenderTarget> DBBRenderTargets = new Dictionary<string,VirtualRenderTarget>();
        
        //创建一个新的RenderTarget，并把它添加到DBBRenderTargets字典中
        public static void Create(string name,int width, int height,bool depth = false, bool preserve = true, int multiSampleCount = 0)
        { 
            if(string.IsNullOrEmpty(name))
            {
                Logger.Log(LogLevel.Warn,"DBBHelper/DBBGamePlayBuffers","Failed to create a RenderTarget,because its name is illegal.");
                return;
            }
            //如果名字已经存在，代表已经创建过该RenderTarget
            if(DBBRenderTargets.ContainsKey(name))
            {
                Logger.Log(LogLevel.Warn,"DBBHelper/DBBGamePlayBuffers","RenderTarget "+name+" has been created before.");
                return;
            }
            VirtualRenderTarget virtualRenderTarget=new VirtualRenderTarget("dbb-"+name, width, height, multiSampleCount, depth, preserve);
            DBBRenderTargets.Add(name,virtualRenderTarget);
            return;
        }
        public static void ReloadBuffer(string name, Vector2 width_and_height)
        {

            if (DBBRenderTargets.ContainsKey(name))
            {
                DBBRenderTargets[name].Width = (int)width_and_height.X;
                DBBRenderTargets[name].Height = (int)width_and_height.Y;
                DBBRenderTargets[name].Reload();
            }
        }
        //创建一张新的2D纹理
        public static Texture2D CreateTexture2D(int width, int height)
        {
            return new Texture2D(Engine.Instance.GraphicsDevice, width, height);
        }
        
        //卸载时删除所有存在于DBBRenderTargets的RenderTarget
        public static void Unload()
        {
            foreach (var item in DBBRenderTargets)
            {
                item.Value.Dispose();
            }
            DBBRenderTargets.Clear();
        }
    }
}