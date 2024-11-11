#if FANTASY_NET
using System.Reflection;
using CommandLine;
using Fantasy.Assembly;
using Fantasy.Async;
using Fantasy.Helper;
using Fantasy.Network;
using Fantasy.Serialize;

// ReSharper disable FunctionNeverReturns

namespace Fantasy.Platform.Net;

/// <summary>
/// Fantasy.Net 应用程序入口
/// </summary>
/// <exception cref="Exception">当命令行格式异常时抛出。</exception>
/// <exception cref="NotSupportedException">不支持的 ProcessType 类型异常。</exception>
public static class Entry
{
    /// <summary>
    /// 框架初始化
    /// </summary>
    /// <param name="assemblies"></param>
    public static void Initialize(params System.Reflection.Assembly[] assemblies)
    {
        // 解析命令行参数（笔记:CommandLineOptions通过注解实现: 想要读取的参数）
        Parser.Default.ParseArguments<CommandLineOptions>(Environment.GetCommandLineArgs())
            .WithNotParsed(error => throw new Exception("Command line format error!"))
            .WithParsed(option =>
            {
                // 笔记:得到命令行参数
                ProcessDefine.Options = option;

                // 笔记:默认是Tcp
                ProcessDefine.InnerNetwork = Enum.Parse<NetworkProtocolType>(option.InnerNetwork);
            });

        // 初始化Log系统
        Log.Initialize();

        // 检查启动参数,后期可能有机器人等不同的启动参数
        switch (ProcessDefine.Options.ProcessType)
        {
            case "Game":
            {
                break;
            }
            default:
            {
                throw new NotSupportedException($"ProcessType is {ProcessDefine.Options.ProcessType} Unrecognized!");
            }
        }

        // 初始化程序集管理系统
        AssemblySystem.Initialize(assemblies);

        // 初始化序列化
        SerializerManager.Initialize();

        // 精度处理（只针对Windows下有作用、其他系统没有这个问题、一般也不会用Windows来做服务器的）
        WinPeriod.Initialize();
    }


    /// <summary>
    /// 启动Fantasy.Net
    /// </summary>
    public static async FTask Start()
    {
        // 启动Process(理解：其实是Process配置中所有的Scene, 只不过指定了下)
        StartProcess().Coroutine();

        await FTask.CompletedTask;

        while (true)
        {
            // 理解：执行所有Scene的Update方法
            ThreadScheduler.Update();

            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// 初始化并且启动框架
    /// </summary>
    /// <param name="assemblies"></param>
    public static async FTask Start(params System.Reflection.Assembly[] assemblies)
    {
        Initialize(assemblies);
        await Start();
    }

    private static async FTask StartProcess()
    {
        // step1:如果启动时指定了分组,则只启动带指定分组的进程
        if (ProcessDefine.Options.StartupGroup != 0)
        {
            foreach (var processConfig in ProcessConfigData.Instance.ForEachByStartupGroup((uint)ProcessDefine.Options.StartupGroup))
            {
                await Process.Create(processConfig.Id);
            }

            return;
        }


        switch (ProcessDefine.Options.Mode)
        {
            // step2: Develop模式下，启动所有进程id的进程，相当于多合1
            case "Develop":
            {
                foreach (var processConfig in ProcessConfigData.Instance.List)
                {
                    await Process.Create(processConfig.Id);
                }

                return;
            }

            // step3: Release模式下, 只启动指定的进程id的进程
            case "Release":
            {
                await Process.Create(ProcessDefine.Options.ProcessId);
                return;
            }
        }
    }

    /// <summary>
    /// 关闭 Fantasy
    /// </summary>
    public static void Close()
    {
        AssemblySystem.Dispose();
        SerializerManager.Dispose();
    }
}
#endif