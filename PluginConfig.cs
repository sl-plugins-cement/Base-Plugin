using System.ComponentModel;

namespace BasePlugin_LabAPI
{
    public class PluginConfig
    {
        [Description("是否开启调试模式")]
        public bool Debug { get; set; } = false;

        [Description("是否启用 [无限弹药] 功能")]
        public bool InfiniteAmmo { get; set; } = true;

        [Description("是否启用 [便携钥匙] 功能")]
        public bool PortableKeyCard { get; set; } = true;

        [Description("[便携钥匙] 功能是否以玩家手持钥匙卡为高优先级")]
        public bool CurrentKeyCardHigh { get; set; } = true;
    }
}
