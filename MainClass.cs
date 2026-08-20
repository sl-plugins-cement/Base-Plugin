using BasePlugin_LabAPI.Events;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using System.Reflection;

namespace BasePlugin_LabAPI
{
    public class MainClass : Plugin
    {
        // 插件名字
        public override string Name { get; } = "BasePlugin";

        // 插件描述
        public override string Description { get; } = "提供如 [无限弹药] 等基础功能";

        // 插件作者
        public override string Author { get; } = "create_xiaoyu";

        // 插件版本
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        // LabAPI 版本
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        // 配置类
        public static PluginConfig Config { get; private set; }

        // 配置是否成功加载
        public bool IsSuccessLoadConfig = true;

        // 玩家事件
        public PlayerEvents PlayerEvent { get; } = new();

        // 插件启用
        public override void Enable()
        {
            // 判断配置是否成功加载
            if (!IsSuccessLoadConfig)
            {
                Logger.Error("配置文件加载失败，无法使用插件");
                return;
            }

            Logger.Debug("注册玩家事件", Config.Debug);
            CustomHandlersManager.RegisterEventsHandler(PlayerEvent);

            Logger.Info("插件已加载");
        }

        // 插件禁用
        public override void Disable()
        {
            Logger.Debug("注销玩家事件", Config.Debug);
            CustomHandlersManager.UnregisterEventsHandler(PlayerEvent);

            Logger.Info("插件已禁用");
        }

        // 加载配置
        public override void LoadConfigs()
        {
            base.LoadConfigs();

            IsSuccessLoadConfig = this.TryLoadConfig("config.yml", out PluginConfig config);
            Config = config;
        }
    }
}
