using BasePlugin_LabAPI.Configs;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using KeycardItem = LabApi.Features.Wrappers.KeycardItem;

namespace BasePlugin_LabAPI.Features
{
    /// <summary>
    /// [便携钥匙]：原版只检查手上的物品（DoorPermissionsPolicy.CheckPermissions），这里在原版拒绝后再检查背包里的钥匙卡。
    /// 门、储物柜、发电机、核弹按钮都实现了 IDoorPermissionRequester，统一用原版的权限策略判断，
    /// 所以“需要全部 / 任一权限”、自定义钥匙卡、地表通行证等规则都和手持时一致。
    /// </summary>
    internal sealed class PortableKeycard : CustomEventsHandler
    {
        private static PortableKeycardConfig Config => MainClass.Config.PortableKeycard;

        public override void OnPlayerInteractingDoor(PlayerInteractingDoorEventArgs ev)
        {
            if (ev.CanOpen || !Config.Doors || !ev.IsAllowed)
            {
                return;
            }

            DoorVariant door = ev.Door.Base;

            // 门上锁时原版会先用 CanOpen = false 触发一次本事件，询问插件是否无视锁（DoorVariant.TryResolveLock）。
            // 钥匙卡不能开锁住的门，所以只处理锁模式本身允许这次开 / 关的情况，那时触发的才是普通的权限判断。
            if (door.ActiveLocks > 0 && !LockAllowsToggle(door))
            {
                return;
            }

            if (TryUseInventoryKeycard(ev.Player, door, ev.Door.DoorName.ToString()))
            {
                ev.CanOpen = true;
            }
        }

        public override void OnPlayerInteractingLocker(PlayerInteractingLockerEventArgs ev)
        {
            if (ev.CanOpen || !Config.Lockers || !ev.IsAllowed)
            {
                return;
            }

            if (TryUseInventoryKeycard(ev.Player, ev.Chamber.Base, "Locker"))
            {
                ev.CanOpen = true;
            }
        }

        public override void OnPlayerUnlockingGenerator(PlayerUnlockingGeneratorEventArgs ev)
        {
            if (ev.CanOpen || !Config.Generators || !ev.IsAllowed)
            {
                return;
            }

            // 设置 CanOpen 而不是直接解锁，原版会继续处理冷却、Unlocked 事件和音效。
            if (TryUseInventoryKeycard(ev.Player, ev.Generator.Base, "Generator"))
            {
                ev.CanOpen = true;
            }
        }

        public override void OnPlayerUnlockingWarheadButton(PlayerUnlockingWarheadButtonEventArgs ev)
        {
            if (ev.IsAllowed || !Config.WarheadButton)
            {
                return;
            }

            AlphaWarheadActivationPanel panel = AlphaWarheadActivationPanel.Instance;
            if (panel != null && TryUseInventoryKeycard(ev.Player, panel, "AlphaWarheadButton"))
            {
                ev.IsAllowed = true;
            }
        }

        private static bool LockAllowsToggle(DoorVariant door)
        {
            DoorLockMode mode = DoorLockUtils.GetMode((DoorLockReason)door.ActiveLocks);
            return (mode.HasFlagFast(DoorLockMode.CanClose) && door.TargetState)
                || (mode.HasFlagFast(DoorLockMode.CanOpen) && !door.TargetState);
        }

        private static bool TryUseInventoryKeycard(Player player, IDoorPermissionRequester requester, string name)
        {
            if (!Config.Enabled || player.IsSCP || !player.IsAlive)
            {
                return false;
            }

            if (Config.HeldKeycardOnly && player.CurrentItem is KeycardItem)
            {
                Log.Debug(() => $"[便携钥匙] {player.Nickname} 手持钥匙卡，按原版只用手上的卡（{name}）");
                return false;
            }

            DoorPermissionsPolicy policy = requester.PermissionsPolicy;

            foreach (KeycardItem keycard in player.Items.OfType<KeycardItem>())
            {
                if (!policy.CheckPermissions(keycard.Base, requester, out PermissionUsed callback))
                {
                    continue;
                }

                // 一次性钥匙卡（地表通行证）靠这个回调标记为已使用并在之后自动销毁；普通钥匙卡的回调只是播放刷卡动画，
                // 卡不在手上时不需要。
                if (keycard.Base is SingleUseKeycardItem)
                {
                    callback?.Invoke(requester, true);
                }

                Log.Debug(() => $"[便携钥匙] {player.Nickname} 用背包里的 {keycard.Type} 打开 {name}（需要 {policy.RequiredPermissions}，" +
                                $"{(policy.RequireAll ? "全部" : "任一")}）");
                return true;
            }

            Log.Debug(() => $"[便携钥匙] {player.Nickname} 背包里没有能打开 {name} 的钥匙卡（需要 {policy.RequiredPermissions}）");
            return false;
        }
    }
}
