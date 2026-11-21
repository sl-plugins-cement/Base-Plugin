using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using PlayerStatsSystem;

namespace BasePlugin_LabAPI.Features
{
    /// <summary>
    /// [有益可乐]：按倍率缩放 SCP-207 每秒造成的伤害，倍率为 0 时完全取消。
    /// </summary>
    internal sealed class HarmlessScp207 : CustomEventsHandler
    {
        public override void OnPlayerHurting(PlayerHurtingEventArgs ev)
        {
            if (!MainClass.Config.Scp207.Enabled
                || ev.DamageHandler is not UniversalDamageHandler handler
                || handler.TranslationId != DeathTranslations.Scp207.Id)
            {
                return;
            }

            float multiplier = Math.Max(0f, (float)MainClass.Config.Scp207.DamageMultiplier);
            if (multiplier <= 0f)
            {
                ev.IsAllowed = false;
                return;
            }

            handler.Damage *= multiplier;
        }
    }
}
