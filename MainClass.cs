using BasePlugin_LabAPI.Features;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using System.Reflection;

namespace BasePlugin_LabAPI
{
    public class MainClass : Plugin
    {
        public override string Name { get; } = "BasePlugin";

        public override string Description { get; } = "服务器基础功能：角色属性、背包上限、无限弹药、便携钥匙、有益可乐、击杀播报";

        public override string Author { get; } = "create_xiaoyu";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        public static MainClass? Instance { get; private set; }

        // 配置加载失败时为默认配置，插件不会启用任何功能修改（见 Enable）。
        public static PluginConfig Config { get; private set; } = new();

        private bool _configLoaded;

        private readonly RoleStats _roleStats = new();
        private readonly InventoryLimits _inventoryLimits = new();
        private readonly InfiniteAmmo _infiniteAmmo = new();
        private readonly PortableKeycard _portableKeycard = new();
        private readonly HarmlessScp207 _harmlessScp207 = new();
        private readonly KillFeed _killFeed = new();

        private IEnumerable<CustomEventsHandler> Handlers => new CustomEventsHandler[]
        {
            _roleStats, _inventoryLimits, _infiniteAmmo, _portableKeycard, _harmlessScp207, _killFeed,
        };

        public override void Enable()
        {
            if (!_configLoaded)
            {
                Log.Error("配置文件加载失败，插件未启用。请检查 config.yml 的格式");
                return;
            }

            Instance = this;
            foreach (CustomEventsHandler handler in Handlers)
            {
                CustomHandlersManager.RegisterEventsHandler(handler);
            }

            _inventoryLimits.Enable();
            _killFeed.Enable();

            Log.Info($"插件已启用 v{Version}");
        }

        public override void Disable()
        {
            if (Instance != this)
            {
                return;
            }

            _killFeed.Disable();
            _inventoryLimits.Disable();

            foreach (CustomEventsHandler handler in Handlers)
            {
                CustomHandlersManager.UnregisterEventsHandler(handler);
            }

            _roleStats.Reset();
            _infiniteAmmo.Reset();
            Instance = null;
            Log.Info("插件已禁用");
        }

        public override void LoadConfigs()
        {
            base.LoadConfigs();
            _configLoaded = TryReloadConfig(out _);
        }

        /// <summary>
        /// 重新读取 config.yml。失败时保留当前配置。
        /// </summary>
        internal bool TryReloadConfig(out string message)
        {
            if (!this.TryLoadConfig("config.yml", out PluginConfig? config) || config == null)
            {
                message = "config.yml 读取失败，保留当前配置";
                return false;
            }

            if (config.MigrateLegacy())
            {
                this.SaveConfig(config, "config.yml");
                Log.Info("已将 1.x 版本的配置项迁移到新的分区格式");
            }

            List<string> warnings = ConfigValidator.Validate(config);
            foreach (string warning in warnings)
            {
                Log.Warn("[配置] " + warning);
            }

            Config = config;

            if (Instance == this)
            {
                _inventoryLimits.Reload();
                _killFeed.ApplyLayout();
            }

            message = warnings.Count == 0 ? "配置已重新加载" : $"配置已重新加载，有 {warnings.Count} 条警告，详见服务器控制台";
            return true;
        }
    }
}
