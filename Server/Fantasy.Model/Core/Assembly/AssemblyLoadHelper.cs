using System.Runtime.Loader;
using Fantasy.Core;
using Fantasy.Core.DataBase;

namespace Fantasy;

public static class AssemblyLoadHelper
{
    private const string HotfixDll = "Fantasy.Hotfix";
    private static AssemblyLoadContext? _assemblyLoadContext = null;

    public static void Initialize()
    {
        LoadModelDll();
        LoadHotfixDll();
    }

    public static void BindConfig()
    {
        // 框架需要一些的配置文件来启动服务器和创建网络服务所以需要ServerConfig.xlsx和MachineConfig.xlsx的配置
        // 由于配置表的代码是生成在框架外面的、框架没办法直接获取到配置文件
        // 考虑到这两个配置文件开发者可能会修改结构、所以提供了一个委托来让开发者开自己定义如何获取框架需要的东西
        // 本来想提供一个接口让玩家把ServerConfig和MachineConfig添加到框架中、但这样就不支持热更
        // 提供委托方式就可以支持配置表热更。因为配置表读取就是支持热更的
        // 虽然这块稍微麻烦点、但好在配置一次以后基本不会改动、后面有更好的办法我会把这个给去掉
        ConfigTableManage.ServerConfig = serverId =>
        {
            if (!ServerConfigData.Instance.TryGet(serverId, out var serverConfig))
            {
                return null;
            }

            return new ServerConfigInfo()
            {
                Id = serverConfig.Id,
                InnerPort = serverConfig.InnerPort,
                MachineId = serverConfig.MachineId
            };
        };
        ConfigTableManage.MachineConfig = machineId =>
        {
            if (!MachineConfigData.Instance.TryGet(machineId, out var machineConfig))
            {
                return null;
            }

            return new MachineConfigInfo()
            {
                Id = machineConfig.Id,
                OuterIP = machineConfig.OuterIP,
                OuterBindIP = machineConfig.OuterBindIP,
                InnerBindIP = machineConfig.InnerBindIP,
                ManagementPort = machineConfig.ManagementPort
            };
        };
        ConfigTableManage.WorldConfigInfo = worldId =>
        {
            if (!WorldConfigData.Instance.TryGet(worldId, out var worldConfig))
            {
                return null;
            }

            return new WorldConfigInfo()
            {
                Id = worldConfig.Id,
                WorldName = worldConfig.WorldName,
                DbConnection = worldConfig.DbConnection,
                DbName = worldConfig.DbName,
                DbType = worldConfig.DbType
            };
        };
        ConfigTableManage.SceneConfig = sceneId =>
        {
            if (!SceneConfigData.Instance.TryGet(sceneId, out var sceneConfig))
            {
                return null;
            }

            return new SceneConfigInfo()
            {
                Id = sceneConfig.Id,
                SceneType = sceneConfig.SceneType,
                Name = sceneConfig.Name,
                NetworkProtocol = sceneConfig.NetworkProtocol,
                RouteId = sceneConfig.RouteId,
                WorldId = sceneConfig.WorldId,
                OuterPort = sceneConfig.OuterPort
            };
        };
        ConfigTableManage.AllServerConfig = () =>
        {
            var list = new List<ServerConfigInfo>();

            foreach (var serverConfig in ServerConfigData.Instance.List)
            {
                list.Add(new ServerConfigInfo()
                {
                    Id = serverConfig.Id,
                    InnerPort = serverConfig.InnerPort,
                    MachineId = serverConfig.MachineId
                });
            }

            return list;
        };
        ConfigTableManage.AllMachineConfig = () =>
        {
            var list = new List<MachineConfigInfo>();

            foreach (var machineConfig in MachineConfigData.Instance.List)
            {
                list.Add(new MachineConfigInfo()
                {
                    Id = machineConfig.Id,
                    OuterIP = machineConfig.OuterIP,
                    OuterBindIP = machineConfig.OuterBindIP,
                    InnerBindIP = machineConfig.InnerBindIP,
                    ManagementPort = machineConfig.ManagementPort
                });
            }

            return list;
        };
        ConfigTableManage.AllSceneConfig = () =>
        {
            var list = new List<SceneConfigInfo>();

            foreach (var sceneConfig in SceneConfigData.Instance.List)
            {
                list.Add(new SceneConfigInfo()
                {
                    Id = sceneConfig.Id,
                    EntityId = sceneConfig.EntityId,
                    SceneType = sceneConfig.SceneType,
                    Name = sceneConfig.Name,
                    NetworkProtocol = sceneConfig.NetworkProtocol,
                    RouteId = sceneConfig.RouteId,
                    WorldId = sceneConfig.WorldId,
                    OuterPort = sceneConfig.OuterPort
                });
            }

            return list;
        };
    }

    private static void LoadModelDll()
    {
        AssemblyManager.Load(AssemblyName.Model, typeof(AssemblyLoadHelper).Assembly);
    }

    public static void LoadHotfixDll()
    {
        if (_assemblyLoadContext != null)
        {
            _assemblyLoadContext.Unload();
            System.GC.Collect();
        }

        _assemblyLoadContext = new AssemblyLoadContext(HotfixDll, true);
        var dllBytes = File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, $"{HotfixDll}.dll"));
        var pdbBytes = File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory, $"{HotfixDll}.pdb"));
        var assembly = _assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
        AssemblyManager.Load(AssemblyName.Hotfix, assembly);
    }
}