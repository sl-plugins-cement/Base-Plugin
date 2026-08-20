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
        public override string Description { get; } = "提供如 [无限子弹] 等基础功能";

        // 插件作者
        public override string Author { get; } = "create_xiaoyu";

        // 插件版本
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        // LabAPI 版本
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        // 配置类
        public PluginConfig Config;

        // 配置是否成功加载
        public bool IsSuccessLoadConfig = true;

        // 是否启用插件
        public bool IsEnable = true;

        // 插件启用
        public override void Enable()
        {
            // 判断配置是否成功加载
            if (!IsSuccessLoadConfig)
            {
                Logger.Error("配置文件加载失败，无法使用插件");
                return;
            }

            IsEnable = Config.IsEnable;

            if (!IsEnable)
            {
                Logger.Info("配置中未启用插件，停止加载");
                return;
            }

            Logger.Info("插件已加载");
        }

        // 插件禁用
        public override void Disable()
        {
            Logger.Info("插件已禁用");
        }

        // 加载配置
        public override void LoadConfigs()
        {
            base.LoadConfigs();

            IsSuccessLoadConfig = this.TryLoadConfig("config.yml", out Config);
        }
    }
}
