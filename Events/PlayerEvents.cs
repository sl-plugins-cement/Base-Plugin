using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace BasePlugin_LabAPI.Events
{
    public class PlayerEvents : CustomEventsHandler
    {
        // 玩家换弹前事件
        //
        // 无限弹药功能实现
        public override void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs eventArgs)
        {
            // 获取对象
            FirearmItem item = eventArgs.FirearmItem; // 武器对象
            Player player = eventArgs.Player; // 玩家对象

            // 判断功能是否启用
            if (!MainClass.Config.InfiniteAmmo)
            {
                Logger.Debug($"玩家: {player.Nickname} 触发 [无限弹药] 功能，但此功能未启用", MainClass.Config.Debug);
                return;
            }

            // 获取玩家库存弹药数量
            ushort AmmoInventory = player.GetAmmo(item.AmmoType);

            // 判断弹药数量是否小于武器最大容量 && 武器内的子弹数量是否小于武器的最大容量
            if (AmmoInventory < item.MaxAmmo && item.StoredAmmo < item.MaxAmmo)
            {
                // 为玩家添加弹药
                player.AddAmmo(item.AmmoType, (ushort)(item.MaxAmmo - item.StoredAmmo - AmmoInventory));

                // 强制允许换弹
                eventArgs.IsAllowed = true;

                // 输出调试日志
                Logger.Debug(
                    $"玩家: {player.Nickname} 触发 [无限弹药] 功能\n" +
                    $"换弹子弹类型: {item.AmmoType}\n" +
                    $"玩家持有数量: {AmmoInventory}\n" +
                    $"武器子弹数量: {item.StoredAmmo}\n" +
                    $"武器最大容量: {item.MaxAmmo}\n" +
                    $"添加子弹数量: {item.MaxAmmo - item.StoredAmmo - AmmoInventory}", MainClass.Config.Debug);
            }
        }
    }
}
