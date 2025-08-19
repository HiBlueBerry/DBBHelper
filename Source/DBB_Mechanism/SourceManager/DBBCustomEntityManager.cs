using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Meta;
using Monocle;
namespace Celeste.Mod.DBBHelper.Mechanism
{
    //自定义实体特性特性
    /// <summary>
    /// 一个用于标记需要自定义管理实体的特性
    /// <para>type_index为实体类别索引号，用于加载Load和UnLoad方法，索引号越低的类别的Load和UnLoad越先被加载</para>
    /// <para>require_load_unload为是否需要提供Load和UnLoad，如果为真则管理器会在加载时加载对应实体的Load和UnLoad方法</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DBBCustomEntityAttribute : Attribute
    {
        private bool require_load_unload = false;
        private int type_index;
        public bool RequireLoadUnLoad { get { return require_load_unload; } }
        public int TypeIndex { get { return type_index;}}
        public DBBCustomEntityAttribute(int type_index, bool require_load_unload = false)
        {
            this.require_load_unload = require_load_unload;
            this.type_index = type_index;
        }
    }

    public class DBBCustomEntityManager
    {
        //按照类别存储所有自定义实体
        private static Dictionary<Type, List<Entity>> AllEntityList = new Dictionary<Type, List<Entity>>();
        //存储所有实体的Load和Unload方法
        private static List<Tuple<int, Tuple<MethodInfo, MethodInfo>>> AllMethod = new List<Tuple<int, Tuple<MethodInfo, MethodInfo>>>();
        /// <summary>
        /// 初始化所有自定义实体
        /// </summary>
        private static void Init_CustomEntityList()
        {
            //获取当前程序集
            Assembly assembly = Assembly.GetExecutingAssembly();
            //寻找所有声明了DBBCustomEntity特性的Entity类型
            IEnumerable<Type> customEntityTypes = assembly.GetTypes().Where(
                t => t.IsDefined(typeof(DBBCustomEntityAttribute), false) && typeof(Entity).IsAssignableFrom(t)
            );
            //对于所有标记的自定义实体类型，为其创建一个新的列表
            foreach (var item in customEntityTypes)
            {
                DBBCustomEntityAttribute attr = item.GetCustomAttribute<DBBCustomEntityAttribute>();
                //检查是否需要Load/Unload
                if (attr.RequireLoadUnLoad)
                {
                    MethodInfo loadMethod = item.GetMethod("Load", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    MethodInfo unloadMethod = item.GetMethod("UnLoad", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (loadMethod == null || unloadMethod == null)
                    {
                        throw new InvalidOperationException(
                            $"{item.Name} need void Load() and void UnLoad() method!"
                        );
                    }
                    AllMethod.Add(new Tuple<int, Tuple<MethodInfo, MethodInfo>>(attr.TypeIndex, new Tuple<MethodInfo, MethodInfo>(loadMethod, unloadMethod)));
                }
                AllEntityList[item] = new List<Entity>();
            }
            //对AllMethod根据TypeIndex进行排序,升序排序
            AllMethod.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        }
        /// <summary>
        /// 销毁所有自定义实体
        /// </summary>
        private static void Destory_CustomEntityList()
        {
            AllEntityList.Clear();
            AllMethod.Clear();
        }
        //用于挂载到Level.UnloadLevel上
        private static void UnLoadCustomEntity(On.Celeste.Level.orig_UnloadLevel orig, Level self)
        {
            orig(self);
            //清空存储自定义实体的列表
            foreach (var item in AllEntityList)
            {
                item.Value.Clear();
            }
        }
        //用于挂载到Level.End上
        private static void EndCustomEntity(On.Celeste.Level.orig_End orig, Level self)
        {
            orig(self);
            //清空存储自定义实体的列表
            foreach (var item in AllEntityList)
            {
                item.Value.Clear();
            }
        }

        /// <summary>
        /// 向对应类型的实体列表中添加该实体，应在Entity.Added()中主动调用该函数
        /// </summary>
        public static void Added(Entity entity)
        {
            if (AllEntityList.TryGetValue(entity.GetType(), out var item))
            {
                item.Add(entity);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBCustomEntityManager", $"Not found {entity.GetType()} in EntityManager, please use DBBCustomEntityAttribute to label it first!");
            }
        }
        /// <summary>
        /// 向entity类型的基类的实体列表中添加该实体，应在Entity.Added()中主动调用该函数
        /// </summary>
        public static void Added_As_BaseType(Entity entity, Type base_type)
        {
            Type entity_type = entity.GetType();
            if (!base_type.IsAssignableFrom(entity_type))
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBCustomEntityManager", $"Can't convert {entity_type} as {base_type} ,please check your entity type!");
                return;
            }
            if (AllEntityList.TryGetValue(base_type, out var item))
            {
                item.Add(entity);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBCustomEntityManager", $"Not found {entity_type} as {base_type} in EntityManager, please use DBBCustomEntityAttribute to label it first!");
            }
        }
        /// <summary>
        /// 从列表中删除该实体，应在Entity.Removed()中主动调用该函数
        /// </summary>
        public static void Removed(Entity entity)
        {
            if (AllEntityList.TryGetValue(entity.GetType(), out var item))
            {
                if (item.Contains(entity))
                {
                    item.Remove(entity);
                    //Logger.Log(LogLevel.Warn, "asd", "removed");
                }
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBCustomEntityManager", $"Not found {entity.GetType()} in EntityManager, please use DBBCustomEntityAttribute to label it first!");
            }
        }
        /// <summary>
        /// 从该实体的列表或基类列表中删除该实体，应在Entity.Removed()中主动调用该函数
        /// </summary>
        public static void Removed_As_BaseType(Entity entity, Type base_type)
        {
            Type type = entity.GetType();
            //先看看是否能直接找到该类型的实体列表
            if (AllEntityList.TryGetValue(type, out var item))
            {
                if (item.Contains(entity))
                {
                    item.Remove(entity);
                }
            }
            //如果没有找到，则看看是否能找到该类型作为base_type的实体列表
            else if (base_type.IsAssignableFrom(type) && AllEntityList.TryGetValue(base_type, out item))
            {
                if (item.Contains(entity))
                {
                    item.Remove(entity);
                }
            }
            //如果都没有找到，则看看是否能找到该类型的基类的实体列表
            else if (AllEntityList.TryGetValue(type.BaseType, out item))
            {
                if (item.Contains(entity))
                {
                    item.Remove(entity);
                }
            }
            //如果还是没有找到，则输出警告
            else
            {
                Logger.Log(LogLevel.Warn, "DBBHelper/DBBCustomEntityManager", $"Not found {type} in EntityManager, please use DBBCustomEntityAttribute to label it first!");
            }
        }
        /// <summary>
        /// 获取被管理的某一类实体的列表
        /// </summary>
        public static bool TrackEntityList(Type type, out List<Entity> entity_list)
        {
            if (AllEntityList.ContainsKey(type))
            {
                entity_list = AllEntityList[type];
                return true;
            }
            else
            {
                entity_list = null;
                return false;
            }
        }
        /// <summary>
        /// 获取某几类实体的列表，其中得到的这些实体或者是type类型，或者是以type类型作为基类
        /// </summary>
        public static bool TrackEntityListAs_OneType(Type type, out List<Entity> entity_list)
        {
            entity_list = new List<Entity>();
            bool foundAny = false;
            foreach (var item in AllEntityList)
            {
                //注意这里是判断AllEntityList中的某一类能否转换成type，常见情况是：如果item的类别是继承自type类别，那么能够转换成type
                if (type.IsAssignableFrom(item.Key))
                {
                    entity_list.AddRange(item.Value);
                    foundAny = true;
                }
            }
            return foundAny;
        }
        /// <summary>
        /// 获取subType类的实体列表，其中这些实体以subType类或者其某个基类的形式存储在自定义实体管理器中
        /// </summary>
        public static bool TrackEntityOf_SubType(Type subType, out List<Entity> entity_list)
        {
            entity_list = new List<Entity>();
            bool foundAny = false;
            foreach (var item in AllEntityList)
            {
                //判断SubType能否转换为item对应的类型，如果不能则查找下一个item
                if (!item.Key.IsAssignableFrom(subType))
                {
                    continue;
                }
                //此时已经找到SubType的基类了
                foreach (var item1 in item.Value)
                {
                    //对应于该类的每一项，如果它确实是SubType的一个实例，则代表找到了一个SubType类型的实体
                    if (subType.IsInstanceOfType(item1))
                    {
                        entity_list.Add(item1);
                        foundAny = true;
                    }
                }
            }
            return foundAny;
        }
        /// <summary>
        /// 调用此函数以挂载管理器
        /// </summary>
        public static void Load()
        {
            //初始化所有带有自定义标签的实体
            Init_CustomEntityList();
            //挂载两个钩子，在场景切换时需要管理自定义实体
            On.Celeste.Level.UnloadLevel += new On.Celeste.Level.hook_UnloadLevel(UnLoadCustomEntity);
            On.Celeste.Level.End += new On.Celeste.Level.hook_End(EndCustomEntity);
            //对于每个自定义实体，如果它有自己的Load()函数，在加载时一并加载
            foreach (var item in AllMethod)
            {
                item.Item2.Item1.Invoke(null, null);
            }

        }
        /// <summary>
        /// 调用此函数以卸载管理器
        /// </summary>
        public static void UnLoad()
        {
            //对于每个自定义实体，如果它有自己的UnLoad()函数，在卸载时一并卸载
            foreach (var item in AllMethod)
            {
                item.Item2.Item2.Invoke(null, null);
            }
            //卸载两个用于管理场景切换时的实体的钩子
            On.Celeste.Level.UnloadLevel -= new On.Celeste.Level.hook_UnloadLevel(UnLoadCustomEntity);
            On.Celeste.Level.End -= new On.Celeste.Level.hook_End(EndCustomEntity);
            //清空所有字典
            Destory_CustomEntityList();
        }
        /// <summary>
        /// 在加载关卡时卸载实体的钩子
        /// </summary> 
        public static void UnLoadInLevel()
        {
            foreach (var item in AllMethod)
            {
                item.Item2.Item2.Invoke(null, null);
            }
        }
        /// <summary>
        /// 在加载关卡时按需卸载实体的钩子,ignore用于忽视某些实体钩子的加载，其为注册时实体的标号
        /// </summary>
        public static void LoadInLevel(List<int> ignore = null)
        {
            //如果忽略项为空，则直接加载所有实体钩子
            if (ignore == null || ignore.Count == 0)
            {
                foreach (var item in AllMethod)
                {
                    item.Item2.Item1.Invoke(null, null);
                }
            }
            //否则
            else
            {
                //先对ignore进行升序排序
                int index_count = ignore.Count;
                ignore.Sort((x, y) => x.CompareTo(y));
                int index = 0;

                //对于每一个实体，按照标号与ignore进行比较，相同的意味着不加载该实体的钩子
                for (int i = 0; i < AllMethod.Count; i++)
                {
                    //如果当前实体的标号已经大于当前ignore项目，则index加1，进行下一次判断
                    //如果index到达最后一项，则意味着不可能匹配ignore
                    while (index < index_count - 1 && i > ignore[index])
                    {
                        index++;
                    }
                    //此时能跳出循环，意味着i<=ingore[index]，如果二者相等，则不加载钩子
                    if (i == ignore[index])
                    {
                        continue;
                    }
                    //否则加载钩子
                    else
                    {
                        AllMethod[i].Item2.Item1.Invoke(null, null);
                    }
                }
            }
        }
    }
}