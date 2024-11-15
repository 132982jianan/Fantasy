﻿using Fantasy.Helper;
using Fantasy.Platform.Net;

// step1:获取配置文件. 比如通过远程获取这个配置文件，这样可以多组服务器共享一套配置了
var machineConfigText = await FileHelper.GetTextByRelativePath("../../../Config/Json/Server/MachineConfigData.Json");
var processConfigText = await FileHelper.GetTextByRelativePath("../../../Config/Json/Server/ProcessConfigData.Json");
var worldConfigText = await FileHelper.GetTextByRelativePath("../../../Config/Json/Server/WorldConfigData.Json");
var sceneConfigText = await FileHelper.GetTextByRelativePath("../../../Config/Json/Server/SceneConfigData.Json");

// step2:初始化配置文件. 如果重复初始化方法会覆盖掉上一次的数据，非常适合热重载时使用(笔记:这一步是使用string的扩展方法,和泛型从而实现反序列化)
MachineConfigData.Initialize(machineConfigText);
ProcessConfigData.Initialize(processConfigText);
WorldConfigData.Initialize(worldConfigText);
SceneConfigData.Initialize(sceneConfigText);

// step3:注册日志模块到框架(这个是调用的ILog接口,从而实现)
// 开发者可以自己注册日志系统到框架，只要实现Fantasy.ILog接口就可以。
// 这里用的是NLog日志系统注册到框架中。
Fantasy.Log.Register(new Fantasy.NLog("Server"));

// step4:初始化框架，添加程序集到框架中（笔记:AssemblyHelper是在Entity程序集中，因此初始化的就是这个Entity所在的程序集）
Fantasy.Platform.Net.Entry.Initialize(Fantasy.AssemblyHelper.Assemblies);

// step5:启动Fantasy.Net
await Fantasy.Platform.Net.Entry.Start();