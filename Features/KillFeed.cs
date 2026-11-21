using BasePlugin_LabAPI.Configs;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace BasePlugin_LabAPI.Features
{
    /// <summary>
    /// [击杀播报]：所有玩家共用一个 HSM 提示，最近的几条击杀叠在一起显示，新的在最上面，到时间逐条消失。
    /// </summary>
    internal sealed class KillFeed : CustomEventsHandler
    {
        private const string HintId = "BasePlugin.KillFeed";

        private readonly List<(string Text, float ExpiresAt)> _lines = new();

        private readonly Hint _hint = new() { Id = HintId, Hide = true };

        private CoroutineHandle _expiry;

        private static KillFeedConfig Config => MainClass.Config.KillFeed;

        public void Enable()
        {
            ApplyLayout();
            foreach (Player player in Player.ReadyList)
            {
                Attach(player);
            }
        }

        public void Disable()
        {
            Timing.KillCoroutines(_expiry);
            _lines.Clear();
            foreach (Player player in Player.ReadyList)
            {
                if (player.IsPlayer)
                {
                    PlayerDisplay.Get(player).RemoveHint(_hint);
                }
            }
        }

        public void ApplyLayout()
        {
            _hint.YCoordinate = (float)Config.YCoordinate;
            _hint.XCoordinate = (float)Config.XCoordinate;
            _hint.Alignment = Config.Alignment;
            _hint.FontSize = Math.Max(1, Config.FontSize);
            Render();
        }

        // 玩家每次连接 HSM 都会新建显示对象，所以在加入时挂上共用提示。
        public override void OnPlayerJoined(PlayerJoinedEventArgs ev) => Attach(ev.Player);

        public override void OnServerWaitingForPlayers()
        {
            _lines.Clear();
            Render();
        }

        public override void OnPlayerDeath(PlayerDeathEventArgs ev)
        {
            if (!Config.Enabled)
            {
                return;
            }

            Player victim = ev.Player;
            Player? attacker = ev.Attacker;
            bool byPlayer = attacker != null && attacker != victim && !attacker.IsHost;

            string? line;
            if (byPlayer)
            {
                line = Config.Format
                    .Replace("{attacker}", AttackerName(attacker!))
                    .Replace("{attacker_role}", RoleName(attacker!.Role))
                    .Replace("{attacker_color}", RoleColor(attacker.Role));
            }
            else if (Config.ShowOtherDeaths)
            {
                line = Config.OtherDeathFormat;
            }
            else
            {
                return;
            }

            line = line
                .Replace("{victim}", Hints.Plain(victim.Nickname))
                .Replace("{victim_role}", RoleName(ev.OldRole))
                .Replace("{victim_color}", RoleColor(ev.OldRole));

            Push(line);
            Log.Debug(() => $"[击杀播报] {line}");
        }

        private static string AttackerName(Player attacker)
        {
            if (Config.HideScp3114Name && attacker.Role == RoleTypeId.Scp3114)
            {
                return "SCP-3114";
            }

            return Hints.Plain(attacker.Nickname);
        }

        private static string RoleName(RoleTypeId role) =>
            role.TryGetRoleTemplate(out PlayerRoleBase template) ? template.RoleName : role.ToString();

        private static string RoleColor(RoleTypeId role) =>
            role.TryGetRoleTemplate(out PlayerRoleBase template)
                ? "#" + ColorUtility.ToHtmlStringRGB(template.RoleColor)
                : "#FFFFFF";

        private void Attach(Player player)
        {
            if (player.IsPlayer)
            {
                PlayerDisplay.Get(player).AddHint(_hint);
            }
        }

        private void Push(string line)
        {
            _lines.Insert(0, (line, Time.time + Math.Max(0.5f, (float)Config.LineDuration)));

            int max = Math.Max(1, Config.MaxLines);
            if (_lines.Count > max)
            {
                _lines.RemoveRange(max, _lines.Count - max);
            }

            Render();

            if (!_expiry.IsRunning)
            {
                _expiry = Timing.RunCoroutine(ExpireLines());
            }
        }

        private IEnumerator<float> ExpireLines()
        {
            while (_lines.Count > 0)
            {
                yield return Timing.WaitForSeconds(0.25f);

                float now = Time.time;
                if (_lines.RemoveAll(x => x.ExpiresAt <= now) > 0)
                {
                    Render();
                }
            }
        }

        private void Render()
        {
            if (_lines.Count == 0)
            {
                _hint.Hide = true;
                return;
            }

            _hint.Text = string.Join("\n", _lines.Select(x => x.Text));
            _hint.Hide = false;
        }
    }
}
