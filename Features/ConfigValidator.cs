using BasePlugin_LabAPI.Configs;
using LabApi.Features.Wrappers;
using PlayerRoles;
using NativeLimits = InventorySystem.Configs.InventoryLimits;

namespace BasePlugin_LabAPI.Features
{
    /// <summary>
    /// 加载配置时检查明显填错的值，只提示不修改，运行时各功能会按各自的规则截断。
    /// </summary>
    internal static class ConfigValidator
    {
        private const int InventorySlots = 8;

        public static List<string> Validate(PluginConfig config)
        {
            List<string> warnings = new();

            foreach (KeyValuePair<RoleTypeId, RoleSettings> entry in config.Roles.Settings)
            {
                RoleSettings? settings = entry.Value;
                if (settings == null)
                {
                    warnings.Add($"roles.settings.{entry.Key} 为空");
                    continue;
                }

                if (settings.MaxHealth > 0 && settings.Health > settings.MaxHealth)
                {
                    warnings.Add($"roles.settings.{entry.Key}: health ({settings.Health}) 大于 max_health ({settings.MaxHealth})，会按 max_health 处理");
                }

                if (settings.MaxHumeShield >= 0 && settings.HumeShield > settings.MaxHumeShield)
                {
                    warnings.Add($"roles.settings.{entry.Key}: hume_shield ({settings.HumeShield}) 大于 max_hume_shield ({settings.MaxHumeShield})，会按 max_hume_shield 处理");
                }

                if (settings.Items.Count > InventorySlots)
                {
                    warnings.Add($"roles.settings.{entry.Key}: items 有 {settings.Items.Count} 个，背包只有 {InventorySlots} 格");
                }

                foreach (ItemType ammo in settings.Ammo.Keys)
                {
                    if (!NativeLimits.StandardAmmoLimits.ContainsKey(ammo))
                    {
                        warnings.Add($"roles.settings.{entry.Key}: ammo 里的 {ammo} 不是弹药类型");
                    }
                }

                foreach (EffectSettings effect in settings.Effects)
                {
                    if (effect == null || string.IsNullOrWhiteSpace(effect.Name))
                    {
                        warnings.Add($"roles.settings.{entry.Key}: effects 里有一项没有填写 name");
                    }
                }
            }

            foreach (KeyValuePair<ItemCategory, sbyte> limit in config.InventoryLimits.CategoryLimits)
            {
                if (limit.Value < 0 || limit.Value > InventorySlots)
                {
                    warnings.Add($"inventory_limits.category_limits.{limit.Key} = {limit.Value} 超出 0-{InventorySlots}，会被截断");
                }
            }

            foreach (ItemType ammo in config.InventoryLimits.AmmoLimits.Keys)
            {
                if (!NativeLimits.StandardAmmoLimits.ContainsKey(ammo))
                {
                    warnings.Add($"inventory_limits.ammo_limits 里的 {ammo} 不是弹药类型");
                }
            }

            foreach (KeyValuePair<ItemType, int> limit in config.InventoryLimits.ItemLimits)
            {
                if (limit.Value < 0)
                {
                    warnings.Add($"inventory_limits.item_limits.{limit.Key} = {limit.Value} 不能为负数");
                }
            }

            if (config.KillFeed.MaxLines < 1)
            {
                warnings.Add("kill_feed.max_lines 至少为 1");
            }

            return warnings;
        }
    }
}
