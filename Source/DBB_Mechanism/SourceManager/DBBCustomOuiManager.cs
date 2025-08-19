using System;
using System.Collections.Generic;
using System.Reflection;
using Monocle;
using System.Linq;
using System.Threading.Tasks;
using Celeste.Mod.Core;

namespace Celeste.Mod.DBBHelper.Mechanism
{
    //自定义OUI特性，用于注册
    /// <summary>
    /// 一个用于标记自定义OUI的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DBBCustomOuiAttribute : Attribute
    {
        public DBBCustomOuiAttribute()
        {
            
        }
    }
    public class DBBCustomOuiManager
    {
        //存储所有继承自OUI类的自定义类型
        private static List<Type> AllOuiType = new List<Type>();
        private static void Init_CustomOuiList()
        {
            //获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();
            //寻找所有声明了DBBCustomOuiAttribute特性的Oui类型
            IEnumerable<Type> customOuiTypes = assembly.GetTypes().Where
            (
                t => t.IsDefined(typeof(DBBCustomOuiAttribute), false) && typeof(Oui).IsAssignableFrom(t)
            );
            foreach (var item in customOuiTypes)
            {
                AllOuiType.Add(item);
            }
        }
        private static void Destory_CustomOuiList()
        {
            AllOuiType.Clear();
        }
        private static void ReloadCustomOui(On.Celeste.Overworld.orig_ReloadMenus orig, Overworld self, Overworld.StartMode startMode)
        {
            orig(self, startMode);
            if (Engine.Scene is Overworld)
            {
                foreach (Type type in AllOuiType)
                {
                    if (typeof(Oui).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        (Engine.Scene as Overworld).RegisterOui(type);
                    }
                }

            }
            else if (Engine.NextScene is Overworld)
            {
                foreach (Type type in AllOuiType)
                {
                    if (typeof(Oui).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        Oui oui = (Engine.NextScene as Overworld).RegisterOui(type);
                        if (oui.IsStart(Engine.NextScene as Overworld, startMode))
                        {
                            oui.Visible = true;
                            (Engine.NextScene as Overworld).Last = (Engine.NextScene as Overworld).Current = oui;
                        }
                    }
                }
            }
        }

        public static void Load()
        {
            //初始化所有带有自定义标签的Oui
            Init_CustomOuiList();
            //挂载钩子，使得游戏可以读取到自定义的OUI
            On.Celeste.Overworld.ReloadMenus += new On.Celeste.Overworld.hook_ReloadMenus(ReloadCustomOui);
        }
        public static void UnLoad()
        {
            //卸载钩子
            On.Celeste.Overworld.ReloadMenus -= new On.Celeste.Overworld.hook_ReloadMenus(ReloadCustomOui);
            Destory_CustomOuiList();
        }
    }
}