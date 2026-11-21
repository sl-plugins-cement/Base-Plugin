using BasePlugin_LabAPI.Configs;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using Mirror;
using NativeLimits = InventorySystem.Configs.InventoryLimits;

namespace BasePlugin_LabAPI.Features
{
    /// <summary>
    /// [背包上限]：类别与弹药上限写进原版的 ServerConfigSynchronizer（客户端界面与原版拾取判定都读它），
    /// 单种物品上限在拾取时检查。
    /// </summary>
    internal sealed class InventoryLimits : CustomEventsHandler
    {
        private static InventoryLimitConfig Config => MainClass.Config.InventoryLimits;

        public void Enable()
        {
            ServerConfigSynchronizer.OnRefreshed += ApplySyncedLimits;
            ApplySyncedLimits();
        }

        public void Disable()
        {
            ServerConfigSynchronizer.OnRefreshed -= ApplySyncedLimits;
            RestoreVanilla();
        }

        /// <summary>
        /// 重新读取配置后调用：先恢复原版值，再套用新配置，这样从配置里删掉的条目会回到原版。
        /// </summary>
        public void Reload()
        {
            RestoreVanilla();
            ApplySyncedLimits();
        }

        private static void RestoreVanilla()
        {
            ServerConfigSynchronizer synchronizer = ServerConfigSynchronizer.Singleton;
            if (synchronizer == null || !NetworkServer.active)
            {
                return;
            }

            // 只刷新这两项，不触发 OnRefreshed，避免和 ApplySyncedLimits 互相调用。
            synchronizer.RefreshCategoryLimits();
            synchronizer.RefreshAmmoLimits();
        }

        // ServerConfigSynchronizer 在服务器启动、以及管理员执行 reload 命令时会按 config_gameplay 重写上限，
        // 之后触发 OnRefreshed，所以在这里重新套用配置。
        private static void ApplySyncedLimits()
        {
            ServerConfigSynchronizer synchronizer = ServerConfigSynchronizer.Singleton;
            if (synchronizer == null || !NetworkServer.active || !Config.Enabled)
            {
                return;
            }

            foreach (KeyValuePair<ItemCategory, sbyte> limit in Config.CategoryLimits)
            {
                int index = (int)limit.Key;
                if (index < 0 || index >= synchronizer.CategoryLimits.Count)
                {
                    continue;
                }

                // 原版用负数表示“上限为绝对值，但到达上限时不提示”（护甲、电台、特殊武器默认为 -1），保留这个约定。
                sbyte value = (sbyte)Math.Max(0, Math.Min(8, (int)limit.Value));
                bool silent = NativeLimits.StandardCategoryLimits.TryGetValue(limit.Key, out sbyte vanilla) && vanilla < 0;
                sbyte synced = silent ? (sbyte)-value : value;

                if (synchronizer.CategoryLimits[index] != synced)
                {
                    synchronizer.CategoryLimits[index] = synced;
                }
            }

            foreach (KeyValuePair<ItemType, ushort> limit in Config.AmmoLimits)
            {
                ServerConfigSynchronizer.AmmoLimit entry = new() { AmmoType = limit.Key, Limit = limit.Value };
                int index = IndexOfAmmo(synchronizer, limit.Key);

                if (index < 0)
                {
                    synchronizer.AmmoLimitsSync.Add(entry);
                }
                else if (synchronizer.AmmoLimitsSync[index].Limit != limit.Value)
                {
                    synchronizer.AmmoLimitsSync[index] = entry;
                }
            }

            Log.Debug(() => "[背包上限] 类别上限：" + string.Join(", ", synchronizer.CategoryLimits.Select((v, i) => $"{(ItemCategory)i}={v}")) +
                            "；弹药上限：" + string.Join(", ", synchronizer.AmmoLimitsSync.Select(x => $"{x.AmmoType}={x.Limit}")));
        }

        private static int IndexOfAmmo(ServerConfigSynchronizer synchronizer, ItemType ammo)
        {
            for (int i = 0; i < synchronizer.AmmoLimitsSync.Count; i++)
            {
                if (synchronizer.AmmoLimitsSync[i].AmmoType == ammo)
                {
                    return i;
                }
            }

            return -1;
        }

        public override void OnPlayerSearchingPickup(PlayerSearchingPickupEventArgs ev)
        {
            if (!Config.Enabled || ev.Pickup == null || !Config.ItemLimits.TryGetValue(ev.Pickup.Type, out int limit))
            {
                return;
            }

            ItemType type = ev.Pickup.Type;
            int carried = ev.Player.Items.Count(item => item.Type == type);
            if (carried < limit)
            {
                return;
            }

            ev.IsAllowed = false;
            Hints.Show(ev.Player, Config.ItemLimitMessage
                .Replace("{item}", type.ToString())
                .Replace("{limit}", limit.ToString()), 3f);

            Log.Debug(() => $"[背包上限] 拒绝 {ev.Player.Nickname} 拾取 {type}：已携带 {carried}，上限 {limit}");
        }
    }
}
