using BasePlugin_LabAPI.Configs;
using System.ComponentModel;
using YamlDotNet.Serialization;

namespace BasePlugin_LabAPI
{
    public sealed class PluginConfig
    {
        [Description("是否输出调试日志")]
        public bool Debug { get; set; } = false;

        [Description("[角色属性] 按角色修改出生时的血量、休谟护盾、AHP、出生物品、弹药与效果（SCP 与人类角色均可）")]
        public RoleConfig Roles { get; set; } = new();

        [Description("[背包上限] 覆盖原版的物品类别上限与弹药上限（会同步给客户端），并可限制单种物品的携带数量")]
        public InventoryLimitConfig InventoryLimits { get; set; } = new();

        [Description("[无限弹药] 换弹时自动补足本次换弹所需的备弹")]
        public InfiniteAmmoConfig UnlimitedAmmo { get; set; } = new();

        [Description("[便携钥匙] 钥匙卡放在背包里即可开门、开储物柜、解锁发电机与核弹按钮")]
        public PortableKeycardConfig PortableKeycard { get; set; } = new();

        [Description("[有益可乐] 调整 SCP-207 每秒造成的伤害")]
        public Scp207Config Scp207 { get; set; } = new();

        [Description("[击杀播报] 有人被击杀时向所有玩家滚动显示击杀信息（需要 HintServiceMeow）")]
        public KillFeedConfig KillFeed { get; set; } = new();

        // 1.x 版本的平铺配置项。读取旧配置后迁移到上面的分区，保存时不再写出。
        [YamlMember(Alias = "infinite_ammo", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public bool? LegacyInfiniteAmmo { get; set; }

        [YamlMember(Alias = "portable_key_card", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public bool? LegacyPortableKeyCard { get; set; }

        [YamlMember(Alias = "current_key_card_high", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public bool? LegacyCurrentKeyCardHigh { get; set; }

        [YamlMember(Alias = "beneficial_s_c_p207", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public bool? LegacyBeneficialScp207 { get; set; }

        [YamlMember(Alias = "kill_broadcast", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public bool? LegacyKillBroadcast { get; set; }

        /// <summary>
        /// 把 1.x 的平铺开关迁移到新分区。返回是否发生了迁移。
        /// </summary>
        internal bool MigrateLegacy()
        {
            bool migrated = false;

            if (LegacyInfiniteAmmo is bool infiniteAmmo)
            {
                UnlimitedAmmo.Enabled = infiniteAmmo;
                migrated = true;
            }

            if (LegacyPortableKeyCard is bool portable)
            {
                PortableKeycard.Enabled = portable;
                migrated = true;
            }

            if (LegacyCurrentKeyCardHigh is bool heldFirst)
            {
                PortableKeycard.HeldKeycardOnly = heldFirst;
                migrated = true;
            }

            if (LegacyBeneficialScp207 is bool beneficial)
            {
                Scp207.Enabled = beneficial;
                migrated = true;
            }

            if (LegacyKillBroadcast is bool killBroadcast)
            {
                KillFeed.Enabled = killBroadcast;
                migrated = true;
            }

            LegacyInfiniteAmmo = null;
            LegacyPortableKeyCard = null;
            LegacyCurrentKeyCardHigh = null;
            LegacyBeneficialScp207 = null;
            LegacyKillBroadcast = null;
            return migrated;
        }
    }
}
