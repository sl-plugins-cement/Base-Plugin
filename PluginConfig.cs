using System.ComponentModel;

namespace BasePlugin_LabAPI
{
    public class PluginConfig
    {
        [Description("是否开启调试模式")]
        public bool Debug { get; set; } = false;

        [Description("是否启用 [无限弹药] 功能")]
        public bool InfiniteAmmo { get; set; } = true;
    }
}
