using BasePlugin_LabAPI.Configs;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerStatsSystem;

namespace BasePlugin_LabAPI.Features
{
    /// <summary>
    /// [角色属性]：出生血量、休谟护盾、AHP、效果，以及出生物品与弹药。
    /// </summary>
    internal sealed class RoleStats : CustomEventsHandler
    {
        // 休谟护盾的最大值覆盖在换角色后仍会保留（HumeShieldStat 只在新角色没有护盾模块时才清除），
        // 所以记下被我们覆盖过的玩家，换到未配置的角色时把覆盖撤掉。
        private readonly HashSet<uint> _humeShieldOverridden = new();

        private static RoleConfig Config => MainClass.Config.Roles;

        private static bool TryGetSettings(RoleTypeId role, out RoleSettings settings)
        {
            settings = null!;
            return Config.Enabled && Config.Settings.TryGetValue(role, out settings) && settings != null;
        }

        // 原版在 OnSpawned 之后才按新角色重置血量和护盾（PlayerRoleManager.OnRoleChanged → StatBase.ClassChanged），
        // OnChangedRole 在这之后触发，是修改出生属性的最早时机。
        public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
        {
            Player player = ev.Player;
            RoleTypeId role = ev.NewRole.RoleTypeId;

            if (!TryGetSettings(role, out RoleSettings settings) || !player.IsAlive)
            {
                ClearHumeShieldOverride(player);
                return;
            }

            ApplyHealth(player, settings);
            ApplyArtificialHealth(player, settings);
            ApplyHumeShield(player, ev.NewRole, settings);
            ApplyEffects(player, settings);

            Log.Debug(() => $"[角色属性] {player.Nickname} 成为 {role}：血量 {player.Health}/{player.MaxHealth}，" +
                            $"AHP {player.ArtificialHealth}，护盾 {player.HumeShield}/{player.MaxHumeShield}");
        }

        public override void OnPlayerReceivingLoadout(PlayerReceivingLoadoutEventArgs ev)
        {
            if (!TryGetSettings(ev.Player.Role, out RoleSettings settings) || !settings.ChangesLoadout)
            {
                return;
            }

            if (settings.ClearDefaultItems)
            {
                ev.ClearItems();
            }

            foreach (ItemType item in settings.Items)
            {
                if (item != ItemType.None)
                {
                    ev.AddItem(item);
                }
            }

            if (settings.ClearDefaultAmmo)
            {
                ev.ClearAmmo();
            }

            foreach (KeyValuePair<ItemType, ushort> ammo in settings.Ammo)
            {
                ev.SetAmmo(ammo.Key, ammo.Value);
            }

            Log.Debug(() => $"[角色属性] {ev.Player.Nickname} 的出生物品：{string.Join(", ", ev.Items)}；弹药：" +
                            string.Join(", ", ev.Ammo.Select(x => $"{x.Key}={x.Value}")));
        }

        public override void OnPlayerLeft(PlayerLeftEventArgs ev) => _humeShieldOverridden.Remove(ev.Player.NetworkId);

        public void Reset() => _humeShieldOverridden.Clear();

        private static void ApplyHealth(Player player, RoleSettings settings)
        {
            if (settings.MaxHealth > 0)
            {
                player.MaxHealth = (float)settings.MaxHealth;
            }

            if (settings.Health > 0)
            {
                player.Health = Math.Min((float)settings.Health, player.MaxHealth);
            }
            else if (settings.MaxHealth > 0)
            {
                player.Health = player.MaxHealth;
            }
        }

        private static void ApplyArtificialHealth(Player player, RoleSettings settings)
        {
            if (settings.ArtificialHealth <= 0)
            {
                return;
            }

            AhpStat ahp = player.ReferenceHub.playerStats.GetModule<AhpStat>();
            ahp.ServerAddProcess(
                (float)settings.ArtificialHealth,
                (float)settings.ArtificialHealth,
                Math.Max(0f, (float)settings.ArtificialHealthDecay),
                Clamp01((float)settings.ArtificialHealthEfficacy),
                0f,
                settings.ArtificialHealthPersistent);
        }

        private void ApplyHumeShield(Player player, PlayerRoleBase role, RoleSettings settings)
        {
            if (role is not IHumeShieldedRole shieldedRole)
            {
                _humeShieldOverridden.Remove(player.NetworkId);
                return;
            }

            if (settings.MaxHumeShield >= 0)
            {
                player.MaxHumeShield = (float)settings.MaxHumeShield;
                _humeShieldOverridden.Add(player.NetworkId);
            }
            else
            {
                ClearHumeShieldOverride(player);
            }

            if (settings.HumeShield >= 0)
            {
                player.HumeShield = (float)(settings.MaxHumeShield >= 0
                    ? Math.Min(settings.HumeShield, settings.MaxHumeShield)
                    : settings.HumeShield);
            }
            else if (settings.MaxHumeShield >= 0)
            {
                player.HumeShield = (float)settings.MaxHumeShield;
            }

            // 回复速度与冷却只存在于原版的动态护盾控制器上；对象池在下次生成时会恢复它们的初始值。
            if (shieldedRole.HumeShieldModule is DynamicHumeShieldController controller)
            {
                if (settings.HumeShieldRegenRate >= 0)
                {
                    controller.RegenerationRate = (float)settings.HumeShieldRegenRate;
                }

                if (settings.HumeShieldRegenCooldown >= 0)
                {
                    controller.RegenerationCooldown = (float)settings.HumeShieldRegenCooldown;
                }
            }
        }

        private void ClearHumeShieldOverride(Player player)
        {
            if (!_humeShieldOverridden.Remove(player.NetworkId))
            {
                return;
            }

            // 负数会关闭覆盖，原版下一帧按角色重新计算最大护盾。
            player.ReferenceHub.playerStats.GetModule<HumeShieldStat>().MaxValue = -1f;
        }

        private static void ApplyEffects(Player player, RoleSettings settings)
        {
            foreach (EffectSettings effect in settings.Effects)
            {
                if (effect == null || effect.Intensity == 0)
                {
                    continue;
                }

                if (!player.TryGetEffect(effect.Name, out var status))
                {
                    Log.Warn($"[角色属性] 未知的效果名：{effect.Name}");
                    continue;
                }

                player.EnableEffect(status, effect.Intensity, Math.Max(0f, (float)effect.Duration));
            }
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
