using PlayerRoles;
using System.ComponentModel;

namespace BasePlugin_LabAPI.Configs
{
    public sealed class RoleConfig
    {
        [Description("是否启用 [角色属性] 功能")]
        public bool Enabled { get; set; } = true;

        [Description("按角色设置出生属性，键为角色名（ClassD、Scientist、FacilityGuard、NtfCaptain、ChaosRifleman、Scp173、Scp096、Scp0492 等），未列出的角色保持原版；各字段填 -1 表示保持原版")]
        public Dictionary<RoleTypeId, RoleSettings> Settings { get; set; } = new()
        {
            // 两个保持原版的示例条目，方便照着填写。
            [RoleTypeId.ClassD] = new RoleSettings(),
            [RoleTypeId.Scp173] = new RoleSettings(),
        };
    }

    // 出生物品与弹药只在角色获得原版出生装备时生效（回合开始、刷新、撤离等）。
    public sealed class RoleSettings
    {
        [Description("最大血量")]
        public decimal MaxHealth { get; set; } = -1;

        [Description("出生血量，-1 为回满")]
        public decimal Health { get; set; } = -1;

        [Description("出生 AHP，0 为不给")]
        public decimal ArtificialHealth { get; set; } = 0;

        [Description("AHP 每秒衰减量（原版 1.2）")]
        public decimal ArtificialHealthDecay { get; set; } = 1.2m;

        [Description("AHP 抵挡伤害的比例，0-1（原版 0.7）")]
        public decimal ArtificialHealthEfficacy { get; set; } = 0.7m;

        [Description("AHP 衰减到 0 后是否保留（可被重新补充）")]
        public bool ArtificialHealthPersistent { get; set; } = false;

        [Description("最大休谟护盾，人类角色也能拥有")]
        public decimal MaxHumeShield { get; set; } = -1;

        [Description("出生休谟护盾值，-1 为回满")]
        public decimal HumeShield { get; set; } = -1;

        [Description("休谟护盾每秒回复量（只对原版会回复护盾的 SCP 生效）")]
        public decimal HumeShieldRegenRate { get; set; } = -1;

        [Description("受伤后多少秒开始回复护盾（只对原版会回复护盾的 SCP 生效）")]
        public decimal HumeShieldRegenCooldown { get; set; } = -1;

        [Description("是否清空原版出生物品")]
        public bool ClearDefaultItems { get; set; } = false;

        [Description("额外给予的出生物品")]
        public List<ItemType> Items { get; set; } = new();

        [Description("是否清空原版出生弹药")]
        public bool ClearDefaultAmmo { get; set; } = false;

        [Description("出生弹药（弹药类型: 数量），会覆盖原版的同类弹药数量")]
        public Dictionary<ItemType, ushort> Ammo { get; set; } = new();

        [Description("出生效果：name 效果名（如 MovementBoost、Scp1853），intensity 强度 1-255，duration 持续秒数（0 为永久）")]
        public List<EffectSettings> Effects { get; set; } = new();

        internal bool ChangesLoadout => ClearDefaultItems || ClearDefaultAmmo || Items.Count > 0 || Ammo.Count > 0;
    }

    public sealed class EffectSettings
    {
        public string Name { get; set; } = string.Empty;

        public byte Intensity { get; set; } = 1;

        public decimal Duration { get; set; } = 0;
    }
}
