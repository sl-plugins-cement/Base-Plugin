using PlayerRoles;
using System.ComponentModel;

namespace BasePlugin_LabAPI.Configs
{
    public sealed class RoleConfig
    {
        [Description("是否启用 [角色属性] 功能")]
        public bool Enabled { get; set; } = true;

        [Description(
            "按角色（RoleTypeId，例如 ClassD、Scientist、NtfCaptain、ChaosRifleman、Scp173、Scp096、Scp0492）设置出生属性，未列出的角色保持原版。" +
            "数值填 -1 表示保持原版。字段：" +
            "max_health 最大血量；health 出生血量（-1 为回满）；" +
            "artificial_health 出生 AHP（0 为不给）、artificial_health_decay 每秒衰减（原版 1.2）、artificial_health_efficacy 抵挡伤害的比例（原版 0.7）、artificial_health_persistent 是否不会衰减到 0 后消失；" +
            "max_hume_shield 最大休谟护盾（人类角色也能拥有）、hume_shield 出生护盾值（-1 为回满）、hume_shield_regen_rate 每秒回复量、hume_shield_regen_cooldown 受伤后多少秒开始回复（后两项只对原版带动态护盾的 SCP 生效）；" +
            "clear_default_items 是否清空原版出生物品、items 额外给予的物品列表；clear_default_ammo 是否清空原版出生弹药、ammo 出生弹药（类型: 数量）；" +
            "effects 出生效果列表（name 效果名，如 MovementBoost、Scp1853；intensity 强度 1-255；duration 持续秒数，0 为永久）。" +
            "出生物品与弹药只在角色正常获得出生装备时生效（例如回合开始、刷新、撤离）")]
        public Dictionary<RoleTypeId, RoleSettings> Settings { get; set; } = new()
        {
            // 两个保持原版的示例条目，方便照着填写。
            [RoleTypeId.ClassD] = new RoleSettings(),
            [RoleTypeId.Scp173] = new RoleSettings(),
        };
    }

    public sealed class RoleSettings
    {
        public float MaxHealth { get; set; } = -1;

        public float Health { get; set; } = -1;

        public float ArtificialHealth { get; set; } = 0;

        public float ArtificialHealthDecay { get; set; } = 1.2f;

        public float ArtificialHealthEfficacy { get; set; } = 0.7f;

        public bool ArtificialHealthPersistent { get; set; } = false;

        public float MaxHumeShield { get; set; } = -1;

        public float HumeShield { get; set; } = -1;

        public float HumeShieldRegenRate { get; set; } = -1;

        public float HumeShieldRegenCooldown { get; set; } = -1;

        public bool ClearDefaultItems { get; set; } = false;

        public List<ItemType> Items { get; set; } = new();

        public bool ClearDefaultAmmo { get; set; } = false;

        public Dictionary<ItemType, ushort> Ammo { get; set; } = new();

        public List<EffectSettings> Effects { get; set; } = new();

        internal bool ChangesLoadout => ClearDefaultItems || ClearDefaultAmmo || Items.Count > 0 || Ammo.Count > 0;
    }

    public sealed class EffectSettings
    {
        public string Name { get; set; } = string.Empty;

        public byte Intensity { get; set; } = 1;

        public float Duration { get; set; } = 0;
    }
}
