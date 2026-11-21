using BasePlugin_LabAPI.Configs;
using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using NativeLimits = InventorySystem.Configs.InventoryLimits;

namespace BasePlugin_LabAPI.Features
{
    /// <summary>
    /// [无限弹药]：换弹请求到达时补足本次换弹所需的备弹，并允许换弹。
    /// 补给的弹药只用于这次换弹：换弹结束或被打断时，没用掉的部分会收回，不会留在背包里被丢出去。
    /// </summary>
    internal sealed class InfiniteAmmo : CustomEventsHandler
    {
        // 玩家 → (弹药类型 → 这次换弹临时补给、还没用掉的数量)
        private readonly Dictionary<uint, Dictionary<ItemType, int>> _granted = new();

        private static InfiniteAmmoConfig Config => MainClass.Config.UnlimitedAmmo;

        private static bool AppliesTo(Player player) =>
            Config.Enabled && (Config.Roles.Count == 0 || Config.Roles.Contains(player.Role));

        // 原版在服务器端校验换弹（MagazineModule.ReloadAuthorization：弹匣未满且备弹 > 0），
        // 校验结果作为 IsAllowed 传进这个事件；客户端不论有没有备弹都会发出换弹请求。
        public override void OnPlayerReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
        {
            Player player = ev.Player;
            FirearmItem firearm = ev.FirearmItem;

            // SCP-127、粒子分解器等没有可补充的弹药类型。
            if (!AppliesTo(player) || !NativeLimits.StandardAmmoLimits.ContainsKey(firearm.AmmoType))
            {
                return;
            }

            int needed = firearm.MaxAmmo - firearm.StoredAmmo;
            if (needed <= 0)
            {
                return;
            }

            int reserve = player.GetAmmo(firearm.AmmoType);
            int topUp = needed - reserve;
            if (topUp > 0)
            {
                player.AddAmmo(firearm.AmmoType, (ushort)Math.Min(topUp, ushort.MaxValue - reserve));
                Remember(player, firearm.AmmoType, topUp);
            }

            // 补足后重新走一遍原版校验，只放行“缺备弹”这一个原因；武器正忙（换弹中、检视中等）仍按原版拒绝。
            ev.IsAllowed = IReloadUnloadValidatorModule.ValidateReload(firearm.Base) && !firearm.Base.AnyModuleBusy();
            if (!ev.IsAllowed)
            {
                Reclaim(player);
            }

            Log.Debug(() => $"[无限弹药] {player.Nickname} 换弹 {firearm.Type}：弹匣 {firearm.StoredAmmo}/{firearm.MaxAmmo}，" +
                            $"原有备弹 {reserve}，补给 {Math.Max(0, topUp)}");
        }

        // 换弹完成或被打断（收枪）都会触发 Reloaded。
        public override void OnPlayerReloadedWeapon(PlayerReloadedWeaponEventArgs ev) => Reclaim(ev.Player);

        public override void OnPlayerDying(PlayerDyingEventArgs ev) => Reclaim(ev.Player);

        public override void OnPlayerDroppingAmmo(PlayerDroppingAmmoEventArgs ev) => Reclaim(ev.Player);

        public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev) => Reclaim(ev.Player);

        public override void OnPlayerLeft(PlayerLeftEventArgs ev) => _granted.Remove(ev.Player.NetworkId);

        public override void OnPlayerUnloadingWeapon(PlayerUnloadingWeaponEventArgs ev)
        {
            if (Config.BlockUnload && AppliesTo(ev.Player))
            {
                ev.IsAllowed = false;
            }
        }

        public void Reset() => _granted.Clear();

        private void Remember(Player player, ItemType ammo, int amount)
        {
            if (!_granted.TryGetValue(player.NetworkId, out Dictionary<ItemType, int>? perType))
            {
                perType = new Dictionary<ItemType, int>();
                _granted[player.NetworkId] = perType;
            }

            perType[ammo] = perType.TryGetValue(ammo, out int existing) ? existing + amount : amount;
        }

        // 弹药不分来源，收回时优先收回补给的部分，玩家自己的备弹留着。
        private void Reclaim(Player player)
        {
            if (!_granted.TryGetValue(player.NetworkId, out Dictionary<ItemType, int>? perType))
            {
                return;
            }

            _granted.Remove(player.NetworkId);

            foreach (KeyValuePair<ItemType, int> grant in perType)
            {
                int current = player.GetAmmo(grant.Key);
                int reclaim = Math.Min(current, grant.Value);
                if (reclaim > 0)
                {
                    player.SetAmmo(grant.Key, (ushort)(current - reclaim));
                }

                Log.Debug(() => $"[无限弹药] 收回 {player.Nickname} 未用完的 {grant.Key} × {reclaim}");
            }
        }
    }
}
