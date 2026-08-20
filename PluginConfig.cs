using System.ComponentModel;

namespace BasePlugin_LabAPI
{
    public class PluginConfig
    {
        [Description("是否启用插件")]
        public bool IsEnable { get; set; } = true;

        [Description("是否开启调试模式")]
        public bool Debug { get; set; } = false;
    }
}
