using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace BasePlugin_LabAPI.Utils
{
    public class PortableKeyCard
    {
        public static bool IsReturn(Player player, DoorPermissionFlags requiredPermissions, bool IsLocked)
        {
            // 判断插件是否启用
            if (!MainClass.Config.PortableKeyCard)
            {
                Logger.Debug($"玩家: {player.Nickname} 触发 [便携钥匙] 功能，但此功能未开启", MainClass.Config.Debug);
                return true;
            }

            // 排除 SCP 操作
            if (player.IsSCP)
            {
                return true;
            }

            // 是否玩家手持钥匙卡为高优先级
            if (MainClass.Config.CurrentKeyCardHigh)
            {
                if (player.CurrentItem != null && player.CurrentItem.Category == ItemCategory.Keycard)
                {
                    Logger.Debug($"玩家: {player.Nickname} 触发 [便携钥匙] 功能，但玩家主手已拿了钥匙卡", MainClass.Config.Debug);
                    return true;
                }
            }

            // 如果这扇门不需要钥匙 || 门是锁定的
            if (requiredPermissions == DoorPermissionFlags.None || IsLocked)
            {
                Logger.Debug($"玩家: {player.Nickname} 触发 [便携钥匙] 功能，但该门不需要钥匙或是锁定状态。", MainClass.Config.Debug); // 输出调试消息
                return true;
            }

            return false;
        }

        public static bool IsOpen(Player player, DoorPermissionFlags requiredPermissions, string doorName = "Unknown", bool IsOpened = false, bool IsLocked = false)
        {
            // 排除 ScpOverride 权限
            requiredPermissions &= ~DoorPermissionFlags.ScpOverride;

            // 输出调试消息
            Logger.Debug(
                    $"玩家: {player.Nickname} 完整触发 [便携钥匙] 功能\n" +
                    $"被操作门类型: {doorName}\n" +
                    $"被操作门权限: {requiredPermissions}\n" +
                    $"被操作门状态:\n" +
                    $"开启: {IsOpened}\n" +
                    $"锁定: {IsLocked}", MainClass.Config.Debug);

            // 获取玩家背包里所有的钥匙卡
            IEnumerable<KeycardItem> keycards = player.Items.OfType<KeycardItem>();

            Logger.Debug($"背包中的权限卡数量: {keycards.Count()}", MainClass.Config.Debug);

            // 遍历所有钥匙卡
            foreach (KeycardItem item in keycards)
            {
                Logger.Debug(
                    $"权限卡 {item.Type}\n" +
                    $"卡权限: {item.Permissions} ({(int)item.Permissions})\n" +
                    $"门权限: {requiredPermissions} ({(int)requiredPermissions})\n" +
                    $"AND结果: {(item.Permissions & requiredPermissions)} ({(int)(item.Permissions & requiredPermissions)})\n" +
                    $"是否满足: {(item.Permissions & requiredPermissions) == requiredPermissions}",MainClass.Config.Debug);

                // 判断钥匙卡的权限是否足够
                if ((item.Permissions & requiredPermissions) == requiredPermissions)
                {
                    return true;
                    // 对于 A B 大门的权限需要额外判断
                } else if (doorName == "EzGateA" || doorName == "EzGateB")
                {
                    // 兼容地表通行证
                    if (item.Type == ItemType.SurfaceAccessPass)
                    {
                        player.RemoveItem(item);
                        return true;
                    }

                    if ((item.Permissions & DoorPermissionFlags.ExitGates) == DoorPermissionFlags.ExitGates)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsOpenDoor(
            Player player,
            DoorPermissionsPolicy permissionsPolicy,
            IDoorPermissionRequester requester,
            string doorName = "Unknown",
            bool IsOpened = false,
            bool IsLocked = false)
        {
            DoorPermissionFlags requiredPermissions = permissionsPolicy.RequiredPermissions;

            Logger.Debug(
                $"玩家: {player.Nickname} 完整触发 [便携钥匙] 功能\n" +
                $"被操作门类型: {doorName}\n" +
                $"被操作门权限: {requiredPermissions}\n" +
                $"权限策略: {(permissionsPolicy.RequireAll ? "需要全部权限" : "满足任一权限")}\n" +
                $"被操作门状态:\n" +
                $"开启: {IsOpened}\n" +
                $"锁定: {IsLocked}", MainClass.Config.Debug);

            IEnumerable<KeycardItem> keycards = player.Items.OfType<KeycardItem>();

            Logger.Debug($"背包中的权限卡数量: {keycards.Count()}", MainClass.Config.Debug);

            foreach (KeycardItem item in keycards)
            {
                DoorPermissionFlags contextualPermissions = item.Base.GetPermissions(requester);
                bool isAllowed = permissionsPolicy.CheckPermissions(item.Base, requester, out _);

                Logger.Debug(
                    $"权限卡 {item.Type}\n" +
                    $"卡权限: {contextualPermissions} ({(int)contextualPermissions})\n" +
                    $"门权限: {requiredPermissions} ({(int)requiredPermissions})\n" +
                    $"权限策略: {(permissionsPolicy.RequireAll ? "全部" : "任一")}\n" +
                    $"是否满足: {isAllowed}", MainClass.Config.Debug);

                if (isAllowed)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
