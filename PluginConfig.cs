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

        [Description("[便携钥匙] 功能 是否以玩家手持钥匙卡为高优先级")]
        public bool CurrentKeyCardHigh { get; set; } = true;

        [Description("是否启用 [有益可乐] 功能")]
        public bool BeneficialSCP207 { get; set; } = true;

        [Description("是否启用 [击杀播报] 功能")]
        public bool KillBroadcast { get; set; } = true;

        [Description("[击杀播报] 功能 显示文字")]
        public string KillBroadcastText = "<color=#98FB98><b>{attacker}</b></color><color=#98F5FF><b>击杀</b></color><color=#FF4040><b>{victim}</b></color>";

        [Description("[击杀播报] 功能 播报 Y 坐标")]
        public float KillBroadcastDisplayY = 180f;

        [Description("[击杀播报] 功能 播报文字大小")]
        public int KillBroadcastDisplaySize = 40;

        [Description("[击杀播报] 功能 播报显示时间")]
        public float KillBroadcastDisplayTime = 3f;
    }
}
