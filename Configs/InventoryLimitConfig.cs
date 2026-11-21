using System.ComponentModel;

namespace BasePlugin_LabAPI.Configs
{
    public sealed class InventoryLimitConfig
    {
        [Description("是否启用 [背包上限] 功能")]
        public bool Enabled { get; set; } = true;

        [Description(
            "物品类别上限（类别: 数量），覆盖 config_gameplay 的 limit_category_*，未列出的类别保持原版。" +
            "类别：Keycard、Medical、Radio、Firearm、Grenade、SCPItem、SpecialWeapon、Armor。数量 0-8（背包总共 8 格），护甲的加成仍会叠加在上面。" +
            "例：{ Medical: 4, Grenade: 3 }")]
        public Dictionary<ItemCategory, sbyte> CategoryLimits { get; set; } = new();

        [Description(
            "弹药上限（弹药类型: 数量，0 为不限），覆盖 config_gameplay 的 limit_ammo*，护甲的加成仍会叠加在上面。" +
            "类型：Ammo9x19、Ammo556x45、Ammo762x39、Ammo44cal、Ammo12gauge。例：{ Ammo9x19: 120 }")]
        public Dictionary<ItemType, ushort> AmmoLimits { get; set; } = new();

        [Description("单种物品的携带上限（物品: 数量），在拾取时检查。例：{ MicroHID: 1, SCP207: 2 }")]
        public Dictionary<ItemType, int> ItemLimits { get; set; } = new();

        [Description("达到单种物品上限时给玩家的提示，{item} 为物品名，{limit} 为上限")]
        public string ItemLimitMessage { get; set; } = "你最多只能携带 {limit} 个 {item}";
    }
}
